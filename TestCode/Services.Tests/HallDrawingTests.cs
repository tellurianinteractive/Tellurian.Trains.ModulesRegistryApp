using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Importers.Drawing;
using Tellurian.Trains.Schedules.Model.Layouts.Halls;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The hall drawing of a venue: only a hall file from Module Draw is accepted, only for a venue and by those who may edit it. It is
/// given with the Registry's name, address and reference, only to users who see the locality.
/// </summary>
[TestClass]
public sealed class HallDrawingTests
{
    private const int Sweden = 1, Norway = 2;
    private const int Venue = 1, Storage = 2, ClubStorage = 3;
    private const int Club = 1;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int countryId = Sweden, params Claim[] role) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, "999"),
        new Claim(AppClaimTypes.PersonId, "999"),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. role,
    ], "Test"));

    private static ClaimsPrincipal CountryAdministrator => Principal(Sweden, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal NorwegianAdministrator => Principal(Norway, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal User => Principal();

    private static byte[] HallFile(string name = "Drawn name") =>
        Encoding.UTF8.GetBytes(new HallExporter().Write(Hall.Rectangle(name, 30000, 20000), ExportOptions.Default));

    private DocumentService Documents => new(_factory, new SystemTimeProvider(), new FakeLogger<DocumentService>());
    private LocalityService Localities => new(_factory);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Group { Id = Club, CountryId = Sweden, FullName = "Club", ShortName = "C", Category = "Club", CityName = "City" });
        await _factory.SeedAsync(
            new Locality { Id = Venue, CountryId = Sweden, Name = "Sporthallen", StreetAddress = "Storgatan 1", PostalCode = "331 30", CityName = "Värnamo", IsMeetingVenue = true },
            new Locality { Id = Storage, CountryId = Sweden, Name = "Garage", CityName = "Jönköping", IsModuleStorage = true },
            new Locality { Id = ClubStorage, CountryId = Sweden, Name = "Club store", CityName = "Jönköping", IsModuleStorage = true, UsedByGroupId = Club });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void AHallFileIsAccepted()
    {
        Assert.IsNull(HallDrawings.ErrorKey(HallFile()));
        Assert.IsNull(HallDrawings.ErrorKey([.. Encoding.UTF8.Preamble, .. HallFile()]));
    }

    [TestMethod]
    public void OtherFilesAreNotHallDrawings()
    {
        Assert.AreEqual("NotAHallDrawing", HallDrawings.ErrorKey(Encoding.UTF8.GetBytes("not json")));
        Assert.AreEqual("NotAHallDrawing", HallDrawings.ErrorKey(Encoding.UTF8.GetBytes("""{ "name": "Hall" }""")));
        Assert.AreEqual("NotAHallDrawing", HallDrawings.ErrorKey([0x50, 0x4B, 0x03, 0x04]));
    }

    [TestMethod]
    public async Task TheRegistrysDataIsWrittenIntoTheFile()
    {
        var locality = (await _factory.FindAsync<Locality>(Venue))!;

        var written = HallDrawings.WithRegistryData(HallFile(), locality);

        var hall = new HallImporter().Import(Encoding.UTF8.GetString(written)).Item;
        Assert.AreEqual("Sporthallen", hall.Name);
        Assert.AreEqual("Storgatan 1, 331 30 Värnamo", hall.Address);
        Assert.AreEqual(Venue, hall.ExternalReferences![HallDrawings.ExternalReferenceKey].GetProperty("localityId").GetInt32());
        Assert.AreEqual(4, hall.WallCount);
    }

    [TestMethod]
    public void AFileThatCantBeReadIsGivenAsStored()
    {
        byte[] content = [1, 2, 3];
        CollectionAssert.AreEqual(content, HallDrawings.WithRegistryData(content, new Locality { Id = 1, Name = "Hall", CityName = "City" }));
    }

    [TestMethod]
    public async Task AnAdministratorUploadsTheHallDrawingOfAVenue()
    {
        var (count, _, document) = await Documents.SaveAsync(CountryAdministrator, new FakeFile(HallFile()), new Locality { Id = Venue }, HallDrawings.FileExtension, 100_000);

        Assert.AreEqual(1, Math.Sign(count));
        Assert.AreEqual(HallDrawings.ContentType, document!.ContentType);
        Assert.AreEqual(document.Id, (await _factory.FindAsync<Locality>(Venue))!.HallDrawingId);
    }

    [TestMethod]
    public async Task OnlyAHallFileIsUploaded()
    {
        var (count, message, _) = await Documents.SaveAsync(CountryAdministrator, new FakeFile(Encoding.UTF8.GetBytes("{}")), new Locality { Id = Venue }, HallDrawings.FileExtension, 100_000);

        Assert.AreEqual(0, count);
        Assert.AreEqual("NotAHallDrawing", message);
        Assert.IsNull((await _factory.FindAsync<Locality>(Venue))!.HallDrawingId);
    }

    [TestMethod]
    public async Task OnlyAVenueHasAHallDrawing()
    {
        var (count, _, _) = await Documents.SaveAsync(CountryAdministrator, new FakeFile(HallFile()), new Locality { Id = Storage }, HallDrawings.FileExtension, 100_000);

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task OnlyThoseWhoMayEditTheLocalityUpload()
    {
        Assert.AreEqual(0, (await Documents.SaveAsync(User, new FakeFile(HallFile()), new Locality { Id = Venue }, HallDrawings.FileExtension, 100_000)).Count);
        Assert.AreEqual(0, (await Documents.SaveAsync(NorwegianAdministrator, new FakeFile(HallFile()), new Locality { Id = Venue }, HallDrawings.FileExtension, 100_000)).Count);
    }

    [TestMethod]
    public async Task AModuleGetsNoHallDrawing()
    {
        Assert.AreEqual((0, (int?)null, string.Empty), DocumentService.DocumentedObject(new Module { Id = 1 }, HallDrawings.FileExtension));
        await Task.CompletedTask;
    }

    [TestMethod]
    public async Task EveryoneSeesTheDrawingOfAVenueWithTheRegistrysData()
    {
        await Documents.SaveAsync(CountryAdministrator, new FakeFile(HallFile()), new Locality { Id = Venue }, HallDrawings.FileExtension, 100_000);

        var drawing = await Localities.FindHallDrawingAsync(User, Venue);
        var documentId = (await _factory.FindAsync<Locality>(Venue))!.HallDrawingId!.Value;
        var download = await Localities.FindHallDrawingByDocumentIdAsync(User, documentId);

        Assert.IsNotNull(drawing);
        StringAssert.Contains(Encoding.UTF8.GetString(drawing.Value.Content), "Sporthallen");
        Assert.IsTrue(download.IsHallDrawing);
        CollectionAssert.AreEqual(drawing.Value.Content, download.Content);
    }

    [TestMethod]
    public async Task ADrawingOfAGroupsLocalityIsOnlyForThoseWhoSeeIt()
    {
        await _factory.SeedAsync(new Document { Id = 1, FileExtension = HallDrawings.FileExtension, ContentType = HallDrawings.ContentType, Content = HallFile() });
        await using (var dbContext = _factory.CreateDbContext())
        {
            (await dbContext.Localities.FindAsync(ClubStorage))!.HallDrawingId = 1;
            await dbContext.SaveChangesAsync();
        }

        var download = await Localities.FindHallDrawingByDocumentIdAsync(User, 1);

        Assert.IsTrue(download.IsHallDrawing);
        Assert.IsNull(download.Content);
        Assert.IsNull(await Localities.FindHallDrawingAsync(User, ClubStorage));
        Assert.IsNotNull(await Localities.FindHallDrawingAsync(CountryAdministrator, ClubStorage));
    }

    [TestMethod]
    public async Task OtherDocumentsAreNotHallDrawings()
    {
        await _factory.SeedAsync(new Document { Id = 7, FileExtension = "pdf", ContentType = "application/pdf", Content = [1] });

        Assert.IsFalse((await Localities.FindHallDrawingByDocumentIdAsync(User, 7)).IsHallDrawing);
    }

    private sealed class FakeFile(byte[] content) : IBrowserFile
    {
        public string Name => "hall.json";
        public DateTimeOffset LastModified => DateTimeOffset.UnixEpoch;
        public long Size => content.Length;
        public string ContentType => "application/json";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => new MemoryStream(content);
    }
}