using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may save and delete a station and its tracks (Migration step 3.6): those who may edit the station (MayEditStationAsync), and for
/// a new station those who may edit its module. Before, saving used the page's owner reference with any station id, a track could be
/// changed or deleted by any signed-in user, and a station by any user who could see it.
/// </summary>
[TestClass]
public sealed class StationSaveTests
{
    private const int CountryId = 1, OtherCountryId = 2;
    private const int Owner = 1, Stranger = 2;
    private const int OwnersStation = 1, FreeModule = 2, StrangersFreeModule = 3;
    private const int Track = 1;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId, int countryId = CountryId, params Claim[] extra) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, personId.ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        .. extra,
    ], "Test"));

    private static ClaimsPrincipal CountryAdministrator(int countryId) => Principal(Stranger, countryId, new Claim(AppClaimTypes.CountryAdministrator, "true"));

    private StationService Stations => new(_factory, new SystemTimeProvider());
    private StationTrackService Tracks => new(_factory);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = OtherCountryId, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Region { Id = 1, CountryId = CountryId, LocalName = "Region", ForeColor = "#000000", BackColor = "#ffffff" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync(
            new Person { Id = Owner, FirstName = "Anna", LastName = "Berg", CityName = "City", CountryId = CountryId },
            new Person { Id = Stranger, FirstName = "Erik", LastName = "Ek", CityName = "City", CountryId = CountryId });
        await _factory.SeedAsync(NewModule(OwnersStation), NewModule(FreeModule), NewModule(StrangersFreeModule));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = OwnersStation, PersonId = Owner, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = FreeModule, PersonId = Owner, OwnedShare = 1 },
            new ModuleOwnership { Id = 3, ModuleId = StrangersFreeModule, PersonId = Stranger, OwnedShare = 1 });
        await _factory.SeedAsync(new Station { Id = OwnersStation, FullName = "Station", Signature = "S", RegionId = 1, PrimaryModuleId = OwnersStation });
        await _factory.SeedAsync(new StationTrack { Id = Track, StationId = OwnersStation, Designation = "1", DisplayOrder = 1, MaxTrainLength = 3, DirectionId = 10 });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static Module NewModule(int id) => new() { Id = id, FullName = $"Module {id}", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 };
    private static Station Changed(string name = "Changed") => new() { Id = OwnersStation, FullName = name, Signature = "S", RegionId = 1, PrimaryModuleId = FreeModule };
    private static Station New() => new() { FullName = "New station", Signature = "N", RegionId = 1 };

    [TestMethod]
    public async Task AnotherUserMayNotChangeSomeonesStation()
    {
        Assert.AreEqual(0, (await Stations.SaveAsync(Principal(Stranger), Changed())).Count);
        Assert.AreEqual("Station", (await _factory.FindAsync<Station>(OwnersStation))!.FullName);
    }

    [TestMethod]
    public async Task TheOwnerChangesTheStationButNotItsModule()
    {
        Assert.AreEqual(1, (await Stations.SaveAsync(Principal(Owner), Changed())).Count);
        var stored = (await _factory.FindAsync<Station>(OwnersStation))!;
        Assert.AreEqual("Changed", stored.FullName);
        Assert.AreEqual(OwnersStation, stored.PrimaryModuleId);
    }

    [TestMethod]
    public async Task ACountryAdministratorChangesStationsOnlyInTheirCountry()
    {
        Assert.AreEqual(0, (await Stations.SaveAsync(CountryAdministrator(OtherCountryId), Changed())).Count);
        Assert.AreEqual(1, (await Stations.SaveAsync(CountryAdministrator(CountryId), Changed())).Count);
    }

    [TestMethod]
    public async Task ANewStationIsAddedOnlyToAModuleTheUserMayEdit()
    {
        Assert.AreEqual(0, (await Stations.SaveAsync(Principal(Owner), New(), StrangersFreeModule)).Count);
        var result = await Stations.SaveAsync(Principal(Owner), New(), FreeModule);
        Assert.IsGreaterThan(0, result.Count, result.Message);
        Assert.AreEqual(result.Entity!.Id, (await _factory.FindAsync<Module>(FreeModule))!.StationId);
    }

    [TestMethod]
    public async Task ANewStationIsNotAddedToAModuleThatHasOne()
    {
        await Stations.SaveAsync(Principal(Owner), New(), FreeModule);
        Assert.AreEqual(0, (await Stations.SaveAsync(Principal(Owner), New(), FreeModule)).Count);
    }

    [TestMethod]
    public async Task AnotherUserMayNotChangeOrDeleteSomeonesTrack()
    {
        var track = new StationTrack { Id = Track, StationId = OwnersStation, Designation = "X", DisplayOrder = 1, MaxTrainLength = 3, DirectionId = 10 };
        Assert.AreEqual(0, (await Tracks.SaveOrUpdateAsync(Principal(Stranger), track)).Count);
        Assert.AreEqual(0, (await Tracks.DeleteAsync(Principal(Stranger), Track)).Count);
        Assert.AreEqual("1", (await _factory.FindAsync<StationTrack>(Track))!.Designation);
    }

    [TestMethod]
    public async Task TheOwnerChangesAddsAndDeletesTracks()
    {
        var changed = new StationTrack { Id = Track, StationId = 99, Designation = "1a", DisplayOrder = 1, MaxTrainLength = 3, DirectionId = 10 };
        Assert.AreEqual(1, (await Tracks.SaveOrUpdateAsync(Principal(Owner), changed)).Count);
        var stored = (await _factory.FindAsync<StationTrack>(Track))!;
        Assert.AreEqual("1a", stored.Designation);
        Assert.AreEqual(OwnersStation, stored.StationId, "A track never moves to another station.");

        var added = await Tracks.SaveOrUpdateAsync(Principal(Owner), new StationTrack { StationId = OwnersStation, Designation = "2", DisplayOrder = 2, MaxTrainLength = 3, DirectionId = 99 });
        Assert.AreEqual(1, added.Count);
        Assert.AreEqual((int)StationTrackDirection.Bidirectional, added.Entity!.DirectionId, "An unknown direction is corrected.");
        Assert.AreEqual(1, (await Tracks.DeleteAsync(Principal(Owner), added.Entity.Id)).Count);
    }

    [TestMethod]
    public async Task AUserMayNotAddATrackToSomeonesStation() =>
        Assert.AreEqual(0, (await Tracks.SaveOrUpdateAsync(Principal(Stranger), new StationTrack { StationId = OwnersStation, Designation = "9", DisplayOrder = 9, MaxTrainLength = 3, DirectionId = 10 })).Count);

    [TestMethod]
    public async Task OnlyThoseWhoMayEditDeleteAStation()
    {
        Assert.IsLessThanOrEqualTo(0, (await Stations.DeleteAsync(Principal(Stranger), OwnersStation)).Count);
        Assert.IsNotNull(await _factory.FindAsync<Station>(OwnersStation));
        Assert.IsGreaterThan(0, (await Stations.DeleteAsync(Principal(Owner), OwnersStation)).Count);
        Assert.IsNull(await _factory.FindAsync<Station>(OwnersStation));
    }
}