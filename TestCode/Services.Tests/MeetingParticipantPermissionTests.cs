using Microsoft.Extensions.Logging.Abstractions;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may manage a meeting's participants, their layout registrations and modules (decided 2026-11-18): an organiser of the
/// meeting at any time, also after registration has closed and for persons from any country (a global administrator, a country
/// administrator in the organising group's country, or a meeting administrator of the organising group or of a layout's group),
/// and the participant themselves while registration is open. Before, these writes only required a signed-in user, and on the
/// module registration page any country administrator counted as an organiser of any meeting.
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
    [DataRow(LayoutGroupMeetingAdministrator, true, DisplayName = "Meeting administrator of a layout's group, in another country")]
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
    [DataRow(Participant, true, false, DisplayName = "The participant")]
    [DataRow(OrganiserGroupMeetingAdministrator, true, true, DisplayName = "Meeting administrator of the organising group")]
    [DataRow(LayoutGroupMeetingAdministrator, true, true, DisplayName = "Meeting administrator of a layout's group")]
    [DataRow(Stranger, false, false, DisplayName = "Another user")]
    public async Task MayManageParticipant(int personId, bool whileOpen, bool afterClosing)
    {
        await using var dbContext = _factory.CreateDbContext();

        Assert.AreEqual(whileOpen, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, WhileOpen), "While open.");
        Assert.AreEqual(whileOpen, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, WhileOpen, OrganiserLayout), "Layout, while open.");
        Assert.AreEqual(afterClosing, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, AfterClosing), "After closing.");
        Assert.AreEqual(afterClosing, await dbContext.MayManageParticipantAsync(Principal(personId), TheParticipant, AfterClosing, OrganiserLayout), "Layout, after closing.");
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
        var byOrganiser = await target.AddPackageModulesAsync(Principal(LayoutGroupMeetingAdministrator), TheLayoutParticipant, Package());
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