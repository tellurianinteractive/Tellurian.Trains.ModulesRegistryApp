using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Saving a region (Migration step 3.4): only a country administrator in the region's country, the name capitalised, and only
/// a permitted colour, with the text colour following it.
/// </summary>
[TestClass]
public sealed class RegionSaveTests
{
    private const int Sweden = 1, Norway = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal CountryAdministrator(int countryId) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.PersonId, "1"),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        new Claim(AppClaimTypes.CountryAdministrator, "true"),
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Region { Id = 1, CountryId = Sweden, LocalName = "Skåne", BackColor = "#FFFFE0", ForeColor = "#000000" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task TheNameIsCapitalisedAndTheTextColourFollowsTheColour()
    {
        var target = new RegionService(_factory);

        var result = await target.SaveAsync(CountryAdministrator(Sweden), new Region { CountryId = Sweden, LocalName = "VÄSTRA GÖTALAND", BackColor = "#ff0000" });

        Assert.AreEqual(1, result.Count);
        var saved = (await _factory.FindAsync<Region>(result.Entity!.Id))!;
        Assert.AreEqual("Västra Götaland", saved.LocalName);
        Assert.AreEqual(RegionColors.Red, saved.BackColor);
        Assert.AreEqual("#FFFFFF", saved.ForeColor);
    }

    [TestMethod]
    public async Task AColourOutsideThePermittedOnesIsNotSaved()
    {
        var target = new RegionService(_factory);

        var result = await target.SaveAsync(CountryAdministrator(Sweden), new Region { CountryId = Sweden, LocalName = "Halland", BackColor = "#FFFFE0" });

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task OnlyACountryAdministratorInTheRegionsCountry()
    {
        var target = new RegionService(_factory);

        var result = await target.SaveAsync(CountryAdministrator(Norway), new Region { CountryId = Sweden, LocalName = "Halland", BackColor = RegionColors.Green });

        Assert.AreEqual(0, result.Count);
    }
}