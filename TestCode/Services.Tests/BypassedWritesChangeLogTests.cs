using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Migration step 2C.5d: writes that used to bypass the change tracker (raw SQL insert, <c>ExecuteUpdate</c>)
/// now go through it, so they write the same data as before and get change log rows.
/// </summary>
[TestClass]
public sealed class BypassedWritesChangeLogTests
{
    private const int CountryId = 1;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal SignedIn => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.CountryId, CountryId.ToString()),
        new Claim(AppClaimTypes.PersonId, "1"),
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 }, NewPerson(1), NewPerson(2));
        await _factory.SeedAsync(new ModuleStandard
        {
            Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25",
            Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo",
        });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task AssistantIsAddedAndLogged()
    {
        await _factory.SeedAsync(NewModule());
        await _factory.SeedAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = 1, OwnedShare = 1.0 });
        await ClearChangeLogAsync();
        var target = new ModuleOwnershipService(_factory);

        var (count, _, module) = await target.AddAssistantAsync(SignedIn, new ModuleOwnership { ModuleId = 1, PersonId = 2, OwnedShare = 0 });

        Assert.AreEqual(1, count);
        var assistant = module!.ModuleOwnerships.Single(mo => mo.PersonId == 2);
        Assert.AreEqual(0.0, assistant.OwnedShare);
        var row = (await ChangeLogsAsync()).Single();
        Assert.AreEqual(nameof(ModuleOwnership), row.EntityType);
        Assert.AreEqual(assistant.Id, row.EntityId);
        Assert.AreEqual(ChangeOperation.Added, row.Operation);
    }

    [TestMethod]
    public async Task DocumentDeleteClearsReferencesAndIsLogged()
    {
        await _factory.SeedAsync(new Document { Id = 1, FileExtension = "pdf", ContentType = "application/pdf", Content = [1, 2, 3] });
        var module = NewModule();
        module.PdfDocumentationId = 1;
        module.DwgDrawingId = 1;
        await _factory.SeedAsync(module);
        await ClearChangeLogAsync();
        var target = new DocumentService(_factory, new SystemTimeProvider());

        var (count, _) = await target.DeleteAsync(SignedIn, 1);

        Assert.IsGreaterThan(0, count);
        Assert.IsNull(await _factory.FindAsync<Document>(1));
        var saved = await _factory.FindAsync<Module>(1);
        Assert.IsNull(saved!.PdfDocumentationId);
        Assert.IsNull(saved.DwgDrawingId);

        var rows = await ChangeLogsAsync();
        var moduleRow = rows.Single(r => r.EntityType == nameof(Module));
        Assert.AreEqual(ChangeOperation.Modified, moduleRow.Operation);
        CollectionAssert.AreEquivalent(new[] { "DwgDrawingId", "PdfDocumentationId" }, moduleRow.ChangedProperties!.Split(','));
        Assert.AreEqual(ChangeOperation.Deleted, rows.Single(r => r.EntityType == nameof(Document)).Operation);
    }

    [TestMethod]
    public async Task VehicleFillInChangesOnlyMatchingVehiclesAndIsLogged()
    {
        var source = NewVehicle(1, "Rc", length: 15.5m);
        await _factory.SeedAsync(source, NewVehicle(2, "Rc"), NewVehicle(3, "Ma"));
        await ClearChangeLogAsync();
        var target = new VehicleService(_factory);

        var count = await target.AddPrototypeLengthForSameKeeperAndClass(source);

        Assert.AreEqual(1, count);
        Assert.AreEqual(15.5m, (await _factory.FindAsync<Vehicle>(2))?.PrototypeLength);
        Assert.IsNull((await _factory.FindAsync<Vehicle>(3))?.PrototypeLength, "Another vehicle class.");
        var row = (await ChangeLogsAsync()).Single();
        Assert.AreEqual(nameof(Vehicle), row.EntityType);
        Assert.AreEqual(2, row.EntityId);
        Assert.AreEqual("PrototypeLength", row.ChangedProperties);
    }

    [TestMethod]
    public async Task VehicleFillInWithNothingToFillWritesNothing()
    {
        var source = NewVehicle(1, "Rc", length: 15.5m);
        await _factory.SeedAsync(source, NewVehicle(2, "Rc", length: 20m));
        await ClearChangeLogAsync();

        var count = await new VehicleService(_factory).AddPrototypeLengthForSameKeeperAndClass(source);

        Assert.AreEqual(0, count);
        Assert.IsEmpty(await ChangeLogsAsync());
    }

    private async Task<List<ChangeLog>> ChangeLogsAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        return await dbContext.ChangeLogs.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
    }

    private async Task ClearChangeLogAsync()
    {
        await using var dbContext = _factory.CreateDbContext();
        await dbContext.ChangeLogs.ExecuteDeleteAsync();
    }

    private static Module NewModule() =>
        new() { Id = 1, FullName = "Module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 };

    private static Person NewPerson(int id) =>
        new() { Id = id, FirstName = "First", LastName = "Last", CityName = "City", CountryId = CountryId };

    private static Vehicle NewVehicle(int id, string vehicleClass, decimal? length = null) => new()
    {
        Id = id,
        OwningPersonId = 1,
        ScaleId = 1,
        KeeperSignature = "SJ",
        VehicleClass = vehicleClass,
        VehicleNumber = id.ToString(),
        PrototypeLength = length,
        Theme = "SE",
    };
}