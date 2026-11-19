using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// A person's names and city are corrected for capitals when saved, not validated (decided 2026-11-18); the same person registered
/// twice and the same sign-in address for two users are refused (Migration step 3.5).
/// </summary>
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

    [TestMethod]
    public async Task TheSamePersonIsNotRegisteredTwice()
    {
        var target = new PersonService(_factory, new SystemTimeProvider());
        await _factory.SeedAsync(new Person { Id = 10, FirstName = "Anna", LastName = "Berg", CityName = "Göteborg", CountryId = 1 });

        var again = await target.SaveAsync(GlobalAdministrator, new Person { FirstName = "anna", LastName = "BERG", CityName = "göteborg", CountryId = 1 });
        var namesake = await target.SaveAsync(GlobalAdministrator, new Person { FirstName = "Anna", LastName = "Berg", CityName = "Malmö", CountryId = 1 });
        var herself = await target.SaveAsync(GlobalAdministrator, new Person { Id = 10, FirstName = "Anna", LastName = "Berg", CityName = "Göteborg", CountryId = 1, MiddleName = "Maria" });

        Assert.AreEqual(0, again.Count);
        Assert.AreEqual(ModulesRegistry.Resources.Strings.PersonAlreadyExists, again.Message);
        Assert.AreEqual(1, namesake.Count, "A namesake in another city.");
        Assert.AreEqual(1, herself.Count, herself.Message);
    }

    [TestMethod]
    public async Task NoEmailAddressIsAnotherUsersSignInAddress()
    {
        var target = new PersonService(_factory, new SystemTimeProvider());
        await _factory.SeedAsync(new User { Id = 5, ObjectId = Guid.NewGuid(), EmailAddress = "anna@example.se", RegistrationTime = DateTimeOffset.Now });
        await _factory.SeedAsync(new Person { Id = 10, FirstName = "Anna", LastName = "Berg", CityName = "Göteborg", CountryId = 1, UserId = 5, EmailAddresses = "anna@example.se" });

        var other = await target.SaveAsync(GlobalAdministrator, new Person { FirstName = "Erik", LastName = "Berg", CityName = "Göteborg", CountryId = 1, EmailAddresses = "Anna@Example.se" });
        var shared = await target.SaveAsync(GlobalAdministrator, new Person { FirstName = "Erik", LastName = "Berg", CityName = "Göteborg", CountryId = 1, EmailAddresses = "erik@example.se; anna@example.se" });
        var herself = await target.SaveAsync(GlobalAdministrator, new Person { Id = 10, FirstName = "Anna", LastName = "Berg", CityName = "Göteborg", CountryId = 1, UserId = 5, EmailAddresses = "anna@example.se", MiddleName = "Maria" });

        Assert.AreEqual(0, other.Count);
        Assert.AreEqual(string.Format(ModulesRegistry.Resources.Strings.EmailAddressIsAnotherUsersSignIn, "anna@example.se"), other.Message);
        Assert.AreEqual(0, shared.Count, "Not as a second address either.");
        Assert.AreEqual(1, herself.Count, herself.Message);
    }
}