using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Where a module is stored: only a locality marked as storage, in the country of the module's owner (person or group), that the
/// user sees (a group's storage place only its members).
/// </summary>
[TestClass]
public sealed class ModuleStorageTests
{
    private const int Sweden = 1, Norway = 2;
    private const int Owner = 1, ClubMember = 2;
    private const int Club = 1, NorwegianClub = 2;
    private const int PersonsModule = 1, ClubsModule = 2, NorwegianClubsModule = 3;
    private const int SwedishStorage = 1, SwedishVenue = 2, NorwegianStorage = 3, ClubStorage = 4, NorwegianClubStorage = 5;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int personId) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, personId.ToString()),
        new Claim(AppClaimTypes.PersonId, personId.ToString()),
        new Claim(AppClaimTypes.CountryId, Sweden.ToString()),
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync(
            new Person { Id = Owner, FirstName = "Anna", LastName = "Berg", CityName = "City", CountryId = Sweden },
            new Person { Id = ClubMember, FirstName = "Erik", LastName = "Ek", CityName = "City", CountryId = Sweden });
        await _factory.SeedAsync(
            new Group { Id = Club, CountryId = Sweden, FullName = "Club", ShortName = "C", Category = "Club", CityName = "City" },
            new Group { Id = NorwegianClub, CountryId = Norway, FullName = "Klubb", ShortName = "K", Category = "Club", CityName = "By" });
        await _factory.SeedAsync(new GroupMember { Id = 1, GroupId = Club, PersonId = ClubMember, IsDataAdministrator = true });
        await _factory.SeedAsync(
            NewLocality(SwedishStorage, "Swedish storage", Sweden, storage: true),
            NewLocality(SwedishVenue, "Swedish venue", Sweden, venue: true),
            NewLocality(NorwegianStorage, "Norwegian storage", Norway, storage: true),
            NewLocality(ClubStorage, "Club storage", Sweden, storage: true, groupId: Club),
            NewLocality(NorwegianClubStorage, "Klubblager", Norway, storage: true));
        await _factory.SeedAsync(NewModule(PersonsModule), NewModule(ClubsModule), NewModule(NorwegianClubsModule));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = PersonsModule, PersonId = Owner, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = ClubsModule, GroupId = Club, OwnedShare = 1 },
            new ModuleOwnership { Id = 3, ModuleId = NorwegianClubsModule, GroupId = NorwegianClub, OwnedShare = 1 });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task OnlyStorageInThePersonOwnersCountry()
    {
        CollectionAssert.AreEquivalent(new[] { SwedishStorage }, await StorageIdsAsync(Owner, PersonsModule, ModuleOwnershipRef.Person(Owner)));
    }

    [TestMethod]
    public async Task OnlyStorageInTheGroupOwnersCountry()
    {
        CollectionAssert.AreEquivalent(new[] { NorwegianStorage, NorwegianClubStorage }, await StorageIdsAsync(Owner, NorwegianClubsModule, ModuleOwnershipRef.Group(NorwegianClub)));
    }

    [TestMethod]
    public async Task AGroupsStorageOnlyForItsMembers()
    {
        CollectionAssert.AreEquivalent(new[] { SwedishStorage, ClubStorage }, await StorageIdsAsync(ClubMember, ClubsModule, ModuleOwnershipRef.Group(Club)));
        CollectionAssert.AreEquivalent(new[] { SwedishStorage }, await StorageIdsAsync(Owner, PersonsModule, ModuleOwnershipRef.Person(Owner)));
    }

    [TestMethod]
    public async Task ANewModuleGetsTheStorageOfTheOwnerItIsAddedFor()
    {
        CollectionAssert.AreEquivalent(new[] { NorwegianStorage, NorwegianClubStorage }, await StorageIdsAsync(Owner, 0, ModuleOwnershipRef.Group(NorwegianClub)));
    }

    [TestMethod]
    public async Task TheCurrentStorageIsKeptInTheList()
    {
        await SetStorageAsync(PersonsModule, SwedishVenue);

        CollectionAssert.AreEquivalent(new[] { SwedishStorage, SwedishVenue }, await StorageIdsAsync(Owner, PersonsModule, ModuleOwnershipRef.Person(Owner)));
    }

    [TestMethod]
    [DataRow(SwedishStorage, true)]
    [DataRow(SwedishVenue, false)]
    [DataRow(NorwegianStorage, false)]
    [DataRow(ClubStorage, false)]
    public async Task OnlyAPermittedStorageIsSaved(int localityId, bool isSaved)
    {
        var (count, _) = await SaveWithStorageAsync(Owner, PersonsModule, ModuleOwnershipRef.Person(Owner), localityId);

        Assert.AreEqual(isSaved, count > 0);
        Assert.AreEqual(isSaved ? localityId : null, (await _factory.FindAsync<Module>(PersonsModule))!.StoredAtLocalityId);
    }

    [TestMethod]
    public async Task AMemberStoresTheGroupsModuleInTheGroupsStorage()
    {
        var (count, _) = await SaveWithStorageAsync(ClubMember, ClubsModule, ModuleOwnershipRef.Group(Club), ClubStorage);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public async Task AnUnchangedStorageIsKeptAlsoWhenTheUserDoesntSeeIt()
    {
        await SetStorageAsync(PersonsModule, ClubStorage);

        var (count, _) = await SaveWithStorageAsync(Owner, PersonsModule, ModuleOwnershipRef.Person(Owner), ClubStorage, "Renamed");

        Assert.AreEqual(1, count);
        Assert.AreEqual(ClubStorage, (await _factory.FindAsync<Module>(PersonsModule))!.StoredAtLocalityId);
    }

    private async Task<int[]> StorageIdsAsync(int personId, int moduleId, ModuleOwnershipRef ownerRef) =>
        [.. (await new LocalityService(_factory).StorageListboxItemsAsync(Principal(personId), moduleId, ownerRef)).Select(i => i.Id)];

    private async Task<(int Count, string Message)> SaveWithStorageAsync(int personId, int moduleId, ModuleOwnershipRef ownerRef, int localityId, string? name = null)
    {
        var module = (await _factory.FindAsync<Module>(moduleId))!;
        module.StoredAtLocalityId = localityId;
        if (name is not null) module.FullName = name;
        var (count, message, _) = await new ModuleService(_factory, new SystemTimeProvider()).SaveAsync(Principal(personId), module, ownerRef);
        return (count, message);
    }

    private async Task SetStorageAsync(int moduleId, int localityId)
    {
        await using var dbContext = _factory.CreateDbContext();
        (await dbContext.Modules.FindAsync(moduleId))!.StoredAtLocalityId = localityId;
        await dbContext.SaveChangesAsync();
    }

    private static Module NewModule(int id) => new() { Id = id, FullName = $"Module {id}", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 };

    private static Locality NewLocality(int id, string name, int countryId, bool storage = false, bool venue = false, int? groupId = null) => new()
    {
        Id = id, Name = name, CountryId = countryId, CityName = "City", IsModuleStorage = storage, IsMeetingVenue = venue, UsedByGroupId = groupId,
    };
}