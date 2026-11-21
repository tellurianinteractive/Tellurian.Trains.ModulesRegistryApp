using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may save a vehicle (Migration step 3.6): the owner, and a country administrator in the owner's country; a new vehicle is the
/// user's own. Before, any signed-in user could save any vehicle. Features that are not for the vehicle's scale are removed.
/// </summary>
[TestClass]
public sealed class VehicleSaveTests
{
    private const int Owner = 1, Other = 2, Danish = 3;
    private const int H0 = 3, N = 5;
    private const int H0Coupling = 101, Wheels = 2, Traction = 204;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId, int countryId = 1, params Claim[] claims) => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.PersonId, personId.ToString()), new Claim(AppClaimTypes.CountryId, countryId.ToString()), .. claims], "Test"));
    private static ClaimsPrincipal CountryAdministrator(int countryId) => Principal(Other, countryId, new Claim(AppClaimTypes.CountryAdministrator, "true"));

    private VehicleService Target => new(_factory);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = 1, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = 2, EnglishName = "Denmark", DomainSuffix = "dk", Languages = "da" });
        await _factory.SeedAsync(
            new Person { Id = Owner, FirstName = "Anna", LastName = "Berg", CountryId = 1 },
            new Person { Id = Other, FirstName = "Erik", LastName = "Ek", CountryId = 1 },
            new Person { Id = Danish, FirstName = "Jens", LastName = "Holm", CountryId = 2 });
        await _factory.SeedAsync(
            new Scale { Id = H0, ShortName = "H0", Denominator = 87 },
            new Scale { Id = N, ShortName = "N", Denominator = 160 });
        await _factory.SeedAsync(
            new VehicleFeature { Id = H0Coupling, Category = "COUPLINGS", Description = "FL6511", OnlyScaleId = H0 },
            new VehicleFeature { Id = Wheels, Category = "WHEELS", Description = "NEM310" },
            new VehicleFeature { Id = Traction, Category = "TRACTION", Description = "Electric", IsResourceCode = true });
        await _factory.SeedAsync(Vehicle(1, Owner), Vehicle(2, Danish));
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static Vehicle Vehicle(int id, int owner, string number = "1323") => new()
    {
        Id = id, OwningPersonId = owner, KeeperSignature = "SJ", VehicleClass = "Rc6", VehicleNumber = number, Theme = "SJ",
        ModelManufacturerName = "Roco", InventoryNumber = 1, ScaleId = H0,
    };

    [TestMethod]
    public async Task AnotherUserMayNotChangeSomeonesVehicle()
    {
        var result = await Target.SaveAsync(Principal(Other), Vehicle(1, Other, "9999"));

        Assert.AreEqual(0, result.Count);
        var stored = (await _factory.FindAsync<Vehicle>(1))!;
        Assert.AreEqual("1323", stored.VehicleNumber);
        Assert.AreEqual(Owner, stored.OwningPersonId);
    }

    [TestMethod]
    public async Task TheOwnerMayChangeTheVehicleButNotItsOwner()
    {
        var result = await Target.SaveAsync(Principal(Owner), Vehicle(1, Other, "1324"));

        Assert.AreEqual(1, result.Count, result.Message);
        var stored = (await _factory.FindAsync<Vehicle>(1))!;
        Assert.AreEqual("1324", stored.VehicleNumber);
        Assert.AreEqual(Owner, stored.OwningPersonId);
    }

    [TestMethod]
    public async Task ACountryAdministratorMayChangeVehiclesOnlyInTheirCountry()
    {
        Assert.AreEqual(1, (await Target.SaveAsync(CountryAdministrator(1), Vehicle(1, Owner, "1325"))).Count);
        Assert.AreEqual(0, (await Target.SaveAsync(CountryAdministrator(1), Vehicle(2, Danish, "1325"))).Count);
        Assert.AreEqual("1323", (await _factory.FindAsync<Vehicle>(2))!.VehicleNumber);
    }

    [TestMethod]
    public async Task ACountryAdministratorGetsVehiclesOnlyInTheirCountry()
    {
        Assert.IsNotNull(await Target.GetVehicleAsync(CountryAdministrator(1), 1, countryId: 1));
        Assert.IsNull(await Target.GetVehicleAsync(CountryAdministrator(1), 2, countryId: 1));
    }

    [TestMethod]
    public async Task UsersAddVehiclesOnlyForThemselves()
    {
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(Other), Vehicle(0, Owner))).Count);
        Assert.AreEqual(1, (await Target.SaveAsync(Principal(Other), Vehicle(0, Other))).Count);
    }

    [TestMethod]
    public async Task AFeatureNotForTheScaleIsRemoved()
    {
        var vehicle = Vehicle(0, Owner);
        vehicle.ScaleId = N;
        vehicle.CouplingFeatureId = H0Coupling;
        vehicle.WheelsFeatureId = Wheels;
        vehicle.TractionFeatureId = H0Coupling; // Not a traction feature.

        var result = await Target.SaveAsync(Principal(Owner), vehicle);

        var stored = (await _factory.FindAsync<Vehicle>(result.Entity!.Id))!;
        Assert.IsNull(stored.CouplingFeatureId);
        Assert.AreEqual(Wheels, stored.WheelsFeatureId);
        Assert.IsNull(stored.TractionFeatureId);
    }

    [TestMethod]
    public async Task TextsAreCorrected()
    {
        var vehicle = Vehicle(0, Owner);
        vehicle.KeeperSignature = " sj ";
        vehicle.Theme = "db";
        vehicle.Note = "  ";
        vehicle.F0 = "“Light”";

        var result = await Target.SaveAsync(Principal(Owner), vehicle);

        var stored = (await _factory.FindAsync<Vehicle>(result.Entity!.Id))!;
        Assert.AreEqual("SJ", stored.KeeperSignature);
        Assert.AreEqual("DB", stored.Theme);
        Assert.IsNull(stored.Note);
        Assert.AreEqual("\"Light\"", stored.F0);
    }
}