using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>A person's names and city are corrected for capitals when saved, not validated (decided 2026-11-18).</summary>
[TestClass]
public sealed class PersonSaveTests
{
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal GlobalAdministrator => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.PersonId, "1"),
        new Claim(AppClaimTypes.CountryId, "1"),
        new Claim(AppClaimTypes.GlobalAdministrator, "true"),
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = 1, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task NamesAndCityAreCapitalised()
    {
        var target = new PersonService(_factory, new SystemTimeProvider());

        var result = await target.SaveAsync(GlobalAdministrator, new Person { FirstName = "ANNA-KARIN", MiddleName = "Af Trolle", LastName = "VON SYDOW", CityName = "frankfurt am main", CountryId = 1 });

        Assert.AreEqual(1, result.Count, result.Message);
        var saved = (await _factory.FindAsync<Person>(result.Entity!.Id))!;
        Assert.AreEqual("Anna-Karin", saved.FirstName);
        Assert.AreEqual("af Trolle", saved.MiddleName);
        Assert.AreEqual("von Sydow", saved.LastName);
        Assert.AreEqual("Frankfurt am Main", saved.CityName);
    }
}