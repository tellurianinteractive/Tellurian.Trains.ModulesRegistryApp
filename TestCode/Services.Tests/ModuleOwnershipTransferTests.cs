using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Resources;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may transfer module ownership, and how (decided 2026-11-21): a person owner to another person; a group's data administrator
/// from the group to a member, or from members to the group; a country administrator all that in their country; only a global
/// administrator to an owner in another country. A group owns a whole module alone.
/// </summary>
[TestClass]
public sealed class ModuleOwnershipTransferTests
{
    private const int Sweden = 1;
    private const int Norway = 2;
    private const int Owner = 1;
    private const int OtherOwner = 2;
    private const int Member = 3;
    private const int DataAdministrator = 4;
    private const int Outsider = 5;
    private const int Norwegian = 6;
    private const int TheGroup = 1;
    private const int OtherGroup = 2;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId, int countryId = Sweden, params Claim[] role) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, personId.ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. role,
    ], "Test"));

    private static ClaimsPrincipal CountryAdministrator(int countryId) => Principal(Outsider, countryId, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal GlobalAdministrator => Principal(Outsider, Sweden, new Claim(AppClaimTypes.GlobalAdministrator, "true"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", PreferredTheme = "Europe" });
        await _factory.SeedAsync([.. Enumerable.Range(1, 6).Select(id => (object)new Person { Id = id, FirstName = "First", LastName = $"Last{id}", CityName = "City", CountryId = id == Norwegian ? Norway : Sweden })]);
        await _factory.SeedAsync(NewGroup(TheGroup), NewGroup(OtherGroup));
        await _factory.SeedAsync(
            new GroupMember { Id = 1, GroupId = TheGroup, PersonId = Owner },
            new GroupMember { Id = 2, GroupId = TheGroup, PersonId = OtherOwner },
            new GroupMember { Id = 3, GroupId = TheGroup, PersonId = Member },
            new GroupMember { Id = 4, GroupId = TheGroup, PersonId = DataAdministrator, IsDataAdministrator = true },
            new GroupMember { Id = 5, GroupId = TheGroup, PersonId = Norwegian });
        await _factory.SeedAsync(new Module { Id = 1, FullName = "Module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task OwnerTransfersPartOfTheShareToAnotherPerson()
    {
        await SeedOwnershipsAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 1 });

        var (count, message, _) = await NewService().TransferOwnershipAsync(Principal(Owner), 1, ToPerson(Outsider, 0.5));

        Assert.IsGreaterThan(0, count, message);
        await AssertSharesAsync((Owner, null, 0.5), (Outsider, null, 0.5));
    }

    [TestMethod]
    public async Task OthersMayNotTransferAPersonsShare()
    {
        await SeedOwnershipsAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 1 });

        var byOutsider = await NewService().TransferOwnershipAsync(Principal(Outsider), 1, ToPerson(Outsider, 1));
        var byDataAdministrator = await NewService().TransferOwnershipAsync(Principal(DataAdministrator), 1, ToPerson(Outsider, 1));
        var byOtherCountryAdministrator = await NewService().TransferOwnershipAsync(CountryAdministrator(Norway), 1, ToPerson(Outsider, 1));

        Assert.AreEqual(Strings.NotAuthorized, byOutsider.Message);
        Assert.AreEqual(Strings.NotAuthorized, byDataAdministrator.Message, "Not a person's share.");
        Assert.AreEqual(Strings.NotAuthorized, byOtherCountryAdministrator.Message);
        await AssertSharesAsync((Owner, null, 1));
    }

    [TestMethod]
    public async Task OwnerMayNotTransferMoreThanTheShare()
    {
        await SeedOwnershipsAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 2, ModuleId = 1, PersonId = OtherOwner, OwnedShare = 0.5 });

        var (count, _, _) = await NewService().TransferOwnershipAsync(Principal(Owner), 1, ToPerson(Outsider, 1));

        Assert.AreEqual(0, count);
        await AssertSharesAsync((Owner, null, 0.5), (OtherOwner, null, 0.5));
    }

    [TestMethod]
    public async Task CountryAdministratorTransfersWithinTheCountry()
    {
        await SeedOwnershipsAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 1 });

        var (count, message, _) = await NewService().TransferOwnershipAsync(CountryAdministrator(Sweden), 1, ToPerson(Outsider, 1));

        Assert.IsGreaterThan(0, count, message);
        await AssertSharesAsync((Outsider, null, 1));
    }

    [TestMethod]
    public async Task OnlyGlobalAdministratorTransfersToAnotherCountry()
    {
        await SeedOwnershipsAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 1 });

        var byOwner = await NewService().TransferOwnershipAsync(Principal(Owner), 1, ToPerson(Norwegian, 1));
        var byCountryAdministrator = await NewService().TransferOwnershipAsync(CountryAdministrator(Sweden), 1, ToPerson(Norwegian, 1));
        var byGlobalAdministrator = await NewService().TransferOwnershipAsync(GlobalAdministrator, 1, ToPerson(Norwegian, 1));

        Assert.AreEqual(Strings.OnlyGlobalAdministratorTransfersBetweenCountries, byOwner.Message);
        Assert.AreEqual(Strings.OnlyGlobalAdministratorTransfersBetweenCountries, byCountryAdministrator.Message);
        Assert.IsGreaterThan(0, byGlobalAdministrator.Count, byGlobalAdministrator.Message);
        await AssertSharesAsync((Norwegian, null, 1));
    }

    [TestMethod]
    public async Task DataAdministratorTransfersGroupsModuleToMember()
    {
        await SeedOwnershipsAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, GroupId = TheGroup, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = 1, PersonId = Member, OwnedShare = 0 });

        var toOutsider = await NewService().TransferOwnershipAsync(Principal(DataAdministrator), 1, ToPerson(Outsider, 1));
        var byMember = await NewService().TransferOwnershipAsync(Principal(Member), 1, ToPerson(Member, 1));
        var (count, message, _) = await NewService().TransferOwnershipAsync(Principal(DataAdministrator), 1, ToPerson(Member, 1));

        Assert.AreEqual(Strings.NewOwnerMustBeGroupMember, toOutsider.Message);
        Assert.AreEqual(Strings.NotAuthorized, byMember.Message, "Only a data administrator.");
        Assert.IsGreaterThan(0, count, message);
        await AssertSharesAsync((Member, null, 1));
    }

    [TestMethod]
    public async Task DataAdministratorTransfersMembersModuleToGroup()
    {
        await SeedOwnershipsAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 2, ModuleId = 1, PersonId = OtherOwner, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 3, ModuleId = 1, PersonId = Outsider, OwnedShare = 0 });

        var (count, message, _) = await NewService().TransferOwnershipAsync(Principal(DataAdministrator), 1, ToGroup(TheGroup));

        Assert.IsGreaterThan(0, count, message);
        await AssertSharesAsync((null, TheGroup, 1), (Outsider, null, 0));
    }

    [TestMethod]
    public async Task ModuleWithOwnerOutsideTheGroupIsNotTransferredToTheGroup()
    {
        await SeedOwnershipsAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 2, ModuleId = 1, PersonId = Outsider, OwnedShare = 0.5 });

        var (_, message, _) = await NewService().TransferOwnershipAsync(GlobalAdministrator, 1, ToGroup(TheGroup));

        Assert.AreEqual(Strings.AllOwnersMustBeGroupMembers, message);
    }

    [TestMethod]
    public async Task GroupsModuleIsNotTransferredToAnotherGroup()
    {
        await SeedOwnershipsAsync(new ModuleOwnership { Id = 1, ModuleId = 1, GroupId = TheGroup, OwnedShare = 1 });

        var (_, message, _) = await NewService().TransferOwnershipAsync(GlobalAdministrator, 1, ToGroup(OtherGroup));

        Assert.AreEqual(Strings.TransferBetweenGroupsIsNotSupported, message);
    }

    [TestMethod]
    public async Task OnlyThoseWhoMayTransferAddAssistants()
    {
        await SeedOwnershipsAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = Owner, OwnedShare = 1 });
        var assistant = new ModuleOwnership { ModuleId = 1, PersonId = Member, OwnedShare = 0 };

        var byOutsider = await NewService().AddAssistantAsync(Principal(Outsider), assistant);
        var byOwner = await NewService().AddAssistantAsync(Principal(Owner), assistant);

        Assert.AreEqual(0, byOutsider.Count);
        Assert.IsGreaterThan(0, byOwner.Count, byOwner.Message);
    }

    private ModuleOwnershipService NewService() => new(_factory);

    private static ModuleOwnership ToPerson(int personId, double share) => new() { ModuleId = 1, PersonId = personId, OwnedShare = share };
    private static ModuleOwnership ToGroup(int groupId) => new() { ModuleId = 1, GroupId = groupId, OwnedShare = 1 };

    private Task SeedOwnershipsAsync(params ModuleOwnership[] ownerships) => _factory.SeedAsync([.. ownerships]);

    private async Task AssertSharesAsync(params (int? PersonId, int? GroupId, double Share)[] expected)
    {
        await using var dbContext = _factory.CreateDbContext();
        var actual = await dbContext.ModuleOwnerships.AsNoTracking().OrderBy(mo => mo.GroupId).ThenBy(mo => mo.PersonId).ToListAsync();
        var sorted = expected.OrderBy(e => e.GroupId).ThenBy(e => e.PersonId).ToList();
        Assert.HasCount(sorted.Count, actual);
        for (var i = 0; i < sorted.Count; i++)
        {
            Assert.AreEqual(sorted[i].PersonId, actual[i].PersonId);
            Assert.AreEqual(sorted[i].GroupId, actual[i].GroupId);
            Assert.AreEqual(sorted[i].Share, actual[i].OwnedShare, 0.001);
        }
    }

    private static Group NewGroup(int id) => new()
    {
        Id = id,
        CountryId = Sweden,
        FullName = $"Group {id}",
        ShortName = $"G{id}",
        Category = "Club",
        CityName = "City",
    };
}