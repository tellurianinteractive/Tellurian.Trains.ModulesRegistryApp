using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using DrawnEndProfile = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.EndProfile;
using ExternalReferences = Tellurian.Trains.Schedules.Model.Layouts.ExternalReferences;
using GeometryPoint = Tellurian.Trains.Schedules.Model.Geometry.Point;
using PlateShape = Tellurian.Trains.Schedules.Model.Layouts.EndProfiles.PlateShape;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// End profile drawings saved from Module Draw through the public API, and who manages end profiles: global administrators and users
/// with the right MayManageEndProfiles, never read-only users, and not country administrators as such (decided 2026-11-26).
/// </summary>
[TestClass]
public sealed class EndProfileDrawingUploadTests
{
    private const int EndProfileId = 1, OtherEndProfileId = 2, DrawnEndProfileId = 3;
    private SqliteDbContextFactory _factory = null!;

    private DocumentService Documents => new(_factory, new SystemTimeProvider(), new FakeLogger<DocumentService>());
    private ModuleEndProfileService EndProfiles => new(_factory);

    private static ClaimsPrincipal Principal(params string[] rights)
    {
        List<Claim> claims =
        [
            new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
            new Claim(AppClaimTypes.UserId, "10"),
            new Claim(AppClaimTypes.PersonId, "10"),
            new Claim(AppClaimTypes.CountryId, "1"),
            .. rights.Select(r => new Claim(r, "true")),
        ];
        return new(new ClaimsIdentity(claims, "Test"));
    }

    private static ClaimsPrincipal Manager => Principal(AppClaimTypes.MayManageEndProfiles);
    private static ClaimsPrincipal GlobalAdministrator => Principal(AppClaimTypes.GlobalAdministrator);
    private static ClaimsPrincipal CountryAdministrator => Principal(AppClaimTypes.CountryAdministrator);
    private static ClaimsPrincipal User => Principal();
    private static ClaimsPrincipal ReadOnlyManager => Principal(AppClaimTypes.MayManageEndProfiles, AppClaimTypes.ReadOnly);

    /// <summary>An end profile file with a rectangular plate, 500 × 200 mm, at <paramref name="revision"/>.</summary>
    private static byte[] EndProfileFile(int revision = 1, int? registryEndProfileId = null, double width = 500)
    {
        var plate = new PlateShape([new GeometryPoint(-width / 2, 0), new GeometryPoint(width / 2, 0)], -200);
        var profile = new DrawnEndProfile
        {
            Id = Guid.NewGuid(),
            Designation = "B96",
            Scale = "H0",
            Plate = plate,
            Outline = plate.Outline(),
            GroundLine = plate.GroundLine(),
            Revision = revision,
        };
        if (registryEndProfileId is int id) profile.ExternalReferences = ExternalReferences.WithModuleRegistryId(null, ExternalReferences.EndProfileId, id);
        return Encoding.UTF8.GetBytes(new EndProfileExporter().Write(profile, ExportOptions.Default));
    }

    private static readonly byte[] StoredDrawing = EndProfileFile(revision: 3, width: 400);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new Document { Id = 1, FileExtension = EndProfileDrawings.FileExtension, ContentType = EndProfileDrawings.ContentType, Content = StoredDrawing });
        await _factory.SeedAsync(
            new ModuleEndProfile { Id = EndProfileId, ScaleId = 1, Designation = "B96" },
            new ModuleEndProfile { Id = OtherEndProfileId, ScaleId = 1, Designation = "MMM480" },
            new ModuleEndProfile { Id = DrawnEndProfileId, ScaleId = 1, Designation = "F96", DrawingId = 1 });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public void GlobalAdministratorsAndUsersWithTheRightManageEndProfiles()
    {
        Assert.IsTrue(GlobalAdministrator.MayManageEndProfiles());
        Assert.IsTrue(Manager.MayManageEndProfiles());
        Assert.IsFalse(CountryAdministrator.MayManageEndProfiles());
        Assert.IsFalse(User.MayManageEndProfiles());
        Assert.IsFalse(ReadOnlyManager.MayManageEndProfiles());
    }

    [TestMethod]
    public async Task AManagerSavesADrawingAndTheEndProfileGetsItsMeasures()
    {
        var result = await Documents.SaveEndProfileDrawingAsync(Manager, EndProfileId, EndProfileFile());

        Assert.AreEqual(new EndProfileDrawingUpload(EndProfileDrawingUploadStatus.Saved, null, 500, 200), result);
        var stored = await _factory.FindAsync<ModuleEndProfile>(EndProfileId);
        Assert.IsNotNull(stored?.DrawingId);
        Assert.AreEqual(500, stored.WidthMillimeter);
    }

    [TestMethod]
    public async Task OthersMayNotSaveADrawing()
    {
        foreach (var principal in new[] { User, CountryAdministrator, ReadOnlyManager })
        {
            var result = await Documents.SaveEndProfileDrawingAsync(principal, EndProfileId, EndProfileFile());

            Assert.AreEqual(EndProfileDrawingUploadStatus.NotAuthorised, result.Status);
        }
        Assert.IsNull((await _factory.FindAsync<ModuleEndProfile>(EndProfileId))?.DrawingId);
    }

    [TestMethod]
    public async Task AnUnknownEndProfileIsNotFound()
    {
        var result = await Documents.SaveEndProfileDrawingAsync(Manager, 99, EndProfileFile());

        Assert.AreEqual(EndProfileDrawingUploadStatus.NotFound, result.Status);
    }

    [TestMethod]
    public async Task OnlyAnEndProfileFileOfThisEndProfileIsSaved()
    {
        var notADrawing = await Documents.SaveEndProfileDrawingAsync(Manager, EndProfileId, Encoding.UTF8.GetBytes("not json"));
        var ofAnother = await Documents.SaveEndProfileDrawingAsync(Manager, EndProfileId, EndProfileFile(registryEndProfileId: OtherEndProfileId));

        Assert.AreEqual(new EndProfileDrawingUpload(EndProfileDrawingUploadStatus.Invalid, "NotAnEndProfileDrawing"), notADrawing);
        Assert.AreEqual(new EndProfileDrawingUpload(EndProfileDrawingUploadStatus.Invalid, "EndProfileDrawingOfAnotherEndProfile"), ofAnother);
    }

    [TestMethod]
    public async Task ATooLargeFileIsNotSaved()
    {
        var result = await Documents.SaveEndProfileDrawingAsync(Manager, EndProfileId, new byte[DocumentService.MaxEndProfileDrawingSize + 1]);

        Assert.AreEqual(EndProfileDrawingUploadStatus.TooLarge, result.Status);
    }

    [TestMethod]
    [DataRow(2)]
    [DataRow(3)]
    public async Task ADrawingNotNewerThanTheStoredOneIsAConflict(int revision)
    {
        var result = await Documents.SaveEndProfileDrawingAsync(Manager, DrawnEndProfileId, EndProfileFile(revision));

        Assert.AreEqual(new EndProfileDrawingUpload(EndProfileDrawingUploadStatus.Conflict, "EndProfileDrawingIsOlder"), result);
        await using var dbContext = _factory.CreateDbContext();
        CollectionAssert.AreEqual(StoredDrawing, (await dbContext.Documents.FindAsync(1))!.Content);
    }

    [TestMethod]
    public async Task ANewerDrawingReplacesTheStoredOne()
    {
        var newer = EndProfileFile(revision: 4);

        var result = await Documents.SaveEndProfileDrawingAsync(Manager, DrawnEndProfileId, newer);

        Assert.AreEqual(EndProfileDrawingUploadStatus.Saved, result.Status);
        await using var dbContext = _factory.CreateDbContext();
        CollectionAssert.AreEqual(newer, (await dbContext.Documents.FindAsync(1))!.Content);
    }

    [TestMethod]
    public async Task TheSameDrawingAgainIsNoConflict()
    {
        var result = await Documents.SaveEndProfileDrawingAsync(Manager, DrawnEndProfileId, StoredDrawing);

        Assert.AreNotEqual(EndProfileDrawingUploadStatus.Conflict, result.Status);
    }

    [TestMethod]
    public async Task AManagerEditsEndProfiles()
    {
        var endProfile = await EndProfiles.FindByIdAsync(Manager, EndProfileId);
        Assert.IsNotNull(endProfile);
        endProfile.Designation = "B96 new";

        var (count, _, _) = await EndProfiles.SaveAsync(Manager, endProfile);

        Assert.AreEqual(1, count);
        Assert.IsNull(await EndProfiles.FindByIdAsync(CountryAdministrator, EndProfileId));
    }
}