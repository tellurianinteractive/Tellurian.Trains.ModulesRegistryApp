using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The external stations list box, which the super user gets in a freight customer's form: the items of the former view
/// <c>ListExternalStation</c>. Before 1.17.3 it was read with <c>FromSqlRaw</c> into <see cref="ListboxItem"/>, which had no table
/// after step 4.1, so the query failed and the form showed an error.
/// </summary>
[TestClass]
public sealed class ExternalStationListboxTests
{
    private const int Sweden = 1, Norway = 2, Vaster = 1, Soder = 2, Ostlandet = 3;
    private SqliteDbContextFactory _factory = null!;
    private ExternalStationService Target => new(_factory);

    private static ClaimsPrincipal User() => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.UserId, "5"), new Claim(AppClaimTypes.CountryId, Sweden.ToString())], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(
            new Region { Id = Vaster, CountryId = Sweden, LocalName = "Väster", BackColor = "#009933", ForeColor = "#FFFFFF" },
            new Region { Id = Soder, CountryId = Sweden, LocalName = "Söder", BackColor = "#FFFF00", ForeColor = "#000000" },
            new Region { Id = Ostlandet, CountryId = Norway, LocalName = "Østlandet", BackColor = "#FF0000", ForeColor = "#FFFFFF" });
        await _factory.SeedAsync(
            new ExternalStation { Id = 40, RegionId = Vaster, FullName = "Agnesberg", Signature = "Agb" },
            new ExternalStation { Id = 41, RegionId = Soder, FullName = "Arlöv", Signature = "Al" },
            new ExternalStation { Id = 50, RegionId = Ostlandet, FullName = "Oslo", Signature = "Osl" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task ItemsAreTheCountrysStationsWithRegionAndDomain()
    {
        var items = (await Target.ListboxItemsAsync(User(), Sweden, null)).ToList();
        Assert.HasCount(2, items);
        Assert.AreEqual("Agnesberg, Väster (se)", items[0].Description);
        Assert.AreEqual(40, items[0].Id);
        Assert.AreEqual("#009933", items[0].BackColor);
        Assert.AreEqual("#FFFFFF", items[0].ForeColor);
        Assert.AreEqual("Arlöv, Söder (se)", items[1].Description);
    }

    [TestMethod]
    public async Task ARegionLimitsTheItems()
    {
        var items = (await Target.ListboxItemsAsync(User(), Sweden, Soder)).ToList();
        Assert.AreEqual(41, items.Single().Id);
    }

    [TestMethod]
    public async Task AnotherCountrysItems()
    {
        var items = (await Target.ListboxItemsAsync(User(), Norway, null)).ToList();
        Assert.AreEqual("Oslo, Østlandet (no)", items.Single().Description);
    }
}