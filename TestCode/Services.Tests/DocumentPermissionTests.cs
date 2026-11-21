using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Diagnostics;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Migration step 2C.7: who may upload and delete documents, worked out from the database; upload in one save;
/// the station link; documents deleted with their module or station unless shared.
/// </summary>
[TestClass]
public sealed class DocumentPermissionTests
{
    private const int CountryId = 1;
    private const int Owner = 1, Other = 2, GroupDataAdministrator = 3, GroupMemberOwner = 4, Assistant = 5;
    private const int PersonModule = 1, GroupModule = 2, MemberModule = 3;
    private const int PersonModuleDocument = 11, GroupModuleDocument = 12, MemberModuleDocument = 13, StationDocument = 14, EndProfileDocument = 15;
    private const int StationId = 7; // Not the primary module's id, so a wrong link shows.

    private SqliteDbContextFactory _factory = null!;
    private FakeLogger<DocumentService> _logger = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        _logger = new FakeLogger<DocumentService>();
        await _factory.SeedAsync(new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(
            new Scale { Id = 1, ShortName = "H0", Denominator = 87 },
            NewPerson(Owner), NewPerson(Other), NewPerson(GroupDataAdministrator), NewPerson(GroupMemberOwner), NewPerson(Assistant),
            new Group { Id = 1, CountryId = CountryId, FullName = "Group", ShortName = "G", Category = "Club", CityName = "City" });
        await _factory.SeedAsync(
            new GroupMember { Id = 1, GroupId = 1, PersonId = GroupDataAdministrator, IsDataAdministrator = true },
            new GroupMember { Id = 2, GroupId = 1, PersonId = GroupMemberOwner },
            new ModuleStandard
            {
                Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25",
                Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo",
            });
        await _factory.SeedAsync(
            NewDocument(PersonModuleDocument, "dwg"), NewDocument(GroupModuleDocument, "dwg"), NewDocument(MemberModuleDocument, "dwg"),
            NewDocument(StationDocument, "pdf"), NewDocument(EndProfileDocument, "pdf"));
        await _factory.SeedAsync(
            NewModule(PersonModule, dwgDrawingId: PersonModuleDocument),
            NewModule(GroupModule, dwgDrawingId: GroupModuleDocument),
            NewModule(MemberModule, dwgDrawingId: MemberModuleDocument));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = PersonModule, PersonId = Owner, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = PersonModule, PersonId = Assistant, OwnedShare = 0 },
            new ModuleOwnership { Id = 3, ModuleId = GroupModule, GroupId = 1, OwnedShare = 1 },
            new ModuleOwnership { Id = 4, ModuleId = MemberModule, PersonId = GroupMemberOwner, OwnedShare = 1 },
            new Station { Id = StationId, FullName = "Station", Signature = "St", PrimaryModuleId = PersonModule, PdfInstructionId = StationDocument },
            new ModuleEndProfile { Id = 1, Designation = "Profile", ScaleId = 1, PdfDocumentId = EndProfileDocument });
        await using var dbContext = _factory.CreateDbContext();
        await dbContext.Modules.Where(m => m.Id == PersonModule).ExecuteUpdateAsync(s => s.SetProperty(m => m.StationId, StationId));
        await dbContext.ChangeLogs.ExecuteDeleteAsync();
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    #region Delete

    [TestMethod]
    [DataRow(Owner, PersonModuleDocument, DisplayName = "Owner, own module")]
    [DataRow(GroupDataAdministrator, GroupModuleDocument, DisplayName = "Data administrator, group's module")]
    [DataRow(GroupDataAdministrator, MemberModuleDocument, DisplayName = "Data administrator, a member's module")]
    [DataRow(Owner, StationDocument, DisplayName = "Owner of a station's module, station document")]
    [DataRow(Assistant, PersonModuleDocument, DisplayName = "Assistant (no share): helps the owners edit the module")]
    public async Task DeleteIsAllowed(int personId, int documentId)
    {
        var (count, _) = await Target.DeleteAsync(Person(personId), documentId);

        Assert.IsGreaterThan(0, count);
        Assert.IsNull(await _factory.FindAsync<Document>(documentId));
    }

    [TestMethod]
    [DataRow(Other, PersonModuleDocument, DisplayName = "Another person")]
    [DataRow(GroupDataAdministrator, PersonModuleDocument, DisplayName = "Data administrator, a non-member's module")]
    [DataRow(Other, StationDocument, DisplayName = "Another person, station document")]
    [DataRow(Owner, EndProfileDocument, DisplayName = "Owner, end profile document")]
    public async Task DeleteIsRefusedAndLogged(int personId, int documentId)
    {
        var (count, _) = await Target.DeleteAsync(Person(personId), documentId);

        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Document>(documentId));
        var entry = _logger.LatestRecord;
        Assert.AreEqual(SecurityLog.DocumentChangeDeniedEventId, entry.Id.Id);
        Assert.AreEqual(LogLevel.Warning, entry.Level);
    }

    [TestMethod]
    public async Task ReadOnlyOwnerIsRefused()
    {
        var (count, _) = await Target.DeleteAsync(Person(Owner, readOnly: true), PersonModuleDocument);

        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Document>(PersonModuleDocument));
    }

    [TestMethod]
    public async Task CountryAdministratorMayDeleteModuleDocument()
    {
        var (count, _) = await Target.DeleteAsync(CountryAdministrator(), PersonModuleDocument);

        Assert.IsGreaterThan(0, count);
        Assert.IsNull(await _factory.FindAsync<Document>(PersonModuleDocument));
    }

    /// <summary>
    /// As <see cref="ModuleEndProfileService"/>: in practice only global administrators edit end profiles.
    /// </summary>
    [TestMethod]
    public async Task OnlyGlobalAdministratorMayDeleteEndProfileDocument()
    {
        Assert.AreEqual(0, (await Target.DeleteAsync(CountryAdministrator(), EndProfileDocument)).Count);
        Assert.IsGreaterThan(0, (await Target.DeleteAsync(GlobalAdministrator(), EndProfileDocument)).Count);
        Assert.IsNull(await _factory.FindAsync<Document>(EndProfileDocument));
    }

    [TestMethod]
    public async Task DeleteKeepsTheModule()
    {
        await Target.DeleteAsync(Person(Owner), PersonModuleDocument);

        var module = await _factory.FindAsync<Module>(PersonModule);
        Assert.IsNotNull(module, "Not deleted by the model's cascade from the document.");
        Assert.IsNull(module.DwgDrawingId);
    }

    #endregion

    #region Upload

    [TestMethod]
    public async Task NewUploadAddsDocumentAndReferenceInOneSave()
    {
        var page = new Module { Id = PersonModule };

        var (count, _, document) = await Target.SaveAsync(Person(Owner), new FakeFile([1, 2, 3]), page, "pdf", 1000);

        Assert.IsGreaterThan(0, count);
        Assert.AreEqual(document!.Id, (await _factory.FindAsync<Module>(PersonModule))?.PdfDocumentationId);
        var rows = await ChangeLogsAsync();
        Assert.HasCount(2, rows);
        Assert.AreEqual(1, rows.Select(r => r.ChangedAt).Distinct().Count(), "One save.");
    }

    [TestMethod]
    public async Task UploadReplacesTheDocumentTheDatabaseRefersTo()
    {
        var stalePage = new Module { Id = PersonModule, DwgDrawingId = null };
        var documentsBefore = await CountDocumentsAsync();

        var (count, _, document) = await Target.SaveAsync(Person(Owner), new FakeFile([9, 9]), stalePage, "dwg", 1000);

        Assert.IsGreaterThan(0, count);
        Assert.AreEqual(PersonModuleDocument, document!.Id);
        Assert.AreEqual(documentsBefore, await CountDocumentsAsync(), "No new document, so no orphan.");
        CollectionAssert.AreEqual(new byte[] { 9, 9 }, (await _factory.FindAsync<Document>(PersonModuleDocument))?.Content);
    }

    [TestMethod]
    public async Task UploadByAnotherPersonIsRefusedAndLogged()
    {
        var documentsBefore = await CountDocumentsAsync();

        var (count, _, document) = await Target.SaveAsync(Person(Other), new FakeFile([1]), new Module { Id = PersonModule }, "pdf", 1000);

        Assert.AreEqual(0, count);
        Assert.IsNull(document);
        Assert.AreEqual(documentsBefore, await CountDocumentsAsync());
        Assert.AreEqual(SecurityLog.DocumentChangeDeniedEventId, _logger.LatestRecord.Id.Id);
    }

    [TestMethod]
    public async Task StationUploadLinksTheStationItself()
    {
        await Target.DeleteAsync(Person(Owner), StationDocument);
        var page = new Station { Id = StationId, PrimaryModuleId = PersonModule };

        var (count, _, document) = await Target.SaveAsync(Person(Owner), new FakeFile([1]), page, "pdf", 1000);

        Assert.IsGreaterThan(0, count);
        Assert.AreEqual(document!.Id, (await _factory.FindAsync<Station>(StationId))?.PdfInstructionId);
        Assert.IsNull((await _factory.FindAsync<Module>(PersonModule))?.PdfDocumentationId);
    }

    [TestMethod]
    public async Task TooLargeUploadIsRefusedAndLoggedWithSizes()
    {
        var documentsBefore = await CountDocumentsAsync();

        var (count, message, document) = await Target.SaveAsync(Person(Owner), new FakeFile(new byte[3000]), new Module { Id = PersonModule }, "skp", 2048);

        Assert.AreEqual(0, count);
        Assert.AreEqual(DocumentService.FileTooLargeMessage, message);
        Assert.IsNull(document);
        Assert.AreEqual(documentsBefore, await CountDocumentsAsync());
        var entry = _logger.LatestRecord;
        Assert.AreEqual(DocumentLog.FileTooLargeEventId, entry.Id.Id);
        Assert.AreEqual(LogLevel.Warning, entry.Level);
        Assert.AreEqual("SkpDrawing", entry.GetStructuredStateValue("DocumentKind"));
        Assert.AreEqual("3", entry.GetStructuredStateValue("FileSizeKb"), "Rounded up.");
        Assert.AreEqual("2", entry.GetStructuredStateValue("MaxSizeKb"));
        Assert.AreEqual((100 + Owner).ToString(), entry.GetStructuredStateValue("UserId"));
        Assert.AreEqual(nameof(Module), entry.GetStructuredStateValue("ObjectType"));
        Assert.AreEqual(PersonModule.ToString(), entry.GetStructuredStateValue("ObjectId"));
    }

    [TestMethod]
    public async Task TooLargeUploadByAnotherPersonIsOnlyLoggedAsRefused()
    {
        await Target.SaveAsync(Person(Other), new FakeFile(new byte[3000]), new Module { Id = PersonModule }, "skp", 2048);

        Assert.AreEqual(SecurityLog.DocumentChangeDeniedEventId, _logger.LatestRecord.Id.Id);
        Assert.IsFalse(_logger.Collector.GetSnapshot().Any(r => r.Id.Id == DocumentLog.FileTooLargeEventId), "Sizes are logged only for users who may upload.");
    }

    [TestMethod]
    [DataRow("dwg", "DwgDrawing")]
    [DataRow("pdf", "PdfDocumentation")]
    [DataRow("skp", "SkpDrawing")]
    public void ModuleDocumentKind(string fileExtension, string expected) =>
        Assert.AreEqual(expected, DocumentService.DocumentKind(new Module(), fileExtension));

    [TestMethod]
    public void StationDocumentKind() =>
        Assert.AreEqual("PdfInstruction", DocumentService.DocumentKind(new Station(), "pdf"));

    #endregion

    #region Documents deleted with their object

    [TestMethod]
    public async Task ModuleDeleteDeletesItsDocuments()
    {
        // The document part of ModuleService.DeleteAsync (ModuleSaveTests deletes through the service).
        await using var dbContext = _factory.CreateDbContext();
        var module = await dbContext.Modules.Include(m => m.ModuleOwnerships).SingleAsync(m => m.Id == MemberModule);

        var removed = await DocumentService.RemoveDocumentsAsync(dbContext, module.DocumentIds());
        dbContext.ModuleOwnerships.RemoveRange(module.ModuleOwnerships);
        dbContext.Modules.Remove(module);
        await dbContext.SaveChangesAsync();

        Assert.AreEqual(1, removed);
        Assert.IsNull(await _factory.FindAsync<Module>(MemberModule));
        Assert.IsNull(await _factory.FindAsync<Document>(MemberModuleDocument));
    }

    [TestMethod]
    public async Task StationDeleteDeletesItsDocument()
    {
        await using var dbContext = _factory.CreateDbContext();
        var station = await dbContext.Stations.SingleAsync(s => s.Id == StationId);
        await dbContext.Modules.Where(m => m.StationId == StationId).ExecuteUpdateAsync(s => s.SetProperty(m => m.StationId, (int?)null));

        var removed = await DocumentService.RemoveDocumentsAsync(dbContext, [station.PdfInstructionId!.Value]);
        dbContext.Stations.Remove(station);
        await dbContext.SaveChangesAsync();

        Assert.AreEqual(1, removed);
        Assert.IsNull(await _factory.FindAsync<Document>(StationDocument));
    }

    #endregion

    private DocumentService Target => new(_factory, new SystemTimeProvider(), _logger);

    private async Task<int> CountDocumentsAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        return await dbContext.Documents.CountAsync();
    }

    private async Task<List<ChangeLog>> ChangeLogsAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        return await dbContext.ChangeLogs.AsNoTracking().ToListAsync();
    }

    private static ClaimsPrincipal Person(int personId, bool readOnly = false)
    {
        List<Claim> claims =
        [
            new(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
            new(AppClaimTypes.PersonId, personId.ToString()),
            new(AppClaimTypes.UserId, (100 + personId).ToString()),
            new(AppClaimTypes.CountryId, CountryId.ToString()),
        ];
        if (readOnly) claims.Add(new(AppClaimTypes.ReadOnly, "true"));
        return new(new ClaimsIdentity(claims, "Test"));
    }

    private static ClaimsPrincipal CountryAdministrator() => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.PersonId, Other.ToString()),
        new Claim(AppClaimTypes.CountryId, CountryId.ToString()),
        new Claim(AppClaimTypes.CountryAdministrator, "true"),
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator() => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.PersonId, Other.ToString()),
        new Claim(AppClaimTypes.CountryId, CountryId.ToString()),
        new Claim(AppClaimTypes.GlobalAdministrator, "true"),
    ], "Test"));

    private static Person NewPerson(int id) =>
        new() { Id = id, FirstName = "First", LastName = "Last", CityName = "City", CountryId = CountryId };

    private static Document NewDocument(int id, string fileExtension) =>
        new() { Id = id, FileExtension = fileExtension, ContentType = "application/octet-stream", Content = [1] };

    private static Module NewModule(int id, int? dwgDrawingId = null) =>
        new() { Id = id, FullName = $"Module {id}", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1, DwgDrawingId = dwgDrawingId };

    private sealed class FakeFile(byte[] content) : IBrowserFile
    {
        public string Name => "file";
        public DateTimeOffset LastModified => DateTimeOffset.UnixEpoch;
        public long Size => content.Length;
        public string ContentType => "application/octet-stream";
        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default) => new MemoryStream(content);
    }
}