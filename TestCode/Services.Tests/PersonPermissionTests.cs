using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may save a person, their country and their user's rights (release 1.15.28). Before, a country administrator could save
/// persons in any country, a group administrator flag computed by the page let anyone administering some group save any person,
/// and every right posted was saved: in a static form all fields are posted, so the service decides.
/// </summary>
[TestClass]
public sealed class PersonPermissionTests
{
    private const int Sweden = 1, Norway = 2;
    private const int Self = 1, GroupAdministrator = 2, Member = 3, Stranger = 4, Norwegian = 5;
    private const int Group = 1;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId, int countryId = Sweden, params Claim[] extra) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, (personId + 10).ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. extra,
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator(int personId = Stranger) => Principal(personId, Sweden, new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal CountryAdministrator(int countryId = Sweden) => Principal(Stranger, countryId, new Claim(AppClaimTypes.CountryAdministrator, "true"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync([.. Enumerable.Range(1, 5).Select(id => (object)new User { Id = id + 10, ObjectId = Guid.NewGuid(), EmailAddress = $"user{id}@example.se", RegistrationTime = DateTimeOffset.Now })]);
        await _factory.SeedAsync([.. Enumerable.Range(1, 5).Select(id => (object)new Person { Id = id, FirstName = "First", LastName = $"Last{id}", CityName = "City", CountryId = id == Norwegian ? Norway : Sweden, UserId = id + 10, EmailAddresses = $"user{id}@example.se" })]);
        await _factory.SeedAsync(new Group { Id = Group, CountryId = Sweden, FullName = "Group", ShortName = "G", Category = "Community", CityName = "City" });
        await _factory.SeedAsync(
            new GroupMember { Id = 1, GroupId = Group, PersonId = GroupAdministrator, IsGroupAdministrator = true },
            new GroupMember { Id = 2, GroupId = Group, PersonId = Member });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private PersonService Target => new(_factory, new SystemTimeProvider());

    private async Task<Person> StoredAsync(int id)
    {
        using var dbContext = _factory.CreateDbContext();
        return await dbContext.People.Include(p => p.User).SingleAsync(p => p.Id == id);
    }

    private async Task<int> RenameAsync(ClaimsPrincipal principal, int id, int groupId = 0, Action<Person>? change = null)
    {
        var person = await StoredAsync(id);
        person.CityName = "Renamed";
        change?.Invoke(person);
        return (await Target.SaveAsync(principal, person, groupId)).Count;
    }

    [TestMethod]
    public async Task WhoMayInvite()
    {
        // Migration step 3.6: before, any signed-in user could invite anyone.
        var member = await StoredAsync(Member);
        Assert.IsTrue(await Target.MayInviteAsync(CountryAdministrator(), member), "A country administrator in the person's country.");
        Assert.IsFalse(await Target.MayInviteAsync(CountryAdministrator(Norway), member), "A country administrator in another country.");
        Assert.IsTrue(await Target.MayInviteAsync(Principal(GroupAdministrator), member), "Administers the members of the person's group.");
        Assert.IsFalse(await Target.MayInviteAsync(Principal(GroupAdministrator), await StoredAsync(Stranger)), "A person outside the group.");
        Assert.IsFalse(await Target.MayInviteAsync(Principal(Stranger), member), "Another user.");
    }

    [TestMethod]
    public async Task WhoMaySave()
    {
        Assert.AreEqual(1, await RenameAsync(GlobalAdministrator(), Norwegian));
        Assert.AreEqual(1, await RenameAsync(CountryAdministrator(), Member));
        Assert.AreEqual(0, await RenameAsync(CountryAdministrator(), Norwegian), "Another country: the old rule allowed it.");
        Assert.AreEqual(1, await RenameAsync(Principal(Self), Self), "The person themselves.");
        Assert.AreEqual(0, await RenameAsync(Principal(Stranger), Member));
        Assert.AreEqual(0, await RenameAsync(Principal(Self, Sweden, new Claim(AppClaimTypes.ReadOnly, "true")), Self), "Read-only.");
    }

    [TestMethod]
    public async Task AGroupAdministratorSavesTheGroupsMembersOnly()
    {
        Assert.AreEqual(1, await RenameAsync(Principal(GroupAdministrator), Member, Group));
        Assert.AreEqual(0, await RenameAsync(Principal(GroupAdministrator), Stranger, Group), "Not a member of the group.");
        Assert.AreEqual(0, await RenameAsync(Principal(Member), Stranger, Group), "Not a group administrator.");
    }

    [TestMethod]
    public async Task OnlyGlobalAndCountryAdministratorsAddPersons()
    {
        // 2026-11-24: before, a group's member administrator could add persons from any country.
        static Person New(int countryId) => new() { FirstName = "Ny", LastName = $"Person{countryId}", CityName = "City", CountryId = countryId };
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(GroupAdministrator), New(Sweden), Group)).Count, "A group's member administrator.");
        Assert.AreEqual(0, (await Target.SaveAsync(CountryAdministrator(), New(Norway))).Count, "A country administrator, in another country.");
        Assert.AreEqual(1, (await Target.SaveAsync(CountryAdministrator(), New(Sweden), Group)).Count, "A country administrator, in their own country.");
        Assert.AreEqual(1, (await Target.SaveAsync(GlobalAdministrator(), New(Norway))).Count, "A global administrator, in any country.");
    }

    [TestMethod]
    public async Task WhoChangesTheCountry()
    {
        Assert.AreEqual(0, await RenameAsync(Principal(Self), Self, change: p => p.CountryId = Norway), "The person themselves.");
        Assert.AreEqual(0, await RenameAsync(CountryAdministrator(), Member, change: p => p.CountryId = Norway), "To another country.");
        Assert.AreEqual(0, await RenameAsync(Principal(GroupAdministrator), Member, Group, p => p.CountryId = Norway), "A group's member administrator, for a stored member.");
        Assert.AreEqual(1, await RenameAsync(GlobalAdministrator(), Member, change: p => p.CountryId = Norway));
    }

    [TestMethod]
    public async Task RightsAUserMayNotChangeKeepTheirValues()
    {
        await RenameAsync(Principal(Self), Self, change: p =>
        {
            p.User.IsGlobalAdministrator = true;
            p.User.IsCountryAdministrator = true;
            p.User.IsApiAccessPermitted = true;
            p.User.MayManageWiFreds = true;
            p.User.AdministratorAreaOfResposibility = "Everything";
        });
        var user = (await StoredAsync(Self)).User;
        Assert.AreEqual("Renamed", (await StoredAsync(Self)).CityName, "The person is saved.");
        Assert.IsFalse(user.IsGlobalAdministrator);
        Assert.IsFalse(user.IsCountryAdministrator);
        Assert.IsFalse(user.IsApiAccessPermitted);
        Assert.IsFalse(user.MayManageWiFreds);
        Assert.IsNull(user.AdministratorAreaOfResposibility);
    }

    [TestMethod]
    public async Task ACountryAdministratorGrantsRightsButNotAdministration()
    {
        await RenameAsync(CountryAdministrator(), Member, change: p =>
        {
            p.User.IsCountryAdministrator = true;
            p.User.IsGlobalAdministrator = true;
            p.User.MayUploadSkpDrawing = true;
        });
        var user = (await StoredAsync(Member)).User;
        Assert.IsTrue(user.MayUploadSkpDrawing);
        Assert.IsFalse(user.IsCountryAdministrator);
        Assert.IsFalse(user.IsGlobalAdministrator);
    }

    [TestMethod]
    public async Task AGlobalAdministratorMakesCountryAdministratorsButGrantsNoGlobalRight()
    {
        await RenameAsync(GlobalAdministrator(), Member, change: p => { p.User.IsCountryAdministrator = true; p.User.IsGlobalAdministrator = true; });
        var user = (await StoredAsync(Member)).User;
        Assert.IsTrue(user.IsCountryAdministrator);
        Assert.IsFalse(user.IsGlobalAdministrator, "Only the super user.");
    }

    [TestMethod]
    public async Task OnlyTheSuperUserChangesTheGlobalAdministratorRight()
    {
        // User 1 is the super user (ClaimsPrincipalExtensions.IsSuperUser).
        var superUser = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
            new Claim(AppClaimTypes.UserId, "1"),
            new Claim(AppClaimTypes.CountryId, Sweden.ToString()),
            new Claim(AppClaimTypes.GlobalAdministrator, "true"),
        ], "Test"));
        await RenameAsync(superUser, Member, change: p => p.User.IsGlobalAdministrator = true);
        Assert.IsTrue((await StoredAsync(Member)).User.IsGlobalAdministrator, "Granted by the super user.");
        await RenameAsync(GlobalAdministrator(), Member, change: p => p.User.IsGlobalAdministrator = false);
        Assert.IsTrue((await StoredAsync(Member)).User.IsGlobalAdministrator, "Not removed by another global administrator.");
        await RenameAsync(superUser, Member, change: p => p.User.IsGlobalAdministrator = false);
        Assert.IsFalse((await StoredAsync(Member)).User.IsGlobalAdministrator, "Removed by the super user.");
    }

    [TestMethod]
    public async Task OnlyAnAdministratorsAreaOfResponsibilityIsSaved()
    {
        await RenameAsync(CountryAdministrator(), Member, change: p => p.User.AdministratorAreaOfResposibility = "Signals");
        Assert.IsNull((await StoredAsync(Member)).User.AdministratorAreaOfResposibility, "Not an administrator.");
        await RenameAsync(GlobalAdministrator(), Member, change: p => { p.User.IsCountryAdministrator = true; p.User.AdministratorAreaOfResposibility = "Signals"; });
        Assert.AreEqual("Signals", (await StoredAsync(Member)).User.AdministratorAreaOfResposibility, "Made a country administrator in the same save.");
        await RenameAsync(GlobalAdministrator(), Member, change: p => { p.User.IsCountryAdministrator = false; p.User.AdministratorAreaOfResposibility = null; });
        Assert.AreEqual("Signals", (await StoredAsync(Member)).User.AdministratorAreaOfResposibility, "Kept when no longer an administrator.");
    }

    [TestMethod]
    public async Task AnotherUserCanNotBeAttached()
    {
        var other = (await StoredAsync(Member)).User;
        Assert.AreEqual(0, await RenameAsync(GlobalAdministrator(), Self, change: p => p.User = other));
    }

    [TestMethod]
    public async Task TheSignInAddressFollowsTheFirstEmailAddress()
    {
        await RenameAsync(Principal(Self), Self, change: p => p.EmailAddresses = "Ny@Example.se, user1@example.se");
        Assert.AreEqual("ny@example.se", (await StoredAsync(Self)).User.EmailAddress);
    }
}