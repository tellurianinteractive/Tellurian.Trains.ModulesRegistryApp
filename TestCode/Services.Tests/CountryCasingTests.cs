using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>A station's texts are corrected by the rules of its country: the region's country, or else the user's.</summary>
[TestClass]
public sealed class CountryCasingTests
{
    private const int Sweden = 1, Germany = 4;
    private SqliteDbContextFactory _factory = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Germany, EnglishName = "Germany", DomainSuffix = "de", Languages = "de" });
        await _factory.SeedAsync(new Region { Id = 1, CountryId = Germany, LocalName = "Hessen", BackColor = "#FFFFE0", ForeColor = "#000000" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static ClaimsPrincipal UserIn(int countryId) => new(new ClaimsIdentity([new Claim(AppClaimTypes.CountryId, countryId.ToString())], "Test"));

    [TestMethod]
    [DataRow(1, Sweden, TextCase.UpperCase, DisplayName = "The region's country decides, not the user's")]
    [DataRow(null, Sweden, TextCase.Capitalized, DisplayName = "No region: the user's country")]
    [DataRow(null, 99, TextCase.InitialCapital, DisplayName = "Neither known: the default")]
    public async Task TheSignatureRule(int? regionId, int userCountryId, TextCase expected)
    {
        await using var dbContext = _factory.CreateDbContext();
        var casings = await dbContext.CountryCasingsAsync(regionId, UserIn(userCountryId));
        Assert.AreEqual(expected, casings.Of(TextKinds.StationSignature, TextCase.InitialCapital));
    }
}
