using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Saving external stations (Migration step 3.6): any signed-in user may add and edit them, the texts are corrected by the
/// rules of the region's country, a saved station keeps its region, and an unchanged save is "no modification" for everyone.
/// </summary>
[TestClass]
public sealed class ExternalStationSaveTests
{
    private const int Germany = 4, Hessen = 1, Bayern = 2;
    private SqliteDbContextFactory _factory = null!;
    private ExternalStationService Target => new(_factory);

    private static ClaimsPrincipal User(int userId = 5) => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.UserId, userId.ToString()), new Claim(AppClaimTypes.CountryId, "1")], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = Germany, EnglishName = "Germany", DomainSuffix = "de", Languages = "de" });
        await _factory.SeedAsync(
            new Region { Id = Hessen, CountryId = Germany, LocalName = "Hessen", BackColor = "#FFFFE0", ForeColor = "#000000" },
            new Region { Id = Bayern, CountryId = Germany, LocalName = "Bayern", BackColor = "#FFFFE0", ForeColor = "#000000" });
        await _factory.SeedAsync(new ExternalStation { Id = 1, RegionId = Hessen, FullName = "Frankfurt Hbf", Signature = "FF" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static ExternalStation Stored() => new() { Id = 1, RegionId = Hessen, FullName = "Frankfurt Hbf", Signature = "FF" };

    [TestMethod]
    [DataRow(5, DisplayName = "A user")]
    [DataRow(1, DisplayName = "The super user")]
    public async Task AnUnchangedSaveIsNoModification(int userId) =>
        Assert.AreEqual(-1, (await Target.SaveAsync(User(userId), Stored())).Count);

    [TestMethod]
    public async Task ANewStationIsCorrectedByTheRulesOfTheRegionsCountry()
    {
        var result = await Target.SaveAsync(User(), new ExternalStation { RegionId = Hessen, FullName = "darmstadt Hbf", Signature = "fd", InternationalName = "" });

        Assert.IsGreaterThan(0, result.Count);
        var saved = (await _factory.FindAsync<ExternalStation>(result.Entity!.Id))!;
        Assert.AreEqual("Darmstadt Hbf", saved.FullName);
        Assert.AreEqual("FD", saved.Signature, "German signatures are in capitals.");
        Assert.IsNull(saved.InternationalName, "An empty optional name is saved as empty.");
    }

    private async Task SeedCustomersAsync()
    {
        await _factory.SeedAsync(new NHM { Id = 1, Code = "1", LevelDigits = 2 });
        await _factory.SeedAsync(new Cargo { Id = 5, NhmCode = 1, EN = "Paper", DE = "Papier", DA = "Papir", SV = "Papper", NB = "Papir" });
        await _factory.SeedAsync(
            new CargoDirection { Id = 1, FullName = "Receive", ShortName = "R" },
            new OperatingDay { Id = 8, Flag = 127, FullName = "Daily", ShortName = "Daily" },
            new CargoQuantityUnit { Id = 4, FullName = "Wagons", Designation = "wg", SingularResourceCode = "Wagon", PluralResourceCode = "Wagons" });
        await _factory.SeedAsync(
            new ExternalStationCustomer { Id = 7, ExternalStationId = 1, CustomerName = "Stora Enso" },
            new ExternalStationCustomer { Id = 8, ExternalStationId = 1, CustomerName = "Holmen" });
        await _factory.SeedAsync(new ExternalStationCustomerCargo { Id = 12, ExternalStationCustomerId = 8, CargoId = 5, DirectionId = 1, OperatingDayId = 8, QuantityUnitId = 4, Quantity = 1 });
    }

    private static ExternalStationCustomerCargo Flow(int id = 0) => new() { Id = id, CargoId = 5, DirectionId = 1, OperatingDayId = 8, QuantityUnitId = 4, Quantity = 2, SpecialCargoName = "PAPER ROLLS", SpecificWagonClass = "" };

    [TestMethod]
    public async Task ANewCargoFlowIsAddedToTheCustomerAndCorrected()
    {
        await SeedCustomersAsync();

        var result = await Target.SaveCargoAsync(User(), 7, Flow());

        Assert.IsGreaterThan(0, result.Count);
        var saved = (await _factory.FindAsync<ExternalStationCustomerCargo>(result.Entity!.Id))!;
        Assert.AreEqual(7, saved.ExternalStationCustomerId);
        Assert.AreEqual("Paper rolls", saved.SpecialCargoName, "A cargo name, as the cargo types'.");
        Assert.IsNull(saved.SpecificWagonClass, "An empty optional text is saved as empty.");
    }

    [TestMethod]
    public async Task AnotherCustomersCargoFlowIsNotChangedOrDeleted()
    {
        await SeedCustomersAsync();

        Assert.AreEqual(0, (await Target.SaveCargoAsync(User(), 7, Flow(12))).Count, "Flow 12 belongs to customer 8.");
        Assert.AreEqual(0, (await Target.DeleteCargoAsync(User(), 7, 12)).Count);
        var flow = (await _factory.FindAsync<ExternalStationCustomerCargo>(12))!;
        Assert.AreEqual(8, flow.ExternalStationCustomerId);
        Assert.AreEqual(1, flow.Quantity);
    }

    [TestMethod]
    public async Task TheCustomersOwnCargoFlowIsChangedAndDeleted()
    {
        await SeedCustomersAsync();

        Assert.IsGreaterThan(0, (await Target.SaveCargoAsync(User(), 8, Flow(12))).Count);
        Assert.AreEqual(2, (await _factory.FindAsync<ExternalStationCustomerCargo>(12))!.Quantity);
        Assert.AreEqual(-1, (await Target.SaveCargoAsync(User(), 8, Flow(12))).Count, "Unchanged.");
        Assert.IsGreaterThan(0, (await Target.DeleteCargoAsync(User(), 8, 12)).Count);
        Assert.IsNull(await _factory.FindAsync<ExternalStationCustomerCargo>(12));
    }

    [TestMethod]
    public async Task SavingTheCustomerKeepsItsCargoFlows()
    {
        await SeedCustomersAsync();
        var customer = (await Target.FindCustomerByIdAsync(User(), 8))!;
        customer.CustomerName = "Holmen Paper";

        Assert.IsGreaterThan(0, (await Target.SaveAsync(User(), customer)).Count);
        Assert.IsNotNull(await _factory.FindAsync<ExternalStationCustomerCargo>(12));
    }

    [TestMethod]
    public async Task ASavedStationKeepsItsRegion()
    {
        var moved = Stored();
        moved.RegionId = Bayern;

        Assert.AreEqual(0, (await Target.SaveAsync(User(), moved)).Count);
        Assert.AreEqual(Hessen, (await _factory.FindAsync<ExternalStation>(1))!.RegionId);
    }
}
