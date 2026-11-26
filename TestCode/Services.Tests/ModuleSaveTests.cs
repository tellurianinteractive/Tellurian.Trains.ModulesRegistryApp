using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Who may save a module and its exits (Migration step 3.6): an existing module those who may edit it (MayEditModuleAsync, assistants
/// included), a new module those who may add modules for the owner (MayAddModuleForAsync). Before, the rights came from the owner in
/// the page's address whatever module was posted.
/// </summary>
[TestClass]
public sealed class ModuleSaveTests
{
    private const int CountryId = 1, OtherCountryId = 2;
    private const int Owner = 1, Stranger = 2, Assistant = 3, DataAdministrator = 4;
    private const int OwnersModule = 1, StrangersModule = 2, StrangersStationModule = 3;
    private const int Group = 1, Exit = 1, EndProfile = 1;
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

    private ModuleService Target => new(_factory, new SystemTimeProvider());

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(
            new Country { Id = CountryId, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = OtherCountryId, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new Region { Id = 1, CountryId = CountryId, LocalName = "Region", ForeColor = "#000000", BackColor = "#ffffff" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "Fremo H0", ScaleId = 1, TrackSystem = "Track", PreferredTheme = "Europe" });
        await _factory.SeedAsync(new ModuleEndProfile { Id = EndProfile, ScaleId = 1, Designation = "Fremo H0-B" });
        await _factory.SeedAsync(
            new Person { Id = Owner, FirstName = "Anna", LastName = "Berg", CityName = "City", CountryId = CountryId },
            new Person { Id = Stranger, FirstName = "Erik", LastName = "Ek", CityName = "City", CountryId = CountryId },
            new Person { Id = Assistant, FirstName = "Lisa", LastName = "Lund", CityName = "City", CountryId = CountryId },
            new Person { Id = DataAdministrator, FirstName = "Olof", LastName = "Ås", CityName = "City", CountryId = CountryId });
        await _factory.SeedAsync(new Group { Id = Group, CountryId = CountryId, FullName = "Group", ShortName = "G", Category = "Club", CityName = "City" });
        await _factory.SeedAsync(
            new GroupMember { Id = 1, GroupId = Group, PersonId = Owner },
            new GroupMember { Id = 2, GroupId = Group, PersonId = DataAdministrator, IsDataAdministrator = true });
        await _factory.SeedAsync(NewModule(OwnersModule, "Owner's module"), NewModule(StrangersModule, "Stranger's module"), NewModule(StrangersStationModule, "Stranger's station"));
        await _factory.SeedAsync(
            new ModuleOwnership { Id = 1, ModuleId = OwnersModule, PersonId = Owner, OwnedShare = 1 },
            new ModuleOwnership { Id = 2, ModuleId = OwnersModule, PersonId = Assistant, OwnedShare = 0 },
            new ModuleOwnership { Id = 3, ModuleId = StrangersModule, PersonId = Stranger, OwnedShare = 1 },
            new ModuleOwnership { Id = 4, ModuleId = StrangersStationModule, PersonId = Stranger, OwnedShare = 1 });
        await _factory.SeedAsync(new Station { Id = 1, FullName = "Station", Signature = "S", RegionId = 1, PrimaryModuleId = StrangersStationModule });
        await _factory.SeedAsync(new ModuleExit { Id = Exit, ModuleId = OwnersModule, EndProfileId = EndProfile, Label = "East", Direction = 1 });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static Module NewModule(int id, string name) => new() { Id = id, FullName = name, ScaleId = 1, StandardId = 1, Length = 100, NumberOfThroughTracks = 1 };
    private static Module Changed(int id, string name = "Changed name") => new() { Id = id, FullName = name, StandardId = 1, Straight = 500, NumberOfThroughTracks = 1, PdfDocumentationId = 99 };

    [TestMethod]
    public async Task AnotherUserMayNotChangeSomeonesModuleWhateverOwnerTheAddressNames()
    {
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(Stranger), Changed(OwnersModule), ModuleOwnershipRef.Person(Stranger))).Count);
        Assert.AreEqual("Owner's module", (await _factory.FindAsync<Module>(OwnersModule))!.FullName);
    }

    [TestMethod]
    [DataRow(Owner, DisplayName = "Owner")]
    [DataRow(Assistant, DisplayName = "Assistant")]
    [DataRow(DataAdministrator, DisplayName = "Data administrator of the owner's group")]
    public async Task ThoseWhoMayEditChangeTheModuleButNotItsDocuments(int personId)
    {
        var result = await Target.SaveAsync(Principal(personId), Changed(OwnersModule), ModuleOwnershipRef.Person(personId));

        Assert.AreEqual(1, result.Count, result.Message);
        var stored = (await _factory.FindAsync<Module>(OwnersModule))!;
        Assert.AreEqual("Changed name", stored.FullName);
        Assert.AreEqual(500, stored.Length, "Calculated from the straight.");
        Assert.IsNull(stored.PdfDocumentationId, "Documents are not changed by a save.");
    }

    [TestMethod]
    public async Task WithoutAPrimaryThemeTheModuleGetsItsStandards()
    {
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "USMS", ScaleId = 1, MainThemeId = (int)Theme.US });
        var american = Changed(0, "American");
        american.StandardId = 2;
        american.PrimaryThemeId = 0;
        var european = Changed(0, "European");
        european.PrimaryThemeId = 0;

        var addedAmerican = await Target.SaveAsync(Principal(Owner), american, ModuleOwnershipRef.Person(Owner));
        var addedEuropean = await Target.SaveAsync(Principal(Owner), european, ModuleOwnershipRef.Person(Owner));

        Assert.AreEqual((int)Theme.US, (await _factory.FindAsync<Module>(addedAmerican.Entity!.Id))!.PrimaryThemeId);
        Assert.AreEqual((int)Theme.Europe, (await _factory.FindAsync<Module>(addedEuropean.Entity!.Id))!.PrimaryThemeId, "The standard without a theme counts as Europe.");
    }

    [TestMethod]
    public async Task TheOwnerChoosesTheThemes()
    {
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "USMS", ScaleId = 1, MainThemeId = (int)Theme.US });
        var changed = Changed(OwnersModule);
        changed.StandardId = 2;
        changed.PrimaryThemeId = (int)Theme.Europe; // Another theme than the standard's.
        changed.SecondaryThemeId = (int)Theme.US;
        var same = Changed(StrangersModule);
        same.PrimaryThemeId = (int)Theme.US;
        same.SecondaryThemeId = (int)Theme.US;

        await Target.SaveAsync(Principal(Owner), changed, ModuleOwnershipRef.Person(Owner));
        await Target.SaveAsync(Principal(Stranger), same, ModuleOwnershipRef.Person(Stranger));

        var stored = (await _factory.FindAsync<Module>(OwnersModule))!;
        Assert.AreEqual((int)Theme.Europe, stored.PrimaryThemeId);
        Assert.AreEqual((int)Theme.US, stored.SecondaryThemeId);
        Assert.IsNull((await _factory.FindAsync<Module>(StrangersModule))!.SecondaryThemeId, "The secondary theme is another theme than the primary.");
    }

    [TestMethod]
    [DataRow(1, null, null, DisplayName = "Same as the primary")]
    [DataRow(1, 0, null, DisplayName = "Zero")]
    [DataRow(1, 2, 2, DisplayName = "Another theme")]
    [DataRow(1, 7, null, DisplayName = "Not a theme")]
    public void TheSecondaryThemeIsAnotherThemeOrNone(int primary, int? secondary, int? expected)
    {
        var module = new Module { FullName = "Module", PrimaryThemeId = primary, SecondaryThemeId = secondary ?? primary };
        module.CorrectInput();
        Assert.AreEqual(expected, module.SecondaryThemeId);
    }

    [TestMethod]
    public async Task ASaveKeepsTheLastMeetingRegistrationDate()
    {
        var lastMeeting = new DateTimeOffset(2025, 9, 12, 0, 0, 0, TimeSpan.Zero);
        await using (var dbContext = _factory.CreateDbContext())
        {
            (await dbContext.Modules.FindAsync(OwnersModule))!.LastMeetingRegistrationDate = lastMeeting;
            await dbContext.SaveChangesAsync();
        }

        var changed = await Target.SaveAsync(Principal(Owner), Changed(OwnersModule), ModuleOwnershipRef.Person(Owner));
        var newModule = Changed(0, "New");
        newModule.LastMeetingRegistrationDate = lastMeeting;
        var added = await Target.SaveAsync(Principal(Owner), newModule, ModuleOwnershipRef.Person(Owner));

        Assert.AreEqual(1, changed.Count, changed.Message);
        Assert.AreEqual(lastMeeting, (await _factory.FindAsync<Module>(OwnersModule))!.LastMeetingRegistrationDate, "The form has no date.");
        Assert.IsNull((await _factory.FindAsync<Module>(added.Entity!.Id))!.LastMeetingRegistrationDate, "Only meeting registrations set it.");
    }

    [TestMethod]
    public async Task ACountryAdministratorChangesModulesOnlyInTheirCountry()
    {
        Assert.AreEqual(0, (await Target.SaveAsync(CountryAdministrator(OtherCountryId), Changed(OwnersModule), ModuleOwnershipRef.Person(Owner))).Count);
        Assert.AreEqual(1, (await Target.SaveAsync(CountryAdministrator(CountryId), Changed(OwnersModule), ModuleOwnershipRef.Person(Owner))).Count);
    }

    [TestMethod]
    public async Task UsersAddModulesOnlyForThoseTheyMay()
    {
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(Stranger), Changed(0, "New"), ModuleOwnershipRef.Person(Owner))).Count);
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(Owner), Changed(0, "New"), ModuleOwnershipRef.Group(Group))).Count, "A member, not data administrator.");
        var result = await Target.SaveAsync(Principal(DataAdministrator), Changed(0, "New"), ModuleOwnershipRef.Group(Group));
        Assert.AreEqual(1, result.Count > 0 ? 1 : 0, result.Message);
        Assert.IsNotNull(result.Entity);
        Assert.IsGreaterThan(0, (await Target.SaveAsync(Principal(Owner), Changed(0, "Mine"), ModuleOwnershipRef.Person(Owner))).Count);
    }

    [TestMethod]
    public async Task AModuleIsNotPutInSomeonesStation()
    {
        var module = Changed(OwnersModule);
        module.StationId = 1;
        Assert.AreEqual(0, (await Target.SaveAsync(Principal(Owner), module, ModuleOwnershipRef.Person(Owner))).Count);
        Assert.IsNull((await _factory.FindAsync<Module>(OwnersModule))!.StationId);
    }

    [TestMethod]
    public async Task AnotherUserMayNotChangeOrDeleteSomeonesExit()
    {
        Assert.AreEqual(0, (await Target.SaveExitAsync(Principal(Stranger), new ModuleExit { Id = Exit, ModuleId = StrangersModule, EndProfileId = EndProfile, Label = "X" })).Count);
        Assert.IsLessThanOrEqualTo(0, (await Target.DeleteExitAsync(Principal(Stranger), Exit)).Count);
        var stored = (await _factory.FindAsync<ModuleExit>(Exit))!;
        Assert.AreEqual("East", stored.Label);
        Assert.AreEqual(OwnersModule, stored.ModuleId);
    }

    [TestMethod]
    public async Task AnAssistantChangesAddsAndDeletesExits()
    {
        Assert.AreEqual(1, (await Target.SaveExitAsync(Principal(Assistant), new ModuleExit { Id = Exit, ModuleId = StrangersModule, EndProfileId = EndProfile, Label = "West", Direction = 2 })).Count);
        Assert.AreEqual(OwnersModule, (await _factory.FindAsync<ModuleExit>(Exit))!.ModuleId, "An exit never moves to another module.");

        var added = await Target.SaveExitAsync(Principal(Assistant), new ModuleExit { ModuleId = OwnersModule, EndProfileId = EndProfile, Label = "North", Direction = 99 });
        Assert.AreEqual(1, added.Count);
        Assert.AreEqual((int)ModuleExitDirection.Undefined, added.Entity!.Direction, "An unknown direction is corrected.");
        Assert.AreEqual(1, (await Target.DeleteExitAsync(Principal(Assistant), added.Entity.Id)).Count);
    }

    [TestMethod]
    public async Task AnExitNeedsAnExistingEndProfile() =>
        Assert.AreEqual(0, (await Target.SaveExitAsync(Principal(Owner), new ModuleExit { ModuleId = OwnersModule, EndProfileId = 99, Label = "South" })).Count);

    [TestMethod]
    public async Task OnlyThoseWhoMayEditFindTheModuleForEditing()
    {
        Assert.IsNotNull(await Target.FindForEditAsync(Principal(Assistant), OwnersModule));
        Assert.IsNull(await Target.FindForEditAsync(Principal(Stranger), OwnersModule));
    }

    [TestMethod]
    public async Task OnlyTheSoleOwnerOrAnAdministratorInTheCountryDeletesAModule()
    {
        Assert.IsLessThanOrEqualTo(0, (await Target.DeleteAsync(Principal(Assistant), OwnersModule)).Count, "An assistant");
        Assert.IsLessThanOrEqualTo(0, (await Target.DeleteAsync(Principal(Stranger), OwnersModule)).Count, "Another user");
        Assert.IsLessThanOrEqualTo(0, (await Target.DeleteAsync(CountryAdministrator(OtherCountryId), OwnersModule)).Count, "A country administrator in another country");
        Assert.IsNotNull(await _factory.FindAsync<Module>(OwnersModule));

        Assert.IsGreaterThan(0, (await Target.DeleteAsync(Principal(Owner), OwnersModule)).Count);
        Assert.IsNull(await _factory.FindAsync<Module>(OwnersModule));
        Assert.IsNull(await _factory.FindAsync<ModuleExit>(Exit));
    }

    [TestMethod]
    public async Task AModuleOfAStationIsNotDeleted()
    {
        await using (var dbContext = _factory.CreateDbContext())
        {
            var module = await dbContext.Modules.FindAsync(StrangersStationModule);
            module!.StationId = 1;
            await dbContext.SaveChangesAsync();
        }
        Assert.IsLessThanOrEqualTo(0, (await Target.DeleteAsync(Principal(Stranger), StrangersStationModule)).Count);
    }
}