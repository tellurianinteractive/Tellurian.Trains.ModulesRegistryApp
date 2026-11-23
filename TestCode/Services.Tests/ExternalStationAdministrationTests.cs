using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may delete external stations and move their customers: country administrators fully manage the external stations of their
/// own country (global administrators of all countries); moving a customer to another station is only for the super user and
/// global administrators.
/// </summary>
[TestClass]
public sealed class ExternalStationAdministrationTests
{
    private const int Sweden = 1, Germany = 4, Vaster = 1, Hessen = 2;
    private SqliteDbContextFactory _factory = null!;
    private ExternalStationService Target => new(_factory);

    private static ClaimsPrincipal User(int userId = 5, int countryId = Sweden, params Claim[] claims) => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.UserId, userId.ToString()), new Claim(AppClaimTypes.CountryId, countryId.ToString()), .. claims], "Test"));
    private static ClaimsPrincipal CountryAdministrator(int countryId) => User(countryId: countryId, claims: new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal GlobalAdministrator => User(claims: new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal SuperUser => User(userId: 1);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Germany, EnglishName = "Germany", DomainSuffix = "de", Languages = "de" });
        await _factory.SeedAsync(
            new Region { Id = Vaster, CountryId = Sweden, LocalName = "Väster", BackColor = "#009933", ForeColor = "#FFFFFF" },
            new Region { Id = Hessen, CountryId = Germany, LocalName = "Hessen", BackColor = "#FFFFE0", ForeColor = "#000000" });
        await _factory.SeedAsync(
            new ExternalStation { Id = 40, RegionId = Vaster, FullName = "Agnesberg", Signature = "Agb" },
            new ExternalStation { Id = 41, RegionId = Vaster, FullName = "Alingsås", Signature = "A" },
            new ExternalStation { Id = 50, RegionId = Hessen, FullName = "Frankfurt Hbf", Signature = "FF" });
        await _factory.SeedAsync(new ExternalStationCustomer { Id = 7, ExternalStationId = 40, CustomerName = "Stora Enso" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task CountryAdministratorDeletesStationInOwnCountry()
    {
        Assert.AreEqual(1, (await Target.DeleteAsync(CountryAdministrator(Sweden), 41)).Count);
        Assert.IsNull(await _factory.FindAsync<ExternalStation>(41));
    }

    [TestMethod]
    public async Task CountryAdministratorMayNotDeleteStationInOtherCountry()
    {
        Assert.AreEqual(0, (await Target.DeleteAsync(CountryAdministrator(Sweden), 50)).Count);
        Assert.IsNotNull(await _factory.FindAsync<ExternalStation>(50));
    }

    [TestMethod]
    public async Task GlobalAdministratorDeletesStationInAnyCountry() =>
        Assert.AreEqual(1, (await Target.DeleteAsync(GlobalAdministrator, 50)).Count);

    [TestMethod]
    public async Task UserMayNotDeleteStation() =>
        Assert.AreEqual(0, (await Target.DeleteAsync(User(), 41)).Count);

    [TestMethod]
    public async Task SuperUserMovesCustomer() => await AssertMovedAsync(SuperUser, expected: true);

    [TestMethod]
    public async Task GlobalAdministratorMovesCustomer() => await AssertMovedAsync(GlobalAdministrator, expected: true);

    [TestMethod]
    public async Task CountryAdministratorMayNotMoveCustomer() => await AssertMovedAsync(CountryAdministrator(Sweden), expected: false);

    [TestMethod]
    public async Task UserMayNotMoveCustomer() => await AssertMovedAsync(User(), expected: false);

    private async Task AssertMovedAsync(ClaimsPrincipal principal, bool expected)
    {
        var result = await Target.MoveCustomerAsync(principal, new ExternalStationCustomer { Id = 7 }, 41);
        Assert.AreEqual(expected ? 1 : 0, result.Count);
        Assert.AreEqual(expected ? 41 : 40, (await _factory.FindAsync<ExternalStationCustomer>(7))!.ExternalStationId);
    }
}