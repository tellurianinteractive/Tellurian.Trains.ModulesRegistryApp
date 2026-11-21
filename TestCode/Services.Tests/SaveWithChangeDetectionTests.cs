using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Migration step 2C.5a: saves load the existing row and copy the values with SetValues,
/// so EF knows what changed: a changed save updates the row, an unchanged save writes nothing (count -1),
/// and a new entity is added.
/// </summary>
[TestClass]
public sealed class SaveWithChangeDetectionTests
{
    private const int CountryId = 1;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal GlobalAdministrator => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.GlobalAdministrator, "true"),
        new Claim(AppClaimTypes.CountryId, CountryId.ToString()),
        new Claim(AppClaimTypes.PersonId, "999"),
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task GroupChangedIsSaved()
    {
        await _factory.SeedAsync(NewGroup(1));
        var target = new GroupService(_factory);
        var group = NewGroup(1);
        group.FullName = "Changed";

        var (count, _, entity) = await target.SaveAsync(GlobalAdministrator, group);

        Assert.AreEqual(1, count);
        Assert.IsNotNull(entity);
        Assert.AreEqual("Changed", (await _factory.FindAsync<Group>(1))?.FullName);
    }

    [TestMethod]
    public async Task GroupUnchangedIsNotSaved()
    {
        await _factory.SeedAsync(NewGroup(1));
        var target = new GroupService(_factory);

        var (count, _, entity) = await target.SaveAsync(GlobalAdministrator, NewGroup(1));

        Assert.AreEqual(-1, count);
        Assert.IsNotNull(entity);
    }

    [TestMethod]
    public async Task GroupNewIsAdded()
    {
        var target = new GroupService(_factory);

        var (count, _, entity) = await target.SaveAsync(GlobalAdministrator, NewGroup(0));

        Assert.AreEqual(1, count);
        Assert.IsNotNull(await _factory.FindAsync<Group>(entity!.Id));
    }

    [TestMethod]
    public async Task GroupMemberChangedIsSavedAndUnchangedIsNot()
    {
        await _factory.SeedAsync(NewGroup(1), NewPerson(1));
        await _factory.SeedAsync(NewGroupMember(1));
        var target = new GroupService(_factory);
        var member = NewGroupMember(1);
        member.IsDataAdministrator = true;

        var (changedCount, _, _) = await target.SaveMemberAsync(GlobalAdministrator, member);
        var (unchangedCount, _, _) = await target.SaveMemberAsync(GlobalAdministrator, member);

        Assert.AreEqual(1, changedCount);
        Assert.AreEqual(-1, unchangedCount);
        Assert.IsTrue((await _factory.FindAsync<GroupMember>(1))?.IsDataAdministrator);
    }

    [TestMethod]
    public async Task GroupMemberNewIsAdded()
    {
        await _factory.SeedAsync(NewGroup(1), NewPerson(1));
        var target = new GroupService(_factory);

        var (count, _, entity) = await target.SaveMemberAsync(GlobalAdministrator, NewGroupMember(0));

        Assert.AreEqual(1, count);
        Assert.IsNotNull(await _factory.FindAsync<GroupMember>(entity!.Id));
    }

    [TestMethod]
    public async Task ModuleStandardChangedIsSavedAndUnchangedIsNot()
    {
        await _factory.SeedAsync(NewScale());
        await _factory.SeedAsync(NewModuleStandard(1));
        var target = new ModuleStandardService(_factory);
        var standard = NewModuleStandard(1);
        standard.Couplings = "Changed";

        var (changedCount, _, _) = await target.SaveAsync(GlobalAdministrator, standard);
        var (unchangedCount, _, _) = await target.SaveAsync(GlobalAdministrator, standard);

        Assert.AreEqual(1, changedCount);
        Assert.AreEqual(-1, unchangedCount);
        Assert.AreEqual("Changed", (await _factory.FindAsync<ModuleStandard>(1))?.Couplings);
    }

    [TestMethod]
    public async Task ModuleStandardNewWithScaleIsAddedWithoutAddingTheScale()
    {
        await _factory.SeedAsync(NewScale());
        var target = new ModuleStandardService(_factory);
        var standard = NewModuleStandard(0);
        standard.Scale = NewScale();

        var (count, _, entity) = await target.SaveAsync(GlobalAdministrator, standard);

        Assert.AreEqual(1, count);
        Assert.IsNotNull(await _factory.FindAsync<ModuleStandard>(entity!.Id));
    }

    [TestMethod]
    public async Task PersonChangedIsSaved()
    {
        await _factory.SeedAsync(NewPerson(1));
        var target = new PersonService(_factory, new SystemTimeProvider());
        var person = NewPerson(1);
        person.CityName = "Changed";

        var (count, _, _) = await target.SaveAsync(GlobalAdministrator, person);

        Assert.AreEqual(1, count);
        Assert.AreEqual("Changed", (await _factory.FindAsync<Person>(1))?.CityName);
    }

    [TestMethod]
    public async Task PersonUnchangedIsNotSaved()
    {
        var person = NewPerson(1);
        person.User = NewUser(1);
        await _factory.SeedAsync(person);
        var target = new PersonService(_factory, new SystemTimeProvider());
        var unchanged = NewPerson(1);
        unchanged.UserId = 1;
        unchanged.User = NewUser(1);

        var (count, _, _) = await target.SaveAsync(GlobalAdministrator, unchanged);

        Assert.AreEqual(-1, count);
    }

    [TestMethod]
    public async Task PersonWithOnlyUserChangedIsSaved()
    {
        var person = NewPerson(1);
        person.User = NewUser(1);
        await _factory.SeedAsync(person);
        var target = new PersonService(_factory, new SystemTimeProvider());
        var changed = NewPerson(1);
        changed.UserId = 1;
        changed.User = NewUser(1);
        changed.User.MayUploadSkpDrawing = true;

        var (count, _, _) = await target.SaveAsync(GlobalAdministrator, changed);

        Assert.AreEqual(1, count);
        Assert.IsTrue((await _factory.FindAsync<User>(1))?.MayUploadSkpDrawing);
    }

    [TestMethod]
    public async Task PersonEmailAddressIsCopiedToUser()
    {
        var person = NewPerson(1);
        person.User = NewUser(1);
        await _factory.SeedAsync(person);
        var target = new PersonService(_factory, new SystemTimeProvider());
        var changed = NewPerson(1);
        changed.UserId = 1;
        changed.User = NewUser(1);
        changed.EmailAddresses = "new@example.com;other@example.com";

        var (count, _, _) = await target.SaveAsync(GlobalAdministrator, changed);

        Assert.IsGreaterThan(0, count);
        Assert.AreEqual("new@example.com", (await _factory.FindAsync<User>(1))?.EmailAddress);
    }

    [TestMethod]
    public async Task PersonNewIsAddedWithoutAUser()
    {
        // Users are created by invitation (UserService), never by saving a person (release 1.15.28).
        var target = new PersonService(_factory, new SystemTimeProvider());
        var person = NewPerson(0);
        person.User = NewUser(0);

        var (count, _, entity) = await target.SaveAsync(GlobalAdministrator, person);

        Assert.AreEqual(1, count);
        var saved = await _factory.FindAsync<Person>(entity!.Id);
        Assert.IsNull(saved?.UserId);
    }

    [TestMethod]
    public async Task PersonExistingGetsNoUserBySaving()
    {
        // Users are created by invitation (UserService), never by saving a person (release 1.15.28).
        await _factory.SeedAsync(NewPerson(1));
        var target = new PersonService(_factory, new SystemTimeProvider());
        var person = NewPerson(1);
        person.User = NewUser(0);

        var (count, _, _) = await target.SaveAsync(GlobalAdministrator, person);

        Assert.AreEqual(0, count);
        Assert.IsNull((await _factory.FindAsync<Person>(1))?.UserId);
    }

    [TestMethod]
    public async Task OwnershipPartlyTransferredToNewOwner()
    {
        await SeedModuleOwnedByAsync((Id: 1, PersonId: 1, Share: 1.0));
        var target = new ModuleOwnershipService(_factory);
        var origin = await _factory.FindAsync<ModuleOwnership>(1);

        var (count, _, module) = await target.TransferOwnershipAsync(GlobalAdministrator, origin!, new ModuleOwnership { ModuleId = 1, PersonId = 2, OwnedShare = 0.5 });

        Assert.AreEqual(2, count);
        Assert.HasCount(2, module!.ModuleOwnerships);
        Assert.AreEqual(0.5, module.ModuleOwnerships.Single(mo => mo.PersonId == 1).OwnedShare);
        Assert.AreEqual(0.5, module.ModuleOwnerships.Single(mo => mo.PersonId == 2).OwnedShare);
    }

    [TestMethod]
    public async Task OwnershipFullyTransferredToExistingOwnerDeletesOrigin()
    {
        await SeedModuleOwnedByAsync((Id: 1, PersonId: 1, Share: 0.5), (Id: 2, PersonId: 2, Share: 0.5));
        var target = new ModuleOwnershipService(_factory);
        var origin = await _factory.FindAsync<ModuleOwnership>(1);

        var (count, _, module) = await target.TransferOwnershipAsync(GlobalAdministrator, origin!, new ModuleOwnership { ModuleId = 1, PersonId = 2, OwnedShare = 0.5 });

        Assert.AreEqual(2, count);
        var ownership = module!.ModuleOwnerships.Single();
        Assert.AreEqual(2, ownership.PersonId);
        Assert.AreEqual(1.0, ownership.OwnedShare);
    }

    private async Task SeedModuleOwnedByAsync(params (int Id, int PersonId, double Share)[] owners)
    {
        await _factory.SeedAsync(NewScale(), NewPerson(1), NewPerson(2));
        await _factory.SeedAsync(NewModuleStandard(1));
        await _factory.SeedAsync(new Module { Id = 1, FullName = "Module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 });
        await _factory.SeedAsync([.. owners.Select(o => new ModuleOwnership { Id = o.Id, ModuleId = 1, PersonId = o.PersonId, OwnedShare = o.Share })]);
    }

    private static Group NewGroup(int id) => new()
    {
        Id = id,
        CountryId = CountryId,
        FullName = "Group",
        ShortName = "G",
        Category = "Club",
        CityName = "City",
    };

    private static GroupMember NewGroupMember(int id) => new()
    {
        Id = id,
        GroupId = 1,
        PersonId = 1,
    };

    private static Person NewPerson(int id) => new()
    {
        Id = id,
        FirstName = "First",
        LastName = "Last",
        CityName = "City",
        CountryId = CountryId,
    };

    private static User NewUser(int id) => new()
    {
        Id = id,
        ObjectId = new Guid("6f9619ff-8b86-d011-b42d-00cf4fc964ff"),
        EmailAddress = "user@example.com",
        RegistrationTime = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
    };

    private static Scale NewScale() => new() { Id = 1, ShortName = "H0", Denominator = 87 };

    private static ModuleStandard NewModuleStandard(int id) => new()
    {
        Id = id,
        ShortName = "Fremo H0",
        ScaleId = 1,
        TrackSystem = "Track",
        Wheelset = "RP25",
        Couplings = "Hooks",
        Electricity = "DC",
        PreferredTheme = "Europe",
        AcceptedNorm = "Fremo",
    };
}
