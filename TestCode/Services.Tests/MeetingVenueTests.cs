using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// A meeting's registered venue: only a locality marked as venue in the meeting's country (the organiser group's). Its name and
/// city become the meeting's venue and city when saved.
/// </summary>
[TestClass]
public sealed class MeetingVenueTests
{
    private const int Sweden = 1, Norway = 2;
    private const int SwedishClub = 1, NorwegianClub = 2;
    private const int SwedishVenue = 1, SwedishStorage = 2, NorwegianVenue = 3;
    private const int MeetingId = 1;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal GlobalAdministrator => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.PersonId, "999"),
        new Claim(AppClaimTypes.CountryId, Sweden.ToString()),
        new Claim(AppClaimTypes.GlobalAdministrator, "true"),
    ], "Test"));

    private MeetingService Meetings => new(_factory, new SystemTimeProvider());
    private LocalityService Localities => new(_factory);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(
            new Group { Id = SwedishClub, CountryId = Sweden, FullName = "Club", ShortName = "C", Category = "Club", CityName = "City" },
            new Group { Id = NorwegianClub, CountryId = Norway, FullName = "Klubb", ShortName = "K", Category = "Club", CityName = "By" });
        await _factory.SeedAsync(
            new Locality { Id = SwedishVenue, CountryId = Sweden, Name = "Sporthallen", CityName = "Värnamo", IsMeetingVenue = true },
            new Locality { Id = SwedishStorage, CountryId = Sweden, Name = "Garage", CityName = "Jönköping", IsModuleStorage = true },
            new Locality { Id = NorwegianVenue, CountryId = Norway, Name = "Idrettshallen", CityName = "Arendal", IsMeetingVenue = true });
        await _factory.SeedAsync(new Meeting { Id = MeetingId, OrganiserGroupId = SwedishClub, Name = "Meeting", CityName = "City", StartDate = new DateTime(2027, 1, 1), EndDate = new DateTime(2027, 1, 3) });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task OnlyVenuesInTheOrganisersCountry()
    {
        CollectionAssert.AreEquivalent(new[] { SwedishVenue }, await VenueIdsAsync(SwedishClub, null));
        CollectionAssert.AreEquivalent(new[] { NorwegianVenue }, await VenueIdsAsync(NorwegianClub, null));
    }

    [TestMethod]
    public async Task TheCurrentVenueIsKeptInTheList()
    {
        CollectionAssert.AreEquivalent(new[] { SwedishVenue, NorwegianVenue }, await VenueIdsAsync(SwedishClub, NorwegianVenue));
    }

    [TestMethod]
    public async Task TheVenuesNameAndCityAreTheMeetings()
    {
        var (count, _) = await SaveWithVenueAsync(SwedishVenue);

        var saved = await _factory.FindAsync<Meeting>(MeetingId);
        Assert.AreEqual(1, count);
        Assert.AreEqual(SwedishVenue, saved!.VenueLocalityId);
        Assert.AreEqual("Sporthallen", saved.VenueName);
        Assert.AreEqual("Värnamo", saved.CityName);
    }

    [TestMethod]
    [DataRow(SwedishStorage)]
    [DataRow(NorwegianVenue)]
    public async Task OnlyAVenueInTheMeetingsCountryIsSaved(int localityId)
    {
        var (count, _) = await SaveWithVenueAsync(localityId);

        Assert.AreEqual(0, count);
        Assert.IsNull((await _factory.FindAsync<Meeting>(MeetingId))!.VenueLocalityId);
    }

    [TestMethod]
    public async Task WithoutVenueTheTextsAreKept()
    {
        var (count, _) = await SaveWithVenueAsync(null, "Folkets hus");

        var saved = await _factory.FindAsync<Meeting>(MeetingId);
        Assert.AreEqual(1, count);
        Assert.AreEqual("Folkets hus", saved!.VenueName);
    }

    private async Task<int[]> VenueIdsAsync(int organiserGroupId, int? currentVenueId) =>
        [.. (await Localities.VenueListboxItemsAsync(GlobalAdministrator, organiserGroupId, currentVenueId)).Select(i => i.Id)];

    private async Task<(int Count, string Message)> SaveWithVenueAsync(int? localityId, string? venueName = null)
    {
        var meeting = (await _factory.FindAsync<Meeting>(MeetingId))!;
        meeting.VenueLocalityId = localityId;
        if (venueName is not null) meeting.VenueName = venueName;
        var (count, message, _) = await Meetings.SaveAsync(GlobalAdministrator, meeting);
        return (count, message);
    }
}