using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;
using System.Text;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using DrawnEndProfile = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.EndProfile;
using GeometryPoint = Tellurian.Trains.Schedules.Model.Geometry.Point;
using PlateShape = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.PlateShape;
using ExternalReferences = Tellurian.Trains.Schedules.Model.Layouts.ExternalReferences;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The drawing of a module end profile: only an end profile file from Module Draw is accepted, not one that belongs to another end
/// profile, and only from global administrators. Every signed-in user gets it, with the Registry's id written into it. The end
/// profile's description is saved with the form; its documents are not.
/// </summary>
[TestClass]
public sealed class EndProfileDrawingTests
{
    private const int Sweden = 1;
    private const int PersonId = 10;
    private const int EndProfileId = 1, OtherEndProfileId = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(bool isGlobalAdministrator)
    {
        List<Claim> claims =
        [
            new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
            new Claim(AppClaimTypes.UserId, PersonId.ToString()),
            new Claim(AppClaimTypes.PersonId, PersonId.ToString()),
            new Claim(AppClaimTypes.CountryId, Sweden.ToString()),
        ];
        if (isGlobalAdministrator) claims.Add(new Claim(AppClaimTypes.GlobalAdministrator, "true"));
        return new(new ClaimsIdentity(claims, "Test"));
    }

    private static ClaimsPrincipal Administrator => Principal(isGlobalAdministrator: true);
    private static ClaimsPrincipal User => Principal(isGlobalAdministrator: false);

    private static byte[] EndProfileFile(int? registryEndProfileId = null)
    {
        var profile = new DrawnEndProfile { Id = Guid.NewGuid(), Designation = "Drawn B96", Scale = "H0" };
        if (registryEndProfileId is int id) profile.ExternalReferences = ExternalReferences.WithModuleRegistryId(null, ExternalReferences.EndProfileId, id);
        return Encoding.UTF8.GetBytes(new EndProfileExporter().Write(profile, ExportOptions.Default));
    }

    /// <summary>An end profile file with a rectangular end plate: <paramref name="width"/> wide, from rail top down <paramref name="height"/>.</summary>
    private static byte[] EndProfileFileWithPlate(double width, double height)
    {
        var plate = new PlateShape([new GeometryPoint(-width / 2, 0), new GeometryPoint(width / 2, 0)], -height);
        var profile = new DrawnEndProfile { Id = Guid.NewGuid(), Designation = "Drawn B96", Scale = "H0", Plate = plate, Outline = plate.Outline(), GroundLine = plate.GroundLine() };
        return Encoding.UTF8.GetBytes(new EndProfileExporter().Write(profile, ExportOptions.Default));
    }

    private async Task<(int? Width, int? Height)> StoredMeasuresAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        return await dbContext.ModuleEndProfiles.Where(e => e.Id == EndProfileId)
            .Select(e => new ValueTuple<int?, int?>(e.WidthMillimeter, EF.Property<int?>(e, "HeightMillimeter")))
            .SingleAsync();
    }

    private static DrawnEndProfile Read(byte[] content) => new EndProfileImporter().Import(Encoding.UTF8.GetString(content)).Item;

    private DocumentService Documents => new(_factory, new SystemTimeProvider(), new FakeLogger<DocumentService>());
    private ModuleEndProfileService EndProfiles => new(_factory);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(
            new ModuleEndProfile { Id = EndProfileId, ScaleId = 1, Designation = "B96" },
            new ModuleEndProfile { Id = OtherEndProfileId, ScaleId = 1, Designation = "MMM480" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void AnEndProfileFileIsAccepted()
    {
        Assert.IsNull(EndProfileDrawings.ErrorKey(EndProfileFile(), EndProfileId));
        Assert.IsNull(EndProfileDrawings.ErrorKey([.. Encoding.UTF8.Preamble, .. EndProfileFile()], EndProfileId));
        Assert.IsNull(EndProfileDrawings.ErrorKey(EndProfileFile(EndProfileId), EndProfileId));
    }

    [TestMethod]
    public void OtherFilesAreNotEndProfileDrawings()
    {
        Assert.AreEqual("NotAnEndProfileDrawing", EndProfileDrawings.ErrorKey(Encoding.UTF8.GetBytes("not json"), EndProfileId));
        Assert.AreEqual("NotAnEndProfileDrawing", EndProfileDrawings.ErrorKey(Encoding.UTF8.GetBytes("""{ "designation": "B96" }"""), EndProfileId));
        Assert.AreEqual("NotAnEndProfileDrawing", EndProfileDrawings.ErrorKey([0x50, 0x4B, 0x03, 0x04], EndProfileId));
    }

    [TestMethod]
    public void AFileOfAnotherEndProfileIsRefused()
    {
        Assert.AreEqual("EndProfileDrawingOfAnotherEndProfile", EndProfileDrawings.ErrorKey(EndProfileFile(OtherEndProfileId), EndProfileId));
    }

    [TestMethod]
    public void TheRegistrysIdIsWrittenIntoTheFile()
    {
        var profile = Read(EndProfileDrawings.WithRegistryData(EndProfileFile(), EndProfileId));

        Assert.AreEqual(EndProfileId, ExternalReferences.ModuleRegistryId(profile.ExternalReferences, ExternalReferences.EndProfileId));
        Assert.AreEqual("Drawn B96", profile.Designation);
    }

    [TestMethod]
    public void AFileThatCantBeReadIsGivenAsStored()
    {
        byte[] content = [1, 2, 3];
        CollectionAssert.AreEqual(content, EndProfileDrawings.WithRegistryData(content, EndProfileId));
    }

    [TestMethod]
    public void AnEndProfilesJsonDocumentIsItsDrawing()
    {
        Assert.AreEqual((EndProfileId, (int?)7, nameof(ModuleEndProfile)), DocumentService.DocumentedObject(new ModuleEndProfile { Id = EndProfileId, DrawingId = 7 }, EndProfileDrawings.FileExtension));
        Assert.AreEqual(nameof(ModuleEndProfile.Drawing), DocumentService.DocumentKind(new ModuleEndProfile(), EndProfileDrawings.FileExtension));
        Assert.IsTrue(new ModuleEndProfile().IsValidDocumentObject());
    }

    [TestMethod]
    public void TheMeasuresAreThoseOfTheEndPlate()
    {
        Assert.AreEqual(((int?)500, (int?)96), EndProfileDrawings.Measures(EndProfileFileWithPlate(500, 96.3)));
        Assert.AreEqual(((int?)null, (int?)null), EndProfileDrawings.Measures(EndProfileFile()), "No plate is no measures.");
        Assert.AreEqual(((int?)null, (int?)null), EndProfileDrawings.Measures([1, 2, 3]));
    }

    [TestMethod]
    public async Task AnUploadedDrawingGivesTheWidthAndHeight()
    {
        await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFileWithPlate(546, 104)), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000);

        Assert.AreEqual(((int?)546, (int?)104), await StoredMeasuresAsync());
    }

    [TestMethod]
    public async Task ADrawingWithoutAPlateKeepsTheMeasures()
    {
        await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFileWithPlate(500, 96)), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000);

        await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFile()), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000);

        Assert.AreEqual(((int?)500, (int?)96), await StoredMeasuresAsync());
    }

    [TestMethod]
    public async Task AGlobalAdministratorUploadsTheDrawing()
    {
        var (count, _, document) = await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFile()), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000);

        Assert.AreEqual(1, Math.Sign(count));
        Assert.AreEqual(EndProfileDrawings.ContentType, document!.ContentType);
        Assert.AreEqual(document.Id, (await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!.DrawingId);
        Assert.AreEqual("B96.endprofile", await Documents.GetDocumentName(document.Id));
    }

    [TestMethod]
    public async Task OnlyAnEndProfileFileOfTheEndProfileIsUploaded()
    {
        Assert.AreEqual("NotAnEndProfileDrawing", (await Documents.SaveAsync(Administrator, new FakeFile(Encoding.UTF8.GetBytes("{}")), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000)).Message);
        Assert.AreEqual("EndProfileDrawingOfAnotherEndProfile", (await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFile(OtherEndProfileId)), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000)).Message);
        Assert.IsNull((await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!.DrawingId);
    }

    [TestMethod]
    public async Task OnlyGlobalAdministratorsUpload()
    {
        Assert.AreEqual(0, (await Documents.SaveAsync(User, new FakeFile(EndProfileFile()), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000)).Count);
        Assert.IsNull((await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!.DrawingId);
    }

    [TestMethod]
    public async Task EveryoneGetsTheDrawingWithTheRegistrysId()
    {
        await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFile()), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000);
        var documentId = (await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!.DrawingId!.Value;

        var drawing = await EndProfiles.FindDrawingAsync(User, EndProfileId);
        var download = await EndProfiles.FindDrawingByDocumentIdAsync(User, documentId);

        Assert.IsNotNull(drawing);
        Assert.AreEqual(EndProfileId, ExternalReferences.ModuleRegistryId(Read(drawing.Value.Content).ExternalReferences, ExternalReferences.EndProfileId));
        Assert.IsTrue(download.IsEndProfileDrawing);
        CollectionAssert.AreEqual(drawing.Value.Content, download.Content);
        Assert.IsNull(await EndProfiles.FindDrawingAsync(null, EndProfileId));
    }

    [TestMethod]
    public async Task AnEndProfileWithoutDrawingHasNone()
    {
        await _factory.SeedAsync(new Document { Id = 7, FileExtension = "pdf", ContentType = "application/pdf", Content = [1] });

        Assert.IsNull(await EndProfiles.FindDrawingAsync(User, EndProfileId));
        Assert.IsFalse((await EndProfiles.FindDrawingByDocumentIdAsync(User, 7)).IsEndProfileDrawing);
    }

    [TestMethod]
    public async Task DeletingTheDrawingClearsTheReference()
    {
        await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFile()), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000);
        var documentId = (await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!.DrawingId!.Value;

        Assert.AreEqual(0, (await Documents.DeleteAsync(User, documentId)).Count);
        var (count, _) = await Documents.DeleteAsync(Administrator, documentId);

        Assert.AreEqual(1, Math.Sign(count));
        Assert.IsNull((await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!.DrawingId);
        Assert.IsNull(await _factory.FindAsync<Document>(documentId));
    }

    [TestMethod]
    public async Task TheMeasuresAreSavedAndTheDrawingKept()
    {
        await Documents.SaveAsync(Administrator, new FakeFile(EndProfileFile()), new ModuleEndProfile { Id = EndProfileId }, EndProfileDrawings.FileExtension, 100_000);
        var documentId = (await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!.DrawingId;

        // As the page posts it: the form's fields on an entity without the document ids.
        var (count, _, _) = await EndProfiles.SaveAsync(Administrator, new ModuleEndProfile { Id = EndProfileId, ScaleId = 1, Designation = "B96", WidthMillimeter = 500 });

        var stored = (await _factory.FindAsync<ModuleEndProfile>(EndProfileId))!;
        Assert.AreEqual(1, count);
        Assert.AreEqual(500, stored.WidthMillimeter);
        Assert.AreEqual(documentId, stored.DrawingId);
    }

    private sealed class FakeFile(byte[] content) : IBrowserFile
    {
        public string Name => "B96.endprofile.json";
        public DateTimeOffset LastModified => DateTimeOffset.UnixEpoch;
        public long Size => content.Length;
        public string ContentType => "application/json";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => new MemoryStream(content);
    }
}