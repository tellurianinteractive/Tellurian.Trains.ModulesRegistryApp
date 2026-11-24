using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Localities: a venue is seen by everyone, a storage place used by a group only by the group's members and the administrators.
/// Administrators in the locality's country and the data administrators of the group using it edit it.
/// </summary>
[TestClass]
public sealed class LocalityServiceTests
{
    private const int CountryId = 1;
    private const int OtherCountryId = 2;
    private const int UserPersonId = 999;
    private const int ClubId = 1;
    private const int OtherClubId = 2;
    private const int PublicStorageId = 1;
    private const int ClubStorageId = 2;
    private const int ClubVenueId = 3;
    private const int OtherCountryStorageId = 4;
    private SqliteDbContextFactory _factory = null!;

    private static ClaimsPrincipal Principal(int countryId, params Claim[] role) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.CountryId, countryId.ToString()),
        new Claim(AppClaimTypes.PersonId, UserPersonId.ToString()),
        .. role,
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator => Principal(CountryId, new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal CountryAdministrator => Principal(CountryId, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal OtherCountryAdministrator => Principal(OtherCountryId, new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal User => Principal(CountryId);

    private LocalityService Target => new(_factory);

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = OtherCountryId, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(NewGroup(ClubId, CountryId), NewGroup(OtherClubId, CountryId), NewGroup(3, OtherCountryId));
        await _factory.SeedAsync(new Person { Id = UserPersonId, FirstName = "First", LastName = "Last", CityName = "City", CountryId = CountryId });
        await _factory.SeedAsync(
            NewLocality(PublicStorageId, "Public storage", storage: true),
            NewLocality(ClubStorageId, "Club storage", storage: true, groupId: ClubId),
            NewLocality(ClubVenueId, "Club venue", venue: true, groupId: ClubId),
            NewLocality(OtherCountryStorageId, "Norwegian storage", storage: true, groupId: 3, countryId: OtherCountryId));
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    [TestMethod]
    public async Task NonMemberSeesVenuesAndLocalitiesWithoutGroup()
    {
        CollectionAssert.AreEquivalent(new[] { PublicStorageId, ClubVenueId }, await VisibleIdsAsync(User));
        Assert.IsNull(await Target.FindByIdAsync(User, ClubStorageId));
    }

    [TestMethod]
    public async Task MemberSeesTheGroupsStorage()
    {
        await MakeMemberAsync(ClubId);

        CollectionAssert.AreEquivalent(new[] { PublicStorageId, ClubStorageId, ClubVenueId }, await VisibleIdsAsync(User));
        Assert.IsNotNull(await Target.FindByIdAsync(User, ClubStorageId));
    }

    [TestMethod]
    public async Task AdministratorsSeeAllInTheirCountry()
    {
        CollectionAssert.AreEquivalent(new[] { PublicStorageId, ClubStorageId, ClubVenueId }, await VisibleIdsAsync(CountryAdministrator));
        CollectionAssert.AreEquivalent(new[] { PublicStorageId, ClubStorageId, ClubVenueId }, await VisibleIdsAsync(GlobalAdministrator));
        CollectionAssert.AreEquivalent(new[] { PublicStorageId, ClubVenueId }, await VisibleIdsAsync(OtherCountryAdministrator));
    }

    [TestMethod]
    public async Task OnlyAdministratorsAndTheGroupsDataAdministratorsEdit()
    {
        await MakeMemberAsync(ClubId);
        var all = (await Target.GetAllInCountryAsync(User, CountryId)).ToDictionary(l => l.Locality.Id, l => l.MayEdit);
        Assert.IsFalse(all[ClubStorageId]);

        await MakeDataAdministratorAsync(ClubId);
        all = (await Target.GetAllInCountryAsync(User, CountryId)).ToDictionary(l => l.Locality.Id, l => l.MayEdit);

        Assert.IsTrue(all[ClubStorageId]);
        Assert.IsTrue(all[ClubVenueId]);
        Assert.IsFalse(all[PublicStorageId]);
    }

    [TestMethod]
    public async Task CountryAdministratorsEditOnlyInTheirCountry()
    {
        Assert.IsTrue((await Target.GetAllInCountryAsync(CountryAdministrator, CountryId)).All(l => l.MayEdit));
        Assert.IsTrue((await Target.GetAllInCountryAsync(OtherCountryAdministrator, CountryId)).All(l => !l.MayEdit));
    }

    [TestMethod]
    public async Task DataAdministratorAddsForTheirGroupInTheGroupsCountry()
    {
        await MakeDataAdministratorAsync(ClubId);

        var (count, _, entity) = await Target.SaveAsync(User, new Locality { CountryId = OtherCountryId, Name = "garage", CityName = "värnamo", UsedByGroupId = ClubId });

        Assert.AreEqual(1, count);
        Assert.AreEqual(CountryId, entity!.CountryId);
        Assert.AreEqual(ClubId, entity.UsedByGroupId);
    }

    [TestMethod]
    public async Task DataAdministratorMayNotAddForOtherGroupsOrWithoutGroup()
    {
        await MakeDataAdministratorAsync(ClubId);

        Assert.AreEqual(0, (await Target.SaveAsync(User, new Locality { CountryId = CountryId, Name = "Garage", CityName = "City", UsedByGroupId = OtherClubId })).Count);
        Assert.AreEqual(0, (await Target.SaveAsync(User, new Locality { CountryId = CountryId, Name = "Garage", CityName = "City" })).Count);
        Assert.IsFalse(await Target.MayAddAsync(Principal(CountryId), OtherCountryId));
    }

    [TestMethod]
    public async Task UserWithoutRightsMayNotAdd()
    {
        Assert.IsFalse(await Target.MayAddAsync(User, CountryId));
        Assert.AreEqual(0, (await Target.SaveAsync(User, new Locality { CountryId = CountryId, Name = "Garage", CityName = "City" })).Count);
    }

    [TestMethod]
    public async Task DataAdministratorKeepsTheGroupAndCountry()
    {
        await MakeDataAdministratorAsync(ClubId);
        var changed = NewLocality(ClubStorageId, "Club store", storage: true, groupId: null, countryId: OtherCountryId);

        var (count, _, _) = await Target.SaveAsync(User, changed);

        var saved = await _factory.FindAsync<Locality>(ClubStorageId);
        Assert.AreEqual(1, count);
        Assert.AreEqual("Club store", saved!.Name);
        Assert.AreEqual(ClubId, saved.UsedByGroupId);
        Assert.AreEqual(CountryId, saved.CountryId);
    }

    [TestMethod]
    public async Task CountryAdministratorMayNotEditInOtherCountry()
    {
        var (count, _, _) = await Target.SaveAsync(OtherCountryAdministrator, NewLocality(PublicStorageId, "Changed", storage: true));

        Assert.AreEqual(0, count);
        Assert.AreEqual("Public storage", (await _factory.FindAsync<Locality>(PublicStorageId))!.Name);
    }

    [TestMethod]
    public async Task TheGroupMustBeInTheLocalitysCountry()
    {
        var (count, _, _) = await Target.SaveAsync(GlobalAdministrator, new Locality { CountryId = CountryId, Name = "Garage", CityName = "City", UsedByGroupId = 3 });

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task SavedTextsAndKindAreCorrected()
    {
        var (_, _, entity) = await Target.SaveAsync(CountryAdministrator, new Locality { CountryId = CountryId, Name = "garage", CityName = "värnamo", PostalCode = "abc 12", Latitude = 57m });

        Assert.AreEqual("Garage", entity!.Name);
        Assert.AreEqual("Värnamo", entity.CityName);
        Assert.AreEqual("ABC 12", entity.PostalCode);
        Assert.IsTrue(entity.IsModuleStorage);
        Assert.IsNull(entity.Latitude);
    }

    [TestMethod]
    public async Task LocalityWhereModulesAreStoredIsNotDeletable()
    {
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo" });
        await _factory.SeedAsync(new Module { Id = 1, FullName = "Module", ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1, StoredAtLocalityId = PublicStorageId });

        var deletable = await Target.DeletableIdsAsync(CountryAdministrator, [PublicStorageId, ClubStorageId]);
        var (count, _) = await Target.DeleteAsync(CountryAdministrator, PublicStorageId);

        CollectionAssert.AreEquivalent(new[] { ClubStorageId }, deletable.ToArray());
        Assert.AreEqual(0, count);
        Assert.IsNotNull(await _factory.FindAsync<Locality>(PublicStorageId));
    }

    [TestMethod]
    public async Task DeletingAVenueDeletesItsHallDrawing()
    {
        await _factory.SeedAsync(new Document { Id = 1, FileExtension = "json", ContentType = "application/json", Content = [1] });
        await using (var dbContext = _factory.CreateDbContext())
        {
            var venue = await dbContext.Localities.FindAsync(ClubVenueId);
            venue!.HallDrawingId = 1;
            await dbContext.SaveChangesAsync();
        }

        var (count, _) = await Target.DeleteAsync(CountryAdministrator, ClubVenueId);

        Assert.AreEqual(2, count);
        Assert.IsNull(await _factory.FindAsync<Locality>(ClubVenueId));
        Assert.IsNull(await _factory.FindAsync<Document>(1));
    }

    private async Task<int[]> VisibleIdsAsync(ClaimsPrincipal principal) =>
        [.. (await Target.GetAllInCountryAsync(principal, CountryId)).Select(l => l.Locality.Id)];

    private Task MakeMemberAsync(int groupId) =>
        _factory.SeedAsync(new GroupMember { GroupId = groupId, PersonId = UserPersonId });

    private Task MakeDataAdministratorAsync(int groupId) =>
        _factory.SeedAsync(new GroupMember { GroupId = groupId, PersonId = UserPersonId, IsDataAdministrator = true });

    private static Group NewGroup(int id, int countryId) => new()
    {
        Id = id, CountryId = countryId, FullName = $"Group {id}", ShortName = $"G{id}", Category = "Club", CityName = "City",
    };

    private static Locality NewLocality(int id, string name, bool storage = false, bool venue = false, int? groupId = null, int countryId = CountryId) => new()
    {
        Id = id, CountryId = countryId, Name = name, CityName = "City", IsModuleStorage = storage, IsMeetingVenue = venue, UsedByGroupId = groupId,
    };
}