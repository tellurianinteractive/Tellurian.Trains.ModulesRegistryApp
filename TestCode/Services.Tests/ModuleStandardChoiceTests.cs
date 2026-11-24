using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The standards to choose from: the international standards and the national standards of the countries of the module's owners,
/// or of the meeting's country for a layout. The standard already chosen is always there.
/// </summary>
[TestClass]
public sealed class ModuleStandardChoiceTests
{
    private SqliteDbContextFactory _factory = null!;
    private const int Sweden = 1, Norway = 2, Denmark = 3;
    private const int International = 1, Swedish = 2, Norwegian = 3, Danish = 4;
    private const int SwedishOwner = 1, NorwegianOwner = 2, DanishAssistant = 3;
    private const int NorwegianGroup = 1;

    private static ClaimsPrincipal User => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, "1"),
        new Claim(AppClaimTypes.PersonId, "1"),
        new Claim(AppClaimTypes.CountryId, "1"),
    ], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" },
            new Country { Id = Denmark, EnglishName = "Denmark", DomainSuffix = "dk", Languages = "da" });
        await _factory.SeedAsync(
            Standard(International, "FREMO", null),
            Standard(Swedish, "SMS", Sweden),
            Standard(Norwegian, "NMS", Norway),
            Standard(Danish, "DMS", Denmark));
        await _factory.SeedAsync(
            new Person { Id = SwedishOwner, FirstName = "Anna", LastName = "Berg", CityName = "City", CountryId = Sweden },
            new Person { Id = NorwegianOwner, FirstName = "Ola", LastName = "Nordmann", CityName = "City", CountryId = Norway },
            new Person { Id = DanishAssistant, FirstName = "Mette", LastName = "Hansen", CityName = "City", CountryId = Denmark });
        await _factory.SeedAsync(new Group { Id = NorwegianGroup, CountryId = Norway, FullName = "Group", ShortName = "G", Category = "Club", CityName = "City" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static ModuleStandard Standard(int id, string name, int? countryId) =>
        new() { Id = id, ShortName = name, ScaleId = 1, MainThemeId = 1, OnlyValidInCountryId = countryId };

    private static int[] Ids(IEnumerable<ListboxItem> items) => [.. items.Select(i => i.Id).Order()];

    [TestMethod]
    public async Task AModuleGetsTheNationalStandardsOfItsOwnersButNotOfItsAssistants()
    {
        await _factory.SeedAsync(new Module { Id = 1, FullName = "Module", ScaleId = 1, StandardId = International, Length = 100, NumberOfThroughTracks = 1 });
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = SwedishOwner, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 2, ModuleId = 1, GroupId = NorwegianGroup, OwnedShare = 0.5 },
            new ModuleOwnership { Id = 3, ModuleId = 1, PersonId = DanishAssistant, OwnedShare = 0 });

        var items = await new ModuleStandardService(_factory).ModuleListboxItemsAsync(User, 1, ModuleOwnershipRef.Person(DanishAssistant));

        CollectionAssert.AreEqual(new[] { International, Swedish, Norwegian }, Ids(items));
    }

    [TestMethod]
    public async Task AModuleKeepsItsStandardFromAnotherCountry()
    {
        await _factory.SeedAsync(new Module { Id = 1, FullName = "Module", ScaleId = 1, StandardId = Danish, Length = 100, NumberOfThroughTracks = 1 });
        await _factory.SeedAsync(new ModuleOwnership { Id = 1, ModuleId = 1, PersonId = SwedishOwner, OwnedShare = 1 });

        var items = await new ModuleStandardService(_factory).ModuleListboxItemsAsync(User, 1, ModuleOwnershipRef.Person(SwedishOwner));

        CollectionAssert.AreEqual(new[] { International, Swedish, Danish }, Ids(items));
    }

    [TestMethod]
    public async Task ANewModuleGetsTheNationalStandardsOfTheOwnerInTheAddress()
    {
        var target = new ModuleStandardService(_factory);

        CollectionAssert.AreEqual(new[] { International, Swedish }, Ids(await target.ModuleListboxItemsAsync(User, 0, ModuleOwnershipRef.Person(SwedishOwner))));
        CollectionAssert.AreEqual(new[] { International, Norwegian }, Ids(await target.ModuleListboxItemsAsync(User, 0, ModuleOwnershipRef.Group(NorwegianGroup))));
    }

    [TestMethod]
    public async Task ALayoutGetsTheNationalStandardsOfTheMeetingsCountry()
    {
        var target = new ModuleStandardService(_factory);

        CollectionAssert.AreEqual(new[] { International, Danish }, Ids(await target.LayoutListboxItemsAsync(User, Denmark, null)));
        CollectionAssert.AreEqual(new[] { International, Swedish, Danish }, Ids(await target.LayoutListboxItemsAsync(User, Denmark, Swedish)), "Its own standard stays.");
    }
}