using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// After a document is deleted, the page's object must not keep the deleted id: saving it failed on the
/// foreign key in production (2026-11-15). <see cref="DocumentService.ClearDocumentReference"/> clears it.
/// </summary>
[TestClass]
public sealed class DocumentReferenceTests
{
    private const int CountryId = 1;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal SignedIn => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString())], "Test"));

    /// <summary>
    /// Document deletes need the right to edit the module (step 2C.7); no owner is seeded here.
    /// </summary>
    private static ClaimsPrincipal Administrator => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.GlobalAdministrator, "true"),
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard
        {
            Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track",
            PreferredTheme = "Europe",
        });
        await _factory.SeedAsync(new Document { Id = 1, FileExtension = "skp", ContentType = "application/octet-stream", Content = [1] });
        await _factory.SeedAsync(NewModule(skpDrawingId: 1));
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    [DataRow("dwg", nameof(Module.DwgDrawingId))]
    [DataRow("pdf", nameof(Module.PdfDocumentationId))]
    [DataRow("skp", nameof(Module.SkpDrawingId))]
    public void ModuleReferenceIsCleared(string fileExtension, string propertyName)
    {
        var module = new Module { DwgDrawingId = 1, PdfDocumentationId = 2, SkpDrawingId = 3 };

        Assert.IsTrue(DocumentService.ClearDocumentReference(module, fileExtension));

        var values = new Dictionary<string, int?>
        {
            [nameof(Module.DwgDrawingId)] = module.DwgDrawingId,
            [nameof(Module.PdfDocumentationId)] = module.PdfDocumentationId,
            [nameof(Module.SkpDrawingId)] = module.SkpDrawingId,
        };
        Assert.IsNull(values[propertyName]);
        Assert.AreEqual(2, values.Values.Count(v => v.HasValue), "The other references are kept.");
    }

    [TestMethod]
    public void StationReferenceIsCleared()
    {
        var station = new Station { PdfInstructionId = 1 };

        Assert.IsTrue(DocumentService.ClearDocumentReference(station, "pdf"));

        Assert.IsNull(station.PdfInstructionId);
    }

    [TestMethod]
    public void UnknownObjectOrExtensionChangesNothing()
    {
        Assert.IsFalse(DocumentService.ClearDocumentReference(new Station { PdfInstructionId = 1 }, "dwg"));
        Assert.IsFalse(DocumentService.ClearDocumentReference(new object(), "pdf"));
        Assert.IsFalse(DocumentService.ClearDocumentReference(null, "pdf"));
    }

    [TestMethod]
    public async Task SavingStaleObjectAfterDeleteFailsOnForeignKey()
    {
        var pageModule = await LoadAsPageAsync();
        await new DocumentService(_factory, new SystemTimeProvider(), new Microsoft.Extensions.Logging.Testing.FakeLogger<DocumentService>()).DeleteAsync(Administrator, 1);

        await Assert.ThrowsAsync<DbUpdateException>(() => SaveAsPageAsync(pageModule));
    }

    [TestMethod]
    public async Task SavingClearedObjectAfterDeleteWorks()
    {
        var pageModule = await LoadAsPageAsync();
        await new DocumentService(_factory, new SystemTimeProvider(), new Microsoft.Extensions.Logging.Testing.FakeLogger<DocumentService>()).DeleteAsync(Administrator, 1);
        pageModule.FullName = "Changed";

        DocumentService.ClearDocumentReference(pageModule, "skp");
        await SaveAsPageAsync(pageModule);

        var saved = await _factory.FindAsync<Module>(1);
        Assert.AreEqual("Changed", saved!.FullName);
        Assert.IsNull(saved.SkpDrawingId);
    }

    /// <summary>
    /// As the edit page: an untracked copy, kept while the user works.
    /// </summary>
    private async Task<Module> LoadAsPageAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        return await dbContext.Modules.AsNoTracking().SingleAsync(m => m.Id == 1);
    }

    /// <summary>
    /// As <c>ModuleService.SaveAsync</c>: load the row and copy the page's values onto it.
    /// </summary>
    private async Task SaveAsPageAsync(Module pageModule)
    {
        await using var dbContext = _factory.CreateDbContext();
        var existing = await dbContext.Modules.SingleAsync(m => m.Id == pageModule.Id);
        dbContext.Entry(existing).CurrentValues.SetValues(pageModule);
        await dbContext.SaveChangesAsync();
    }

    private static Module NewModule(int? skpDrawingId) =>
        new() { Id = 1, FullName = "Module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1, SkpDrawingId = skpDrawingId };
}