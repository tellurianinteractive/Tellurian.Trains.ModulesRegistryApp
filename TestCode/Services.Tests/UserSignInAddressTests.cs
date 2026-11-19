using Microsoft.Extensions.Logging.Abstractions;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// A user has one email address, the sign-in address; a person may have several (Migration step 3.5, decided 2026-11-19). A
/// sign-in address belongs to one user and their own person only: it is not another user's sign-in address, nor among another
/// person's addresses, and a whole address is matched, so "a@b.se" isn't "aa@b.se".
/// </summary>
[TestClass]
public sealed class UserSignInAddressTests
{
    private SqliteDbContextFactory _factory = null!;
    private UserService Target => new(_factory, NullLogger<UserService>.Instance);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = 1, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
        await _factory.SeedAsync(new User { Id = 5, ObjectId = Guid.NewGuid(), EmailAddress = "anna@example.se", RegistrationTime = DateTimeOffset.Now });
        await _factory.SeedAsync(
            new Person { Id = 10, FirstName = "Anna", LastName = "Berg", CountryId = 1, UserId = 5, EmailAddresses = "anna@example.se" },
            new Person { Id = 11, FirstName = "Erik", LastName = "Berg", CountryId = 1, EmailAddresses = "erik@example.se; familjen@example.se" },
            new Person { Id = 12, FirstName = "Eva", LastName = "Berg", CountryId = 1, EmailAddresses = "familjen@example.se" },
            new Person { Id = 13, FirstName = "Olle", LastName = "Ek", CountryId = 1, EmailAddresses = "aerik@example.se" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task ASignInAddressIsNotChangedToAnAddressInUse()
    {
        await _factory.SeedAsync(new User { Id = 6, ObjectId = Guid.NewGuid(), EmailAddress = "olle@example.se", RegistrationTime = DateTimeOffset.Now });

        Assert.IsNull(await Target.SetEmailAsync(6, "anna@example.se"), "Another user's sign-in address.");
        Assert.IsNull(await Target.SetEmailAsync(6, "Erik@Example.se"), "Another person's address.");
        Assert.IsNotNull(await Target.SetEmailAsync(5, "anna@example.se"), "The user's own address.");
    }

    [TestMethod]
    public async Task AUserIsCreatedOnlyForTheOnePersonWithTheWholeAddress()
    {
        var erik = await Target.FindOrCreateAsync("erik@example.se", null);
        var family = await Target.FindOrCreateAsync("familjen@example.se", null);

        Assert.IsNotNull(erik);
        Assert.AreEqual(11, (await _factory.FindAsync<Person>(11))!.UserId == erik.Id ? 11 : 0, "Erik's person, not Olle's with \"aerik@example.se\".");
        Assert.IsNull((await _factory.FindAsync<Person>(13))!.UserId);
        Assert.IsNull(family, "An address shared by two persons can't be a sign-in address.");
    }
}