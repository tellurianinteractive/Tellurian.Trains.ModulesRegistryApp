using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// A group may be deleted from the groups list when it has no members, no modules and no meetings, by a global
/// administrator or a country administrator in the group's country.
/// </summary>
[TestClass]
public sealed class GroupDeleteTests
{
    private const int CountryId = 1;
    private const int OtherCountryId = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(params Claim[] role) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.CountryId, CountryId.ToString()),
        new Claim(AppClaimTypes.PersonId, "999"),
        .. role,
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator => Principal(new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal CountryAdministrator => Principal(new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal User => Principal();

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = OtherCountryId, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(NewGroup(1), NewGroup(2), NewGroup(3, OtherCountryId));
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task GroupWithoutMembersModulesAndMeetingsIsDeleted()
    {
        var target = new GroupService(_factory);

        var (count, _) = await target.DeleteAsync(GlobalAdministrator, 1);

        Assert.AreEqual(1, count);
        Assert.IsNull(await _factory.FindAsync<Group>(1));
    }

    [TestMethod]
    public async Task GroupWithMemberIsNotDeletable()
    {
        await _factory.SeedAsync(new Person { Id = 1, FirstName = "First", LastName = "Last", CityName = "City", CountryId = CountryId });
        await _factory.SeedAsync(new GroupMember { Id = 1, GroupId = 1, PersonId = 1 });

        await AssertNotDeletableAsync(1);
    }

    [TestMethod]
    public async Task GroupWithModuleIsNotDeletable()
    {
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync(new Module { Id = 1, FullName = "Module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 });
        await _factory.SeedAsync(new ModuleOwnership { Id = 1, ModuleId = 1, GroupId = 1, OwnedShare = 1 });

        await AssertNotDeletableAsync(1);
    }

    [TestMethod]
    public async Task GroupWithMeetingIsNotDeletable()
    {
        await _factory.SeedAsync(new Meeting { Id = 1, OrganiserGroupId = 1, Name = "Meeting", CityName = "City", StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 1, 3) });

        await AssertNotDeletableAsync(1);
    }

    [TestMethod]
    public async Task CountryAdministratorMayDeleteOnlyInTheOwnCountry()
    {
        var target = new GroupService(_factory);

        var deletable = await target.DeletableIdsAsync(CountryAdministrator, [1, 2, 3]);
        var (count, _) = await target.DeleteAsync(CountryAdministrator, 3);

        CollectionAssert.AreEquivalent(new[] { 1, 2 }, deletable.ToArray());
        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Group>(3));
    }

    [TestMethod]
    public async Task UserMayNotDelete()
    {
        var target = new GroupService(_factory);

        var deletable = await target.DeletableIdsAsync(User, [1, 2, 3]);
        var (count, _) = await target.DeleteAsync(User, 1);

        Assert.IsEmpty(deletable);
        Assert.IsLessThanOrEqualTo(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Group>(1));
    }

    [TestMethod]
    public async Task GlobalAdministratorMayDeleteInAllCountries()
    {
        var deletable = await new GroupService(_factory).DeletableIdsAsync(GlobalAdministrator, [1, 2, 3]);

        CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, deletable.ToArray());
    }

    private async Task AssertNotDeletableAsync(int groupId)
    {
        var target = new GroupService(_factory);

        var deletable = await target.DeletableIdsAsync(GlobalAdministrator, [1, 2]);
        var (count, _) = await target.DeleteAsync(GlobalAdministrator, groupId);

        Assert.DoesNotContain(groupId, deletable);
        Assert.Contains(2, deletable);
        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Group>(groupId));
    }

    private static Group NewGroup(int id, int countryId = CountryId) => new()
    {
        Id = id,
        CountryId = countryId,
        FullName = $"Group {id}",
        ShortName = $"G{id}",
        Category = "Club",
        CityName = "City",
    };
}