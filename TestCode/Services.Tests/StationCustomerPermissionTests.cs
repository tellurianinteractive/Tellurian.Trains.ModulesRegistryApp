using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// A station's freight customers and their waybills may be changed only by those who may edit the station: its module's
/// owners, data administrators of an owning group or of a group an owner is a member of, and administrators (release
/// 1.15.10; before, any signed-in user could). Moving a customer to another station requires both stations. Each cargo flow
/// is saved and deleted on its own, only as a flow of the customer it belongs to (1.15.22).
/// </summary>
[TestClass]
public sealed class StationCustomerPermissionTests
{
    private const int CountryId = 1;
    private const int OtherCountryId = 2;
    private const int Owner = 1, OtherOwner = 2, OwnersGroupAdministrator = 3, GroupOwnersAdministrator = 4, Stranger = 5;
    private const int PersonStation = 1, OtherPersonStation = 2, GroupStation = 3;
    private const int PersonStationCustomer = 1, OtherPersonStationCustomer = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId, int countryId = CountryId, params Claim[] extra) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, personId.ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. extra,
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = OtherCountryId, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Region { Id = 1, CountryId = CountryId, LocalName = "Region", ForeColor = "#000000", BackColor = "#ffffff" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync([.. Enumerable.Range(1, 5).Select(id => (object)new Person { Id = id, FirstName = "First", LastName = $"Last{id}", CityName = "City", CountryId = CountryId })]);
        await _factory.SeedAsync(NewGroup(1), NewGroup(2));
        await _factory.SeedAsync(NewModule(PersonStation), NewModule(OtherPersonStation), NewModule(GroupStation));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = PersonStation, PersonId = Owner, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = OtherPersonStation, PersonId = OtherOwner, OwnedShare = 1 },
            new ModuleOwnership { Id = 3, ModuleId = GroupStation, GroupId = 2, OwnedShare = 1 });
        await _factory.SeedAsync(
            // Group 1: the owner is a member and person 3 is its data administrator. Group 2 owns a station; person 4 is its data administrator.
            new GroupMember { Id = 1, GroupId = 1, PersonId = Owner },
            new GroupMember { Id = 2, GroupId = 1, PersonId = OwnersGroupAdministrator, IsDataAdministrator = true },
            new GroupMember { Id = 3, GroupId = 2, PersonId = GroupOwnersAdministrator, IsDataAdministrator = true },
            new GroupMember { Id = 4, GroupId = 2, PersonId = Stranger });
        await _factory.SeedAsync(NewStation(PersonStation), NewStation(OtherPersonStation), NewStation(GroupStation));
        await _factory.SeedAsync(NewCustomer(PersonStationCustomer, PersonStation), NewCustomer(OtherPersonStationCustomer, OtherPersonStation));
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    [DataRow(Owner, PersonStation, true, DisplayName = "Owner")]
    [DataRow(Stranger, PersonStation, false, DisplayName = "Another user")]
    [DataRow(OtherOwner, PersonStation, false, DisplayName = "Owner of another station")]
    [DataRow(OwnersGroupAdministrator, PersonStation, true, DisplayName = "Data administrator in the owner's group")]
    [DataRow(GroupOwnersAdministrator, GroupStation, true, DisplayName = "Data administrator of the owning group")]
    [DataRow(Stranger, GroupStation, false, DisplayName = "Member, not administrator, of the owning group")]
    public async Task MayEditStation(int personId, int stationId, bool expected)
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.AreEqual(expected, await dbContext.MayEditStationAsync(Principal(personId), stationId));
    }

    [TestMethod]
    public async Task AdministratorsMayEditStation()
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.IsTrue(await dbContext.MayEditStationAsync(Principal(Stranger, extra: new Claim(AppClaimTypes.GlobalAdministrator, "true")), PersonStation));
        Assert.IsTrue(await dbContext.MayEditStationAsync(Principal(Stranger, extra: new Claim(AppClaimTypes.CountryAdministrator, "true")), PersonStation));
        Assert.IsFalse(await dbContext.MayEditStationAsync(Principal(Stranger, OtherCountryId, new Claim(AppClaimTypes.CountryAdministrator, "true")), PersonStation), "A country administrator in another country.");
    }

    [TestMethod]
    public async Task ReadOnlyOwnerMayNotEditStation()
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.IsFalse(await dbContext.MayEditStationAsync(Principal(Owner, extra: new Claim(AppClaimTypes.ReadOnly, "true")), PersonStation));
    }

    [TestMethod]
    public async Task OwnerSavesCustomer()
    {
        var target = new StationCustomerService(_factory);

        var (count, _, _) = await target.SaveAsync(Principal(Owner), PersonStation, NewCustomer(PersonStationCustomer, PersonStation, "Renamed"));

        Assert.AreEqual(1, count);
        Assert.AreEqual("Renamed", (await _factory.FindAsync<StationCustomer>(PersonStationCustomer))!.CustomerName);
    }

    [TestMethod]
    public async Task AnotherUserMayNotSaveOrAddCustomer()
    {
        var target = new StationCustomerService(_factory);

        var (updated, _, _) = await target.SaveAsync(Principal(Stranger), PersonStation, NewCustomer(PersonStationCustomer, PersonStation, "Renamed"));
        var (added, _, _) = await target.SaveAsync(Principal(Stranger), PersonStation, NewCustomer(0, PersonStation, "New"));

        Assert.AreEqual(0, updated);
        Assert.AreEqual(0, added);
        Assert.AreEqual("Customer", (await _factory.FindAsync<StationCustomer>(PersonStationCustomer))!.CustomerName);
    }

    [TestMethod]
    public async Task AnotherUserMayNotDeleteCustomer()
    {
        var target = new StationCustomerService(_factory);

        var (count, _) = await target.DeleteAsync(Principal(Stranger), PersonStationCustomer);

        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<StationCustomer>(PersonStationCustomer));
    }

    [TestMethod]
    public async Task OwnerDeletesCustomer()
    {
        var target = new StationCustomerService(_factory);

        var (count, _) = await target.DeleteAsync(Principal(Owner), PersonStationCustomer);

        Assert.AreEqual(1, count);
        Assert.IsNull(await _factory.FindAsync<StationCustomer>(PersonStationCustomer));
    }

    [TestMethod]
    public async Task OwnerMayNotTakeAnotherStationsCustomer()
    {
        // Saving another station's customer id on the owner's station would move that customer here.
        var target = new StationCustomerService(_factory);

        var (count, _, _) = await target.SaveAsync(Principal(Owner), PersonStation, NewCustomer(OtherPersonStationCustomer, PersonStation, "Taken"));

        Assert.AreEqual(0, count);
        var customer = await _factory.FindAsync<StationCustomer>(OtherPersonStationCustomer);
        Assert.AreEqual(OtherPersonStation, customer!.StationId);
        Assert.AreEqual("Customer", customer.CustomerName);
    }

    [TestMethod]
    public async Task AdministratorMovesCustomerToAnotherStation()
    {
        // A feature: correcting a customer that was added to the wrong station.
        var target = new StationCustomerService(_factory);
        var administrator = Principal(Stranger, extra: new Claim(AppClaimTypes.GlobalAdministrator, "true"));

        var (count, _, _) = await target.SaveAsync(administrator, OtherPersonStation, NewCustomer(PersonStationCustomer, OtherPersonStation));

        Assert.AreEqual(1, count);
        Assert.AreEqual(OtherPersonStation, (await _factory.FindAsync<StationCustomer>(PersonStationCustomer))!.StationId);
    }

    [TestMethod]
    public async Task AnotherUserMayNotGenerateWaybills()
    {
        // Allowed, the stored procedure would run, which SQLite doesn't have; so 0 without an exception means refused.
        var target = new StationCustomerWaybillsService(_factory);

        var count = await target.AddGeneratedCustomerWaybills(Principal(Stranger), PersonStationCustomer);

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task MayEditStationOfCustomer()
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.IsTrue(await dbContext.MayEditStationOfCustomerAsync(Principal(Owner), PersonStationCustomer));
        Assert.IsFalse(await dbContext.MayEditStationOfCustomerAsync(Principal(Owner), OtherPersonStationCustomer));
        Assert.IsFalse(await dbContext.MayEditStationOfCustomerAsync(Principal(Owner), 999), "A customer that doesn't exist.");
    }

    private async Task SeedCargoFlowAsync()
    {
        await _factory.SeedAsync(new NHM { Id = 1, Code = "1", LevelDigits = 2, DA = "", DE = "", EN = "", NL = "", PL = "", SV = "" });
        await _factory.SeedAsync(new Cargo { Id = 5, NhmCode = 1, EN = "Paper", DE = "Papier", DA = "Papir", SV = "Papper", NB = "Papir" });
        await _factory.SeedAsync(
            new CargoDirection { Id = 1, FullName = "Receive", ShortName = "R" },
            new OperatingDay { Id = 8, Flag = 127, FullName = "Daily", ShortName = "Daily" },
            new CargoQuantityUnit { Id = 4, FullName = "Wagons", Designation = "wg", SingularResourceCode = "Wagon", PluralResourceCode = "Wagons" },
            new CargoReadyTime { Id = 1, FullName = "Unspecified", ShortName = "-" },
            new CargoPackagingUnit { Id = 0, SingularResourceCode = "Unspecified", PluralResourceCode = "Unspecified", PrepositionResourceCode = "" });
        await _factory.SeedAsync(new StationCustomerCargo { Id = 12, StationCustomerId = OtherPersonStationCustomer, CargoId = 5, DirectionId = 1, OperatingDayId = 8, QuantityUnitId = 4, Quantity = 1, ReadyTimeId = 1 });
        await _factory.SeedAsync(new StationCustomerCargo { Id = 13, StationCustomerId = PersonStationCustomer, CargoId = 5, DirectionId = 1, OperatingDayId = 8, QuantityUnitId = 4, Quantity = 1, ReadyTimeId = 1 });
    }

    private static StationCustomerCargo Flow(int id = 0) => new()
    {
        Id = id, CargoId = 5, DirectionId = 1, OperatingDayId = 8, QuantityUnitId = 4, Quantity = 2, ReadyTimeId = 1, TrackOrAreaColor = "#fffffe",
        SpecificWagonClass = " ", TrackOrArea = "", MaxTrainsetLength = 0, FromYear = 1990, UptoYear = 1960,
    };

    [TestMethod]
    public async Task OwnerAddsACorrectedCargoFlow()
    {
        await SeedCargoFlowAsync();
        var target = new StationCustomerService(_factory);

        var (count, _, entity) = await target.SaveCargoAsync(Principal(Owner), PersonStationCustomer, Flow());

        Assert.IsGreaterThan(0, count);
        var saved = (await _factory.FindAsync<StationCustomerCargo>(entity!.Id))!;
        Assert.AreEqual(PersonStationCustomer, saved.StationCustomerId);
        Assert.IsNull(saved.SpecificWagonClass, "An empty optional text is saved as empty.");
        Assert.IsNull(saved.TrackOrArea);
        Assert.IsNull(saved.MaxTrainsetLength, "Zero is no limit.");
        Assert.AreEqual(RegionColors.White, saved.TrackOrAreaColor);
        Assert.AreEqual((short)1960, saved.FromYear, "Reversed years are swapped.");
    }

    [TestMethod]
    public async Task OwnerChangesAndDeletesTheCustomersOwnCargoFlow()
    {
        await SeedCargoFlowAsync();
        var target = new StationCustomerService(_factory);

        Assert.IsGreaterThan(0, (await target.SaveCargoAsync(Principal(Owner), PersonStationCustomer, Flow(13))).Count);
        Assert.AreEqual(2, (await _factory.FindAsync<StationCustomerCargo>(13))!.Quantity);
        Assert.AreEqual(-1, (await target.SaveCargoAsync(Principal(Owner), PersonStationCustomer, Flow(13))).Count, "Unchanged.");
        Assert.IsGreaterThan(0, (await target.DeleteCargoAsync(Principal(Owner), PersonStationCustomer, 13)).Count);
        Assert.IsNull(await _factory.FindAsync<StationCustomerCargo>(13));
    }

    [TestMethod]
    public async Task AnotherCustomersCargoFlowIsNotChangedOrDeleted()
    {
        await SeedCargoFlowAsync();
        var target = new StationCustomerService(_factory);

        Assert.AreEqual(0, (await target.SaveCargoAsync(Principal(Owner), PersonStationCustomer, Flow(12))).Count, "Flow 12 belongs to the other station's customer.");
        Assert.AreEqual(0, (await target.DeleteCargoAsync(Principal(Owner), PersonStationCustomer, 12)).Count);
        var flow = (await _factory.FindAsync<StationCustomerCargo>(12))!;
        Assert.AreEqual(OtherPersonStationCustomer, flow.StationCustomerId);
        Assert.AreEqual(1, flow.Quantity);
    }

    [TestMethod]
    public async Task AnotherUserMayNotAddChangeOrDeleteCargoFlows()
    {
        await SeedCargoFlowAsync();
        var target = new StationCustomerService(_factory);

        Assert.AreEqual(0, (await target.SaveCargoAsync(Principal(Stranger), OtherPersonStationCustomer, Flow())).Count);
        Assert.AreEqual(0, (await target.SaveCargoAsync(Principal(Stranger), OtherPersonStationCustomer, Flow(12))).Count);
        Assert.AreEqual(0, (await target.DeleteCargoAsync(Principal(Stranger), OtherPersonStationCustomer, 12)).Count);
        Assert.AreEqual(1, (await _factory.FindAsync<StationCustomerCargo>(12))!.Quantity);
    }

    [TestMethod]
    public async Task SavingTheCustomerKeepsItsCargoFlows()
    {
        await SeedCargoFlowAsync();
        var target = new StationCustomerService(_factory);
        var customer = (await target.FindByIdAsync(Principal(Owner), PersonStationCustomer))!;
        customer.CustomerName = "Renamed";
        customer.Cargos.Clear();

        Assert.IsGreaterThan(0, (await target.SaveAsync(Principal(Owner), PersonStation, customer)).Count);
        Assert.IsNotNull(await _factory.FindAsync<StationCustomerCargo>(13));
    }

    [TestMethod]
    public async Task AdministratorMovesCustomerWithMoveCustomer()
    {
        var target = new StationCustomerService(_factory);
        var administrator = Principal(Stranger, extra: new Claim(AppClaimTypes.CountryAdministrator, "true"));

        var (count, _) = await target.MoveCustomerAsync(administrator, PersonStationCustomer, GroupStation);

        Assert.AreEqual(1, count);
        Assert.AreEqual(GroupStation, (await _factory.FindAsync<StationCustomer>(PersonStationCustomer))!.StationId);
    }

    [TestMethod]
    public async Task OwnerMayNotMoveCustomerToAStationTheyMayNotEdit()
    {
        var target = new StationCustomerService(_factory);

        Assert.AreEqual(0, (await target.MoveCustomerAsync(Principal(Owner), PersonStationCustomer, OtherPersonStation)).Count);
        Assert.AreEqual(0, (await target.MoveCustomerAsync(Principal(Owner), OtherPersonStationCustomer, PersonStation)).Count, "Nor take another station's customer.");
        Assert.AreEqual(PersonStation, (await _factory.FindAsync<StationCustomer>(PersonStationCustomer))!.StationId);
        Assert.AreEqual(OtherPersonStation, (await _factory.FindAsync<StationCustomer>(OtherPersonStationCustomer))!.StationId);
    }

    [TestMethod]
    public async Task OnlyAdministratorsOfTheCountryGetStationsToMoveTo()
    {
        var target = new StationCustomerService(_factory);

        Assert.HasCount(3, await target.MoveToStationItemsAsync(Principal(Stranger, extra: new Claim(AppClaimTypes.CountryAdministrator, "true")), CountryId));
        Assert.IsEmpty(await target.MoveToStationItemsAsync(Principal(Owner), CountryId));
        Assert.IsEmpty(await target.MoveToStationItemsAsync(Principal(Stranger, OtherCountryId, new Claim(AppClaimTypes.CountryAdministrator, "true")), CountryId));
    }

    private async Task SeedWaybillsAsync()
    {
        await SeedCargoFlowAsync();
        await _factory.SeedAsync(
            new StationCustomerWaybill { Id = 20, StationCustomerId = PersonStationCustomer, StationCustomerCargoId = 13, OperatingDayId = 8, PrintCount = 1 },
            new StationCustomerWaybill { Id = 21, StationCustomerId = OtherPersonStationCustomer, StationCustomerCargoId = 12, OperatingDayId = 8, PrintCount = 1 });
    }

    private static StationCustomerWaybill Waybill(int id, int customerId) => new() { Id = id, StationCustomerId = customerId, OperatingDayId = 8, PrintCount = 3 };

    [TestMethod]
    public async Task OwnerSavesTheCustomersOwnWaybill()
    {
        await SeedWaybillsAsync();
        var target = new StationCustomerWaybillsService(_factory);

        Assert.IsGreaterThan(0, (await target.SaveAsync(Principal(Owner), PersonStationCustomer, Waybill(20, PersonStationCustomer))).Count);
        var saved = (await _factory.FindAsync<StationCustomerWaybill>(20))!;
        Assert.AreEqual(3, saved.PrintCount);
        Assert.AreEqual(13, saved.StationCustomerCargoId, "A waybill stays with its cargo flow.");
    }

    [TestMethod]
    public async Task AnotherCustomersWaybillIsNotSaved()
    {
        await SeedWaybillsAsync();
        var target = new StationCustomerWaybillsService(_factory);

        Assert.AreEqual(0, (await target.SaveAsync(Principal(Owner), PersonStationCustomer, Waybill(21, PersonStationCustomer))).Count, "Waybill 21 belongs to the other station's customer.");
        Assert.AreEqual(0, (await target.SaveAsync(Principal(Stranger), OtherPersonStationCustomer, Waybill(21, OtherPersonStationCustomer))).Count, "Another user.");
        Assert.AreEqual(1, (await _factory.FindAsync<StationCustomerWaybill>(21))!.PrintCount);
    }

    private static Group NewGroup(int id) => new() { Id = id, CountryId = CountryId, FullName = $"Group {id}", ShortName = $"G{id}", Category = "Club", CityName = "City" };
    private static Module NewModule(int id) => new() { Id = id, FullName = $"Module {id}", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 };
    private static Station NewStation(int id) => new() { Id = id, FullName = $"Station {id}", Signature = $"S{id}", RegionId = 1, PrimaryModuleId = id };
    private static StationCustomer NewCustomer(int id, int stationId, string name = "Customer") => new() { Id = id, StationId = stationId, CustomerName = name };
}