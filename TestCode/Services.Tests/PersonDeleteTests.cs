using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Deleting a person deletes everything that belongs to the person and changes what the person shares with others (decided
/// 2026-11-21). Only a country administrator in the person's country may do it. SQLite has no triggers, so this tests what the
/// service writes, not what the SQL Server triggers delete below it.
/// </summary>
[TestClass]
public sealed class PersonDeleteTests
{
    private const int Sweden = 1;
    private const int Norway = 2;
    private const int ThePerson = 1;
    private const int OtherPerson = 2;
    private const int Administrator = 3;
    private const int TheUser = 11;
    private const int TheGroup = 1;
    private const int OtherGroup = 2;
    private const int PastMeeting = 1;
    private const int UpcomingMeeting = 2;
    private const int PastLayout = 1;
    private SqliteDbContextFactory _factory = null!;
    private readonly TestTime _time = new() { LocalTime = new DateTime(2026, 11, 21, 12, 0, 0) };

    private static ClaimsPrincipal Principal(int countryId, params Claim[] role) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.PersonId, Administrator.ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. role,
    ], "Test"));

    private static ClaimsPrincipal CountryAdministrator => Principal(Sweden, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal OtherCountryAdministrator => Principal(Norway, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal GlobalAdministrator => Principal(Sweden, new Claim(AppClaimTypes.GlobalAdministrator, "true"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync(new User { Id = TheUser, ObjectId = Guid.NewGuid(), EmailAddress = "anna@example.se", RegistrationTime = DateTimeOffset.Now });
        await _factory.SeedAsync(
            new Person { Id = ThePerson, FirstName = "Anna", LastName = "Berg", CityName = "Göteborg", CountryId = Sweden, UserId = TheUser },
            new Person { Id = OtherPerson, FirstName = "Erik", LastName = "Ek", CityName = "Malmö", CountryId = Sweden },
            new Person { Id = Administrator, FirstName = "Admin", LastName = "Istrator", CityName = "Lund", CountryId = Sweden });
        await _factory.SeedAsync(
            new Group { Id = TheGroup, CountryId = Sweden, FullName = "Modulgruppen", ShortName = "MG", Category = "Club", CityName = "City" },
            new Group { Id = OtherGroup, CountryId = Sweden, FullName = "Andra gruppen", ShortName = "AG", Category = "Club", CityName = "City" });
        await _factory.SeedAsync(
            NewMeeting(PastMeeting, new DateTime(2025, 5, 1)),
            NewMeeting(UpcomingMeeting, new DateTime(2026, 12, 10), OtherGroup));
        await _factory.SeedAsync(new Layout { Id = PastLayout, MeetingId = PastMeeting, OrganisingGroupId = TheGroup, PrimaryModuleStandardId = 1, RegistrationOpeningDate = new DateTime(2025, 1, 1), RegistrationClosingDate = new DateTime(2025, 4, 1), ShortName = "Bana" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task OnlyCountryAdministratorInPersonsCountryMayDelete()
    {
        var target = NewService();

        Assert.IsNull(await target.GetDeletionAsync(GlobalAdministrator, ThePerson), "Not a global administrator by that right alone.");
        Assert.IsNull(await target.GetDeletionAsync(OtherCountryAdministrator, ThePerson));
        Assert.AreEqual(0, (await target.DeleteAsync(GlobalAdministrator, ThePerson)).Count);
        Assert.AreEqual(0, (await target.DeleteAsync(OtherCountryAdministrator, ThePerson)).Count);
        Assert.IsNotNull(await _factory.FindAsync<Person>(ThePerson));
        Assert.IsNotNull(await target.GetDeletionAsync(CountryAdministrator, ThePerson));
    }

    [TestMethod]
    public async Task EverythingThatBelongsToThePersonIsDeleted()
    {
        await _factory.SeedAsync(new GroupMember { Id = 1, GroupId = TheGroup, PersonId = ThePerson });
        await _factory.SeedAsync(new MeetingParticipant { Id = 1, MeetingId = PastMeeting, PersonId = ThePerson, RegistrationTime = DateTimeOffset.Now });
        await _factory.SeedAsync(new LayoutParticipant { Id = 1, LayoutId = PastLayout, MeetingParticipantId = 1, PersonId = ThePerson });
        await _factory.SeedAsync(NewModule(1));
        await _factory.SeedAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = ThePerson, OwnedShare = 1 });
        await _factory.SeedAsync(new Station { Id = 1, FullName = "Ekeby", Signature = "Eb", PrimaryModuleId = 1 });
        await SetModuleStationAsync(1, 1);
        await _factory.SeedAsync(new LayoutModule { Id = 1, LayoutParticipantId = 1, ModuleId = 1, RegisteredTime = DateTimeOffset.Now });
        await _factory.SeedAsync(new Vehicle { Id = 1, OwningPersonId = ThePerson, ScaleId = 1, KeeperSignature = "SJ", VehicleClass = "Rc", VehicleNumber = "1", Theme = "SE", InventoryNumber = 1 });
        await _factory.SeedAsync(new WiFredThrottle { Id = 1, OwningPersonId = ThePerson, MacAddress = "00:00:00:00:00:01", Name = "W", RegistrationDateTime = DateTimeOffset.Now, InventoryNumber = 1 });
        await SetLayoutContactAsync(PastLayout, ThePerson);
        var target = NewService();

        var (count, message) = await target.DeleteAsync(CountryAdministrator, ThePerson);

        Assert.IsGreaterThan(0, count, message);
        await using var dbContext = _factory.CreateDbContext();
        Assert.IsFalse(dbContext.People.Any(p => p.Id == ThePerson));
        Assert.IsFalse(dbContext.Users.IgnoreQueryFilters().Any(u => u.Id == TheUser));
        Assert.IsFalse(dbContext.GroupMembers.Any());
        Assert.IsFalse(dbContext.MeetingParticipants.Any());
        Assert.IsFalse(dbContext.LayoutParticipants.Any());
        Assert.IsFalse(dbContext.LayoutModules.Any());
        Assert.IsFalse(dbContext.Modules.Any());
        Assert.IsFalse(dbContext.ModuleOwnerships.Any());
        Assert.IsFalse(dbContext.Stations.Any());
        Assert.IsFalse(dbContext.Vehicles.Any());
        Assert.IsFalse(dbContext.WiFredThrottles.Any());
        Assert.IsNull(dbContext.Layouts.Single().ContactPersonId, "The layout stays, without contact.");
        Assert.IsTrue(dbContext.ChangeLogs.Any(c => c.EntityType == nameof(Person) && c.EntityId == ThePerson), "Written through the change tracker.");
    }

    [TestMethod]
    public async Task SharedModuleKeepsTheOtherOwnersWithTheWholeShare()
    {
        await _factory.SeedAsync(NewModule(1), NewModule(2));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = ThePerson, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 2, ModuleId = 1, PersonId = OtherPerson, OwnedShare = 0.25 },
            new ModuleOwnership { Id = 3, ModuleId = 1, GroupId = TheGroup, OwnedShare = 0.25 },
            new ModuleOwnership { Id = 4, ModuleId = 2, PersonId = OtherPerson, OwnedShare = 1 },
            new ModuleOwnership { Id = 5, ModuleId = 2, PersonId = ThePerson, OwnedShare = 0 });
        var target = NewService();

        var deletion = (await target.GetDeletionAsync(CountryAdministrator, ThePerson))!;
        var (count, message) = await target.DeleteAsync(CountryAdministrator, ThePerson);

        CollectionAssert.AreEqual(new[] { "Module 1" }, deletion.ModulesShared.ToArray());
        CollectionAssert.AreEqual(new[] { "Module 2" }, deletion.ModulesAssisted.ToArray());
        Assert.IsEmpty(deletion.ModulesDeleted);
        Assert.IsGreaterThan(0, count, message);
        await using var dbContext = _factory.CreateDbContext();
        Assert.AreEqual(2, dbContext.Modules.Count());
        Assert.AreEqual(0.5, dbContext.ModuleOwnerships.Single(mo => mo.Id == 2).OwnedShare, 1e-9);
        Assert.AreEqual(0.5, dbContext.ModuleOwnerships.Single(mo => mo.Id == 3).OwnedShare, 1e-9);
        Assert.IsFalse(dbContext.ModuleOwnerships.Any(mo => mo.PersonId == ThePerson));
    }

    [TestMethod]
    public async Task StationStaysWithAnotherOwnersModule()
    {
        await _factory.SeedAsync(NewModule(1), NewModule(2));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = ThePerson, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = 2, PersonId = OtherPerson, OwnedShare = 1 });
        await _factory.SeedAsync(new Station { Id = 1, FullName = "Ekeby", Signature = "Eb", PrimaryModuleId = 1 });
        await SetModuleStationAsync(1, 1);
        await SetModuleStationAsync(2, 1);
        var target = NewService();

        var deletion = (await target.GetDeletionAsync(CountryAdministrator, ThePerson))!;
        var (count, message) = await target.DeleteAsync(CountryAdministrator, ThePerson);

        CollectionAssert.AreEqual(new[] { "Ekeby" }, deletion.StationsKept.ToArray());
        Assert.IsEmpty(deletion.StationsDeleted);
        Assert.IsGreaterThan(0, count, message);
        var station = (await _factory.FindAsync<Station>(1))!;
        Assert.AreEqual(2, station.PrimaryModuleId);
        Assert.IsNull(await _factory.FindAsync<Module>(1));
        Assert.AreEqual(1, (await _factory.FindAsync<Module>(2))!.StationId);
    }

    [TestMethod]
    public async Task RegistrationForUpcomingMeetingPreventsDelete()
    {
        await _factory.SeedAsync(new MeetingParticipant { Id = 1, MeetingId = UpcomingMeeting, PersonId = ThePerson, RegistrationTime = DateTimeOffset.Now });
        var target = NewService();

        var deletion = (await target.GetDeletionAsync(CountryAdministrator, ThePerson))!;
        var (count, _) = await target.DeleteAsync(CountryAdministrator, ThePerson);

        Assert.IsFalse(deletion.MayDelete);
        Assert.AreEqual(string.Format(ModulesRegistry.Resources.Strings.PersonIsRegisteredForUpcomingMeeting, "Meeting 2 2026"), deletion.Obstacles.Single());
        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Person>(ThePerson));
    }

    [TestMethod]
    public async Task CountryAdministratorMayNotDeleteThemselves()
    {
        var target = NewService();

        var deletion = (await target.GetDeletionAsync(CountryAdministrator, Administrator))!;

        Assert.AreEqual(ModulesRegistry.Resources.Strings.YouMayNotDeleteYourself, deletion.Obstacles.Single());
        Assert.AreEqual(0, (await target.DeleteAsync(CountryAdministrator, Administrator)).Count);
    }

    [TestMethod]
    public async Task OwnedModuleIsDeletedAloneWithItsStation()
    {
        await _factory.SeedAsync(NewModule(1), NewModule(2));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = ThePerson, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = 2, PersonId = ThePerson, OwnedShare = 1 });
        await _factory.SeedAsync(new Station { Id = 1, FullName = "Ekeby", Signature = "Eb", PrimaryModuleId = 1 });
        await SetModuleStationAsync(1, 1);
        var target = NewService();

        var listed = (await target.GetDeletionAsync(CountryAdministrator, ThePerson))!.ModulesDeleted;
        var (count, message) = await target.DeleteOwnedModuleAsync(CountryAdministrator, ThePerson, 1);

        Assert.AreEqual(new PersonDeletion.OwnedModule(1, "Module 1", "Ekeby", false), listed[0]);
        Assert.IsGreaterThan(0, count, message);
        Assert.IsNull(await _factory.FindAsync<Module>(1));
        Assert.IsNull(await _factory.FindAsync<Station>(1));
        Assert.IsNotNull(await _factory.FindAsync<Module>(2), "Only the chosen module.");
        Assert.IsNotNull(await _factory.FindAsync<Person>(ThePerson), "The person stays.");
    }

    [TestMethod]
    public async Task ModuleRegisteredForUpcomingMeetingMayOnlyBeTransferred()
    {
        await _factory.SeedAsync(NewModule(1));
        await _factory.SeedAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = ThePerson, OwnedShare = 1 });
        // Lent to another participant.
        await _factory.SeedAsync(new Layout { Id = 2, MeetingId = UpcomingMeeting, OrganisingGroupId = OtherGroup, PrimaryModuleStandardId = 1, RegistrationOpeningDate = new DateTime(2026, 10, 1), RegistrationClosingDate = new DateTime(2026, 12, 1) });
        await _factory.SeedAsync(new MeetingParticipant { Id = 1, MeetingId = UpcomingMeeting, PersonId = OtherPerson, RegistrationTime = DateTimeOffset.Now });
        await _factory.SeedAsync(new LayoutParticipant { Id = 1, LayoutId = 2, MeetingParticipantId = 1, PersonId = OtherPerson });
        await _factory.SeedAsync(new LayoutModule { Id = 1, LayoutParticipantId = 1, ModuleId = 1, RegisteredTime = DateTimeOffset.Now });
        var target = NewService();

        var deletion = (await target.GetDeletionAsync(CountryAdministrator, ThePerson))!;
        var (count, _) = await target.DeleteOwnedModuleAsync(CountryAdministrator, ThePerson, 1);

        Assert.IsTrue(deletion.ModulesDeleted.Single().IsRegisteredForUpcomingMeeting);
        Assert.IsFalse(deletion.MayDelete);
        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Module>(1));
    }

    [TestMethod]
    public async Task OnlyCountryAdministratorInPersonsCountryMayDeleteOwnedModule()
    {
        await _factory.SeedAsync(NewModule(1));
        await _factory.SeedAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = ThePerson, OwnedShare = 1 });
        var target = NewService();

        Assert.AreEqual(0, (await target.DeleteOwnedModuleAsync(GlobalAdministrator, ThePerson, 1)).Count);
        Assert.AreEqual(0, (await target.DeleteOwnedModuleAsync(OtherCountryAdministrator, ThePerson, 1)).Count);
        Assert.AreEqual(0, (await target.DeleteOwnedModuleAsync(CountryAdministrator, OtherPerson, 1)).Count, "Not a module the person owns.");
        Assert.IsNotNull(await _factory.FindAsync<Module>(1));
    }

    [TestMethod]
    public async Task MemberOfGroupOrganisingUpcomingMeetingIsDeletedAndLeavesTheGroup()
    {
        await _factory.SeedAsync(new GroupMember { Id = 1, GroupId = OtherGroup, PersonId = ThePerson });
        var target = NewService();

        var deletion = (await target.GetDeletionAsync(CountryAdministrator, ThePerson))!;
        var (count, message) = await target.DeleteAsync(CountryAdministrator, ThePerson);

        Assert.IsTrue(deletion.MayDelete, string.Join(" ", deletion.Obstacles));
        Assert.IsGreaterThan(0, count, message);
        await using var dbContext = _factory.CreateDbContext();
        Assert.IsFalse(dbContext.GroupMembers.Any());
    }

    [TestMethod]
    public async Task SharedModuleRegisteredForUpcomingMeetingGoesToTheOtherOwner()
    {
        await _factory.SeedAsync(new GroupMember { Id = 1, GroupId = OtherGroup, PersonId = ThePerson });
        await _factory.SeedAsync(NewModule(1));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = ThePerson, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 2, ModuleId = 1, PersonId = OtherPerson, OwnedShare = 0.5 });
        await _factory.SeedAsync(new Layout { Id = 2, MeetingId = UpcomingMeeting, OrganisingGroupId = OtherGroup, PrimaryModuleStandardId = 1, RegistrationOpeningDate = new DateTime(2026, 10, 1), RegistrationClosingDate = new DateTime(2026, 12, 1) });
        await _factory.SeedAsync(new MeetingParticipant { Id = 1, MeetingId = UpcomingMeeting, PersonId = OtherPerson, RegistrationTime = DateTimeOffset.Now });
        await _factory.SeedAsync(new LayoutParticipant { Id = 1, LayoutId = 2, MeetingParticipantId = 1, PersonId = OtherPerson });
        await _factory.SeedAsync(new LayoutModule { Id = 1, LayoutParticipantId = 1, ModuleId = 1, RegisteredTime = DateTimeOffset.Now });
        var target = NewService();

        var deletion = (await target.GetDeletionAsync(CountryAdministrator, ThePerson))!;
        var (count, message) = await target.DeleteAsync(CountryAdministrator, ThePerson);

        Assert.IsTrue(deletion.MayDelete, string.Join(" ", deletion.Obstacles));
        Assert.IsGreaterThan(0, count, message);
        await using var dbContext = _factory.CreateDbContext();
        Assert.AreEqual(1.0, dbContext.ModuleOwnerships.Single().OwnedShare, 1e-9);
        Assert.AreEqual(1, dbContext.LayoutModules.Count(), "The registration stays.");
    }

    private PersonService NewService() => new(_factory, _time);

    private async Task SetModuleStationAsync(int moduleId, int stationId)
    {
        await using var dbContext = _factory.CreateDbContext();
        dbContext.Modules.Single(m => m.Id == moduleId).StationId = stationId;
        await dbContext.SaveChangesAsync();
    }

    private async Task SetLayoutContactAsync(int layoutId, int personId)
    {
        await using var dbContext = _factory.CreateDbContext();
        dbContext.Layouts.Single(l => l.Id == layoutId).ContactPersonId = personId;
        await dbContext.SaveChangesAsync();
    }

    private static Module NewModule(int id) => new() { Id = id, FullName = $"Module {id}", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 };

    private static Meeting NewMeeting(int id, DateTime start, int organiserGroupId = TheGroup) => new()
    {
        Id = id,
        OrganiserGroupId = organiserGroupId,
        Name = $"Meeting {id}",
        CityName = "City",
        StartDate = start,
        EndDate = start.AddDays(2),
    };

    private sealed class TestTime : ITimeProvider
    {
        public DateTime LocalTime { get; set; }
        public DateTimeOffset Now => new(LocalTime, TimeSpan.FromHours(1));
        public DateOnly Today => DateOnly.FromDateTime(LocalTime);
    }
}