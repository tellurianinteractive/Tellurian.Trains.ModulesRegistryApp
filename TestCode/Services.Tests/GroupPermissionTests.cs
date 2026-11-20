using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may change a group, its members and its layout modules (release 1.15.25). Before, any user who wasn't read-only could save
/// any group, any user could add members to any group and change any group's layout modules, and a member of another group could
/// be changed through a group the user administered.
/// </summary>
[TestClass]
public sealed class GroupPermissionTests
{
    private const int Sweden = 1, Norway = 2;
    private const int GroupAdministrator = 1, DataAdministrator = 2, Member = 3, Stranger = 4, OtherGroupMember = 5;
    private const int Group = 1, OtherGroup = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId, int countryId = Sweden, params Claim[] extra) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, personId.ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. extra,
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator => Principal(Stranger, Sweden, new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal CountryAdministrator(int countryId = Sweden) => Principal(Stranger, countryId, new Claim(AppClaimTypes.CountryAdministrator, "true"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new GroupDomain { Id = 1, Name = "Domain" }, new GroupDomain { Id = 2, Name = "Other domain" });
        await _factory.SeedAsync([.. Enumerable.Range(1, 5).Select(id => (object)new Person { Id = id, FirstName = "First", LastName = $"Last{id}", CityName = "City", CountryId = Sweden })]);
        await _factory.SeedAsync(
            new Group { Id = Group, CountryId = Sweden, FullName = "Group", ShortName = "G", Category = "Community", CityName = "City", GroupDomainId = 1 },
            new Group { Id = OtherGroup, CountryId = Sweden, FullName = "Other group", ShortName = "OG", Category = "Community", CityName = "City" });
        await _factory.SeedAsync(
            new GroupMember { Id = 1, GroupId = Group, PersonId = GroupAdministrator, IsGroupAdministrator = true },
            new GroupMember { Id = 2, GroupId = Group, PersonId = DataAdministrator, IsDataAdministrator = true },
            new GroupMember { Id = 3, GroupId = Group, PersonId = Member },
            new GroupMember { Id = 5, GroupId = OtherGroup, PersonId = OtherGroupMember });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, MainThemeId = 1 });
        await _factory.SeedAsync(
            new Module { Id = 1, FullName = "Member's module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 },
            new Module { Id = 2, FullName = "Someone else's module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 });
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Member, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = 2, PersonId = OtherGroupMember, OwnedShare = 1 });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static Group Changed(int countryId = Sweden, int? domainId = 1) =>
        new() { Id = Group, CountryId = countryId, FullName = "Renamed", ShortName = "G", Category = "Community", CityName = "City", GroupDomainId = domainId };

    [TestMethod]
    public async Task AUserWhoDoesntAdministerTheGroupMayNotSaveIt()
    {
        var target = new GroupService(_factory);

        Assert.AreEqual(0, (await target.SaveAsync(Principal(Stranger), Changed())).Count);
        Assert.AreEqual(0, (await target.SaveAsync(Principal(Member), Changed())).Count);
        Assert.AreEqual(0, (await target.SaveAsync(CountryAdministrator(Norway), Changed())).Count, "A country administrator in another country.");
        Assert.AreEqual("Group", (await _factory.FindAsync<Group>(Group))!.FullName);
    }

    [TestMethod]
    public async Task AGroupAdministratorSavesTheGroupButNotItsCountryOrDomain()
    {
        var (count, _, _) = await new GroupService(_factory).SaveAsync(Principal(GroupAdministrator), Changed(Norway, domainId: 2));

        Assert.AreEqual(1, count);
        var saved = (await _factory.FindAsync<Group>(Group))!;
        Assert.AreEqual("Renamed", saved.FullName);
        Assert.AreEqual(Sweden, saved.CountryId);
        Assert.AreEqual(1, saved.GroupDomainId);
    }

    [TestMethod]
    public async Task ACountryAdministratorChangesTheDomainButNotTheCountry()
    {
        await new GroupService(_factory).SaveAsync(CountryAdministrator(), Changed(Norway, domainId: 2));

        var saved = (await _factory.FindAsync<Group>(Group))!;
        Assert.AreEqual(Sweden, saved.CountryId);
        Assert.AreEqual(2, saved.GroupDomainId);
    }

    [TestMethod]
    public async Task AGlobalAdministratorMovesAGroupToAnotherCountry()
    {
        await new GroupService(_factory).SaveAsync(GlobalAdministrator, Changed(Norway));

        Assert.AreEqual(Norway, (await _factory.FindAsync<Group>(Group))!.CountryId);
    }

    [TestMethod]
    public async Task OnlyAdministratorsOfTheCountryAddAGroup()
    {
        var target = new GroupService(_factory);
        static Group New(int countryId) => new() { CountryId = countryId, FullName = "New group", ShortName = "NG", Category = "Community", CityName = "City" };

        Assert.AreEqual(0, (await target.SaveAsync(Principal(GroupAdministrator), New(Sweden))).Count);
        Assert.AreEqual(0, (await target.SaveAsync(CountryAdministrator(), New(Norway))).Count);
        Assert.AreEqual(1, (await target.SaveAsync(CountryAdministrator(), New(Sweden))).Count);
    }

    [TestMethod]
    public async Task OnlyMemberAdministratorsAddMembers()
    {
        var target = new GroupService(_factory);

        Assert.AreEqual(0, (await target.AddMemberAsync(Principal(Stranger), new GroupMember { GroupId = Group, PersonId = Stranger })).Count, "The old rule let anyone.");
        Assert.AreEqual(0, (await target.AddMemberAsync(Principal(DataAdministrator), new GroupMember { GroupId = Group, PersonId = Stranger })).Count);
        Assert.AreEqual(1, (await target.AddMemberAsync(Principal(GroupAdministrator), new GroupMember { GroupId = Group, PersonId = Stranger })).Count);
    }

    [TestMethod]
    public async Task AMemberOfAnotherGroupIsNotChanged()
    {
        var target = new GroupService(_factory);

        // Member 5 belongs to the other group; the group administrator of group 1 posts it as a member of group 1.
        var (count, _, _) = await target.SaveMemberAsync(Principal(GroupAdministrator), new GroupMember { Id = 5, GroupId = Group, PersonId = OtherGroupMember, IsGroupAdministrator = true });

        Assert.AreEqual(0, count);
        var member = (await _factory.FindAsync<GroupMember>(5))!;
        Assert.AreEqual(OtherGroup, member.GroupId);
        Assert.IsFalse(member.IsGroupAdministrator);
    }

    [TestMethod]
    public async Task AGroupAdministratorChangesAMembersRightsButNotTheirPerson()
    {
        var (count, _, _) = await new GroupService(_factory).SaveMemberAsync(Principal(GroupAdministrator), new GroupMember { Id = 3, GroupId = Group, PersonId = Stranger, IsDataAdministrator = true });

        Assert.AreEqual(1, count);
        var member = (await _factory.FindAsync<GroupMember>(3))!;
        Assert.IsTrue(member.IsDataAdministrator);
        Assert.AreEqual(Member, member.PersonId);
    }

    [TestMethod]
    public async Task MemberAdministratorsRemoveMembers()
    {
        var target = new GroupService(_factory);

        Assert.AreEqual(0, (await target.RemoveMemberAsync(Principal(Stranger), 3)).Count);
        Assert.AreEqual(1, (await target.RemoveMemberAsync(Principal(GroupAdministrator), 3)).Count, "Group administrators see Remove.");
        Assert.IsNull(await _factory.FindAsync<GroupMember>(3));
    }

    [TestMethod]
    public async Task OnlyMemberAdministratorsChangeLayoutModules()
    {
        var target = new GroupLayoutModuleService(_factory);

        Assert.AreEqual(0, await target.AddModule(Principal(Stranger), Group, 1, groupMemberId: 3), "The old rule let anyone.");
        Assert.AreEqual(1, await target.AddModule(Principal(GroupAdministrator), Group, 1, groupMemberId: 3));
        Assert.AreEqual(0, await target.RemoveModule(Principal(Stranger), Group, 1));
        Assert.AreEqual(1, await target.RemoveModule(Principal(GroupAdministrator), Group, 1));
    }

    [TestMethod]
    public async Task ALayoutModuleMustBeOwnedByAMemberOfTheGroup()
    {
        var target = new GroupLayoutModuleService(_factory);

        Assert.AreEqual(0, await target.AddModule(Principal(GroupAdministrator), Group, 2, groupMemberId: 3), "Module 2 isn't the member's.");
        Assert.AreEqual(0, await target.AddModule(Principal(GroupAdministrator), Group, 2, groupMemberId: 5), "Member 5 is in another group.");
    }
}