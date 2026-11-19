using Microsoft.Extensions.Logging.Abstractions;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may do what with a meeting (decided 2026-11-19, <see cref="MeetingPermissionExtensions"/>): meeting organisers (a global
/// administrator, a country administrator in the arranging group's country, a meeting administrator of the arranging group)
/// handle everything; a layout group's meeting administrators handle their own layout and register and cancel meeting
/// participants; the participant handles their own registration while it is open. Before, these writes only required a
/// signed-in user, and on the module registration page any country administrator counted as an organiser of any meeting.
/// </summary>
[TestClass]
public sealed class MeetingParticipantPermissionTests
{
    private const int Sweden = 1, Norway = 2;
    private const int Participant = 1, OrganiserGroupMeetingAdministrator = 2, LayoutGroupMeetingAdministrator = 3, OrganiserGroupMember = 4, Stranger = 5, NorwegianPerson = 6;
    private const int OrganiserGroup = 1, LayoutGroup = 2;
    private const int TheMeeting = 1, OtherMeeting = 2;
    private const int OrganiserLayout = 1, LayoutGroupsLayout = 2, OtherMeetingsLayout = 3;
    private const int TheParticipant = 1, NorwegianParticipant = 2;
    private const int TheLayoutParticipant = 1, TheLayoutModule = 1, TheModule = 1;

    private static readonly DateTime WhileOpen = new(2026, 11, 15, 12, 0, 0);
    private static readonly DateTime AfterClosing = new(2026, 12, 5, 12, 0, 0);

    private SqliteDbContextFactory _factory = null!;
    private readonly TestTime _time = new();

    private static ClaimsPrincipal Principal(int personId, int countryId = Sweden, params Claim[] extra) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, personId.ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. extra,
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator => Principal(Stranger, Norway, new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal SwedishCountryAdministrator => Principal(Stranger, Sweden, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal NorwegianCountryAdministrator => Principal(Stranger, Norway, new Claim(AppClaimTypes.CountryAdministrator, "true"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _time.LocalTime = WhileOpen;
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync([.. Enumerable.Range(1, 6).Select(id => (object)new Person { Id = id, FirstName = "First", LastName = $"Last{id}", CityName = "City", CountryId = id == NorwegianPerson ? Norway : Sweden })]);
        await _factory.SeedAsync(NewGroup(OrganiserGroup, Sweden), NewGroup(LayoutGroup, Norway));
        await _factory.SeedAsync(
            new GroupMember { Id = 1, GroupId = OrganiserGroup, PersonId = OrganiserGroupMeetingAdministrator, IsMeetingAdministrator = true },
            new GroupMember { Id = 2, GroupId = LayoutGroup, PersonId = LayoutGroupMeetingAdministrator, IsMeetingAdministrator = true },
            new GroupMember { Id = 3, GroupId = OrganiserGroup, PersonId = OrganiserGroupMember, IsGroupAdministrator = true });
        await _factory.SeedAsync(NewMeeting(TheMeeting), NewMeeting(OtherMeeting, LayoutGroup));
        await _factory.SeedAsync(NewLayout(OrganiserLayout, TheMeeting, OrganiserGroup), NewLayout(LayoutGroupsLayout, TheMeeting, LayoutGroup), NewLayout(OtherMeetingsLayout, OtherMeeting, LayoutGroup));
        await _factory.SeedAsync(NewParticipant(TheParticipant, Participant), NewParticipant(NorwegianParticipant, NorwegianPerson));
        await _factory.SeedAsync(new Module { Id = TheModule, FullName = "Module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 });
        await _factory.SeedAsync(new LayoutParticipant { Id = TheLayoutParticipant, LayoutId = OrganiserLayout, MeetingParticipantId = TheParticipant, PersonId = Participant });
        await _factory.SeedAsync(new LayoutModule { Id = TheLayoutModule, LayoutParticipantId = TheLayoutParticipant, ModuleId = TheModule, RegisteredTime = WhileOpen });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    [DataRow(OrganiserGroupMeetingAdministrator, true, DisplayName = "Meeting administrator of the organising group")]
    [DataRow(LayoutGroupMeetingAdministrator, false, DisplayName = "Meeting administrator of a layout's group")]
    [DataRow(OrganiserGroupMember, false, DisplayName = "Group administrator, not meeting administrator")]
    [DataRow(Participant, false, DisplayName = "A participant")]
    [DataRow(Stranger, false, DisplayName = "Another user")]
    public async Task IsMeetingOrganiser(int personId, bool expected)
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.AreEqual(expected, await dbContext.IsMeetingOrganiserAsync(Principal(personId), TheMeeting));
    }

    [TestMethod]
    public async Task AdministratorsAreOrganisersOnlyInTheOrganisingGroupsCountry()
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.IsTrue(await dbContext.IsMeetingOrganiserAsync(GlobalAdministrator, TheMeeting));
        Assert.IsTrue(await dbContext.IsMeetingOrganiserAsync(SwedishCountryAdministrator, TheMeeting));
        Assert.IsFalse(await dbContext.IsMeetingOrganiserAsync(NorwegianCountryAdministrator, TheMeeting), "A country administrator in another country.");
    }

    [TestMethod]
    public async Task ReadOnlyUsersAndUnknownMeetingsAreNotOrganised()
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.IsFalse(await dbContext.IsMeetingOrganiserAsync(Principal(OrganiserGroupMeetingAdministrator, extra: new Claim(AppClaimTypes.ReadOnly, "true")), TheMeeting));
        Assert.IsFalse(await dbContext.IsMeetingOrganiserAsync(GlobalAdministrator, 99));
    }

    [TestMethod]
    public async Task TheMeetingServiceUsesTheDatabaseNotTheLoadedMeeting()
    {
        var target = new MeetingService(_factory, _time);
        var meetingWithoutOrganiserGroup = new Meeting { Id = TheMeeting, OrganiserGroupId = OrganiserGroup };

        Assert.IsFalse(await target.IsAdministratorOrMeetingOrganiser(NorwegianCountryAdministrator, meetingWithoutOrganiserGroup),
            "Without its organising group loaded, the check fell back to the user's own country.");
    }

    [TestMethod]
    [DataRow(Participant, true, false, true, false, DisplayName = "The participant")]
    [DataRow(OrganiserGroupMeetingAdministrator, true, true, true, true, DisplayName = "Meeting administrator of the organising group")]
    [DataRow(LayoutGroupMeetingAdministrator, true, true, false, false, DisplayName = "Meeting administrator of another layout's group")]
    [DataRow(Stranger, false, false, false, false, DisplayName = "Another user")]
    public async Task MayManageParticipant(int personId, bool whileOpen, bool afterClosing, bool layoutWhileOpen, bool layoutAfterClosing)
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.AreEqual(whileOpen, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, WhileOpen), "While open.");
        Assert.AreEqual(layoutWhileOpen, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, WhileOpen, OrganiserLayout), "Layout, while open.");
        Assert.AreEqual(afterClosing, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, AfterClosing), "After closing.");
        Assert.AreEqual(layoutAfterClosing, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, AfterClosing, OrganiserLayout), "Layout, after closing.");
    }

    [TestMethod]
    public async Task ALayoutGroupsMeetingAdministratorHandlesOnlyTheirOwnLayout()
    {
        await using var dbContext = _factory.CreateDbContext();
        var layoutAdministrator = Principal(LayoutGroupMeetingAdministrator, Norway);

        Assert.IsTrue(await dbContext.IsLayoutOrganiserAsync(layoutAdministrator, LayoutGroupsLayout), "Their own layout.");
        Assert.IsFalse(await dbContext.IsLayoutOrganiserAsync(layoutAdministrator, OrganiserLayout), "Another layout.");
        Assert.IsTrue(await dbContext.IsLayoutOrganiserAsync(Principal(OrganiserGroupMeetingAdministrator), LayoutGroupsLayout), "The arranging group's meeting administrator.");
        Assert.IsTrue(await dbContext.MayManageMeetingParticipantsAsync(layoutAdministrator, TheMeeting), "Registers and cancels meeting participants.");
        Assert.IsFalse(await dbContext.MayManageMeetingParticipantsAsync(Principal(Stranger), TheMeeting));
    }

    [TestMethod]
    public async Task SavingALayout()
    {
        var target = new LayoutService(_factory, _time, NullLogger<LayoutService>.Instance);
        async Task<Layout> Stored(int id) { await using var dbContext = _factory.CreateDbContext(); return (await dbContext.Layouts.FindAsync(id))!; }

        var ownLayout = await Stored(LayoutGroupsLayout); ownLayout.ShortName = "Changed";
        var otherLayout = await Stored(OrganiserLayout); otherLayout.ShortName = "Changed";
        var regrouped = await Stored(LayoutGroupsLayout); regrouped.OrganisingGroupId = OrganiserGroup;
        var newLayout = NewLayout(0, TheMeeting, LayoutGroup);

        Assert.AreEqual(1, (await target.SaveAsync(Principal(LayoutGroupMeetingAdministrator, Norway), ownLayout)).Count, "Their own layout.");
        Assert.AreEqual(0, (await target.SaveAsync(Principal(LayoutGroupMeetingAdministrator, Norway), otherLayout)).Count, "Another layout.");
        Assert.AreEqual(0, (await target.SaveAsync(Principal(LayoutGroupMeetingAdministrator, Norway), regrouped)).Count, "Another organising group.");
        Assert.AreEqual(0, (await target.SaveAsync(Principal(LayoutGroupMeetingAdministrator, Norway), newLayout)).Count, "A new layout.");
        Assert.AreEqual(0, (await target.SaveAsync(NorwegianCountryAdministrator, otherLayout)).Count, "A country administrator in another country.");
        Assert.AreEqual(1, (await target.SaveAsync(SwedishCountryAdministrator, otherLayout)).Count, "A country administrator in the arranging group's country.");
        Assert.AreEqual(1, (await target.SaveAsync(Principal(OrganiserGroupMeetingAdministrator), newLayout)).Count, "A new layout by the arranging group.");
    }

    [TestMethod]
    public async Task SavingAMeeting()
    {
        var target = new MeetingService(_factory, _time);
        async Task<Meeting> Stored() => (await target.FindByIdWithLayoutsAsync(TheMeeting))!;

        var renamed = await Stored(); renamed.Name = "Renamed";
        Assert.AreEqual(0, (await target.SaveAsync(Principal(LayoutGroupMeetingAdministrator, Norway), renamed)).Count, "A layout group's meeting administrator.");
        Assert.AreEqual(0, (await target.SaveAsync(NorwegianCountryAdministrator, renamed)).Count, "A country administrator in another country.");
        Assert.IsGreaterThan(0, (await target.SaveAsync(Principal(OrganiserGroupMeetingAdministrator), renamed)).Count, "The arranging group's meeting administrator.");
        var moved = await Stored(); moved.OrganiserGroupId = LayoutGroup;
        Assert.AreEqual(0, (await target.SaveAsync(SwedishCountryAdministrator, moved)).Count, "Moved to a group in another country.");
        Assert.AreEqual(0, (await target.SaveAsync(NorwegianCountryAdministrator, NewMeeting(0))).Count, "A new meeting for a group in another country.");
        Assert.AreEqual(1, (await target.SaveAsync(SwedishCountryAdministrator, NewMeeting(0))).Count, "A new meeting for a group in their country.");
    }

    [TestMethod]
    public async Task DeletingLayoutsAndMeetings()
    {
        var target = new MeetingService(_factory, _time);

        Assert.AreEqual(0, (await target.DeleteLayoutAsync(Principal(LayoutGroupMeetingAdministrator, Norway), TheMeeting, LayoutGroupsLayout)).Count, "A layout group's meeting administrator.");
        Assert.AreEqual(0, (await target.DeleteLayoutAsync(null, TheMeeting, LayoutGroupsLayout)).Count, "Not signed in.");
        Assert.IsFalse(await target.MayDeleteAsync(Principal(OrganiserGroupMeetingAdministrator), TheMeeting), "With registered participants.");
        Assert.AreEqual(1, (await target.DeleteLayoutAsync(Principal(OrganiserGroupMeetingAdministrator), TheMeeting, LayoutGroupsLayout)).Count, "The arranging group's meeting administrator.");
        Assert.IsFalse(await target.MayDeleteAsync(SwedishCountryAdministrator, OtherMeeting), "A country administrator in another country.");
        Assert.IsTrue(await target.MayDeleteAsync(Principal(LayoutGroupMeetingAdministrator, Norway), OtherMeeting), "The arranging group's meeting administrator, without participants.");
    }

    [TestMethod]
    public async Task ALayoutStationIsChangedOnlyByItsParticipantOrALayoutOrganiser()
    {
        await _factory.SeedAsync(new Station { Id = 1, FullName = "Station", Signature = "S", PrimaryModuleId = TheModule });
        await _factory.SeedAsync(new LayoutStation { Id = 1, LayoutParticipantId = TheLayoutParticipant, StationId = 1 });
        var target = new LayoutService(_factory, _time, NullLogger<LayoutService>.Instance);
        LayoutStation Changed() => new() { Id = 1, LayoutParticipantId = TheLayoutParticipant, StationId = 1, OtherName = "Other" };

        Assert.AreEqual(0, (await target.UpdateAsync(Principal(Stranger), Changed())).Count, "Another user.");
        Assert.AreEqual(0, (await target.UpdateAsync(Principal(LayoutGroupMeetingAdministrator, Norway), Changed())).Count, "Another layout's organiser.");
        Assert.AreEqual(1, (await target.UpdateAsync(Principal(Participant), Changed())).Count, "The participant.");
    }

    [TestMethod]
    public async Task ALayoutOfAnotherMeetingIsRefused()
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.IsFalse(await dbContext.MayManageParticipantAsync(GlobalAdministrator, TheParticipant, WhileOpen, OtherMeetingsLayout));
    }

    [TestMethod]
    public async Task AnOrganiserMayRegisterAParticipantFromAnotherCountryForALayoutAfterClosing()
    {
        _time.LocalTime = AfterClosing;
        var target = new LayoutParticipantService(_factory, _time);

        var refused = await target.SaveAsync(Principal(Participant), NewLayoutParticipant(NorwegianParticipant, NorwegianPerson));
        var saved = await target.SaveAsync(Principal(OrganiserGroupMeetingAdministrator), NewLayoutParticipant(NorwegianParticipant, NorwegianPerson));

        Assert.AreEqual(0, refused.Count);
        Assert.AreEqual(1, saved.Count);
    }

    [TestMethod]
    public async Task ALayoutRegistrationMustBelongToTheMeetingParticipantsPerson()
    {
        var target = new LayoutParticipantService(_factory, _time);

        var result = await target.SaveAsync(GlobalAdministrator, NewLayoutParticipant(TheParticipant, NorwegianPerson));

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task AnotherUserMayNotRemoveAParticipantsModule()
    {
        var target = new LayoutService(_factory, _time, NullLogger<LayoutService>.Instance);

        var result = await target.RemoveModuleWithStationAsync(Principal(Stranger), TheLayoutModule);

        Assert.AreEqual(0, result.Count);
        Assert.IsNotNull(await _factory.FindAsync<LayoutModule>(TheLayoutModule));
    }

    [TestMethod]
    public async Task AnOrganiserMayRemoveAParticipantsModuleAfterClosing()
    {
        _time.LocalTime = AfterClosing;
        var target = new LayoutService(_factory, _time, NullLogger<LayoutService>.Instance);

        var result = await target.RemoveModuleWithStationAsync(SwedishCountryAdministrator, TheLayoutModule);

        Assert.IsGreaterThan(0, result.Count, result.Message);
        Assert.IsNull(await _factory.FindAsync<LayoutModule>(TheLayoutModule));
    }

    [TestMethod]
    public async Task AnOrganiserMayAddModulesAfterClosingButOnlyWhenTheParticipantIsThereWhileModulesAreNeeded()
    {
        _time.LocalTime = AfterClosing;
        var target = new LayoutService(_factory, _time, NullLogger<LayoutService>.Instance);
        await using (var dbContext = _factory.CreateDbContext())
        {
            dbContext.LayoutModules.Remove((await dbContext.LayoutModules.FindAsync(TheLayoutModule))!);
            await dbContext.SaveChangesAsync();
        }

        var byStranger = await target.AddPackageModulesAsync(Principal(Stranger), TheLayoutParticipant, Package());
        var byOrganiser = await target.AddPackageModulesAsync(Principal(OrganiserGroupMeetingAdministrator), TheLayoutParticipant, Package());
        await using (var dbContext = _factory.CreateDbContext())
        {
            var participant = (await dbContext.MeetingParticipants.FindAsync(TheParticipant))!;
            participant.ParticipateDay3 = false; // Leaves before the end of the meeting.
            await dbContext.SaveChangesAsync();
        }
        var whenLeavingEarly = await target.AddPackageModulesAsync(GlobalAdministrator, TheLayoutParticipant, Package());

        Assert.AreEqual(0, byStranger.Count);
        Assert.AreEqual(1, byOrganiser.Count);
        Assert.AreEqual(0, whenLeavingEarly.Count, "The rule about the participant's times applies to organisers too.");
    }

    [TestMethod]
    public async Task CancellingARegistration()
    {
        var target = new MeetingService(_factory, _time);

        Assert.AreEqual(0, (await target.CancelMeetingParticipaction(Principal(Stranger), NorwegianParticipant)).Count, "Another user.");
        _time.LocalTime = AfterClosing;
        Assert.AreEqual(0, (await target.CancelMeetingParticipaction(Principal(NorwegianPerson, Norway), NorwegianParticipant)).Count, "The participant after closing.");
        Assert.AreEqual(1, (await target.CancelMeetingParticipaction(Principal(OrganiserGroupMeetingAdministrator), NorwegianParticipant)).Count, "An organiser after closing.");
    }

    [TestMethod]
    public async Task ARegistrationCannotBeMovedToAnotherPerson()
    {
        var target = new MeetingService(_factory, _time);
        var moved = NewParticipant(TheParticipant, Stranger);

        var result = await target.SaveAsync(Principal(Stranger), moved);

        Assert.AreEqual(0, result.Count);
        Assert.AreEqual(Participant, (await _factory.FindAsync<MeetingParticipant>(TheParticipant))!.PersonId);
    }

    [TestMethod]
    public async Task TheParticipantMayChangeTheRegistrationOnlyWhileOpen()
    {
        var target = new MeetingService(_factory, _time);
        var whileOpen = NewParticipant(TheParticipant, Participant);
        whileOpen.ParticipateDay3 = false;
        var afterClosing = NewParticipant(TheParticipant, Participant);

        var savedWhileOpen = await target.SaveAsync(Principal(Participant), whileOpen);
        _time.LocalTime = AfterClosing;
        var refusedAfterClosing = await target.SaveAsync(Principal(Participant), afterClosing);
        var savedByOrganiser = await target.SaveAsync(Principal(OrganiserGroupMeetingAdministrator), afterClosing);

        Assert.AreEqual(1, savedWhileOpen.Count, savedWhileOpen.Message);
        Assert.AreEqual(0, refusedAfterClosing.Count, "The participant after closing.");
        Assert.AreEqual(1, savedByOrganiser.Count, savedByOrganiser.Message);
    }

    private static ModulePackage Package() =>
        new(1, ModulePackageType.SingleModule, "Module", "Owner", [Participant], [new AvailableModule { ModuleId = TheModule, FullName = "Module", FirstName = "First", LastName = "Last1", CityName = "City" }]);

    private static Group NewGroup(int id, int countryId) => new() { Id = id, CountryId = countryId, FullName = $"Group {id}", ShortName = $"G{id}", Category = "Club", CityName = "City" };

    private static Meeting NewMeeting(int id, int organiserGroupId = OrganiserGroup) => new()
    {
        Id = id,
        OrganiserGroupId = organiserGroupId,
        Name = $"Meeting {id}",
        CityName = "City",
        VenueName = "Venue",
        StartDate = new DateTime(2026, 12, 10),
        EndDate = new DateTime(2026, 12, 12),
    };

    private static Layout NewLayout(int id, int meetingId, int groupId) => new()
    {
        Id = id,
        MeetingId = meetingId,
        OrganisingGroupId = groupId,
        PrimaryModuleStandardId = 1,
        RegistrationOpeningDate = new DateTime(2026, 11, 1),
        RegistrationClosingDate = new DateTime(2026, 12, 1),
        ShortName = $"Layout {id}",
    };

    private static MeetingParticipant NewParticipant(int id, int personId) => new()
    {
        Id = id,
        MeetingId = TheMeeting,
        PersonId = personId,
        ParticipateDay1 = true,
        ParticipateDay2 = true,
        ParticipateDay3 = true,
        EarliestDepartureTime = new TimeOnly(14, 0),
        RegistrationTime = WhileOpen,
    };

    private static LayoutParticipant NewLayoutParticipant(int meetingParticipantId, int personId) =>
        new() { LayoutId = LayoutGroupsLayout, MeetingParticipantId = meetingParticipantId, PersonId = personId };

    private sealed class TestTime : ITimeProvider
    {
        public DateTime LocalTime { get; set; }
        public DateTimeOffset Now => new(LocalTime, TimeSpan.FromHours(1));
        public DateOnly Today => DateOnly.FromDateTime(LocalTime);
    }
}