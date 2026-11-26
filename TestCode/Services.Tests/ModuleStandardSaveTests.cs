using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Module standards and end profiles are shared by all countries, so only global administrators save them (Migration step 3.6;
/// before, any user who wasn't read-only could save a module standard). Saved values are corrected.
/// </summary>
[TestClass]
public sealed class ModuleStandardSaveTests
{
    private SqliteDbContextFactory _factory = null!;
    private const int Sweden = 1;
    private const int Norway = 2;
    private const int NEM310 = 2, RP25 = 3, Steam = 201, FL6511 = 101, Kadee = 109, MicroTrains = 110;

    private static ClaimsPrincipal Principal(params Claim[] extra) => new(new ClaimsIdentity(
    [
        new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()),
        new Claim(AppClaimTypes.UserId, "1"),
        new Claim(AppClaimTypes.PersonId, "1"),
        new Claim(AppClaimTypes.CountryId, "1"),
        .. extra,
    ], "Test"));

    private static ClaimsPrincipal GlobalAdministrator => Principal(new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal CountryAdministrator => Principal(new Claim(AppClaimTypes.CountryAdministrator, "true"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Scale { Id = 3, ShortName = "H0", Denominator = 87 });
        await _factory.SeedAsync(
            new Country { Id = Sweden, EnglishName = "Sweden", DomainSuffix = "se", Languages = "sv" },
            new Country { Id = Norway, EnglishName = "Norway", DomainSuffix = "no", Languages = "nb" });
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "FREMO/H0-RE", ScaleId = 3, TrackSystem = "Track", PreferredTheme = "Europe", MainThemeId = 1 });
        await _factory.SeedAsync(new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "FREMO 1" });
        await _factory.SeedAsync(new Scale { Id = 1, ShortName = "N", Denominator = 160 });
        await _factory.SeedAsync(
            new Feature { Id = NEM310, Category = "WHEELS", Description = "NEM310" },
            new Feature { Id = RP25, Category = "WHEELS", Description = "RP25" },
            new Feature { Id = Steam, Category = "TRACTION", Description = "Steam", IsResourceCode = true },
            new Feature { Id = FL6511, Category = "COUPLINGS", OnlyScaleId = 3, Description = "FL6511" },
            new Feature { Id = Kadee, Category = "COUPLINGS", Description = "Kadee" },
            new Feature { Id = MicroTrains, Category = "COUPLINGS", OnlyScaleId = 1, Description = "Micro-Trains" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static ModuleStandard Changed() => new()
    {
        Id = 1, ShortName = " FREMO/H0-RE ", ScaleId = 3, TrackSystem = "",
        PreferredTheme = "Europe", MainThemeId = 1,
    };

    [TestMethod]
    public async Task AGlobalAdministratorSavesACorrectedStandard()
    {
        var (count, _, _) = await new ModuleStandardService(_factory).SaveAsync(GlobalAdministrator, Changed());

        Assert.AreEqual(1, count);
        var saved = (await _factory.FindAsync<ModuleStandard>(1))!;
        Assert.AreEqual("FREMO/H0-RE", saved.ShortName);
        Assert.IsNull(saved.TrackSystem, "An empty optional text is saved as empty.");
    }

    private static ModuleStandard WithFeatures(ModuleStandard standard, params int[] featureIds)
    {
        standard.Features = [.. featureIds.Select(id => new ModuleFeature { ModuleStandardId = standard.Id, FeatureId = id })];
        return standard;
    }

    private async Task<int[]> SavedFeatureIdsAsync(int standardId)
    {
        await using var dbContext = _factory.CreateDbContext();
        return [.. dbContext.ModuleFeatures.Where(f => f.ModuleStandardId == standardId).Select(f => f.FeatureId).OrderBy(id => id)];
    }

    [TestMethod]
    public async Task AStandardAcceptsWheelsAndCouplingsOfItsScaleOrAnyScale()
    {
        await new ModuleStandardService(_factory).SaveAsync(GlobalAdministrator, WithFeatures(Changed(), NEM310, RP25, FL6511, Kadee, MicroTrains, Steam, 999));

        CollectionAssert.AreEqual(new[] { NEM310, RP25, FL6511, Kadee }, await SavedFeatureIdsAsync(1), "Two wheel profiles and two couplings; not another scale's, traction or unknown.");
    }

    [TestMethod]
    public async Task ChangedFeaturesAreRemovedAndAdded()
    {
        var target = new ModuleStandardService(_factory);
        await target.SaveAsync(GlobalAdministrator, WithFeatures(Changed(), RP25, FL6511));

        var (count, _, _) = await target.SaveAsync(GlobalAdministrator, WithFeatures(Changed(), NEM310, FL6511));
        var (unchangedCount, _, _) = await target.SaveAsync(GlobalAdministrator, WithFeatures(Changed(), NEM310, FL6511));

        Assert.AreEqual(2, count, "One removed and one added.");
        Assert.AreEqual(-1, unchangedCount);
        CollectionAssert.AreEqual(new[] { NEM310, FL6511 }, await SavedFeatureIdsAsync(1));
    }

    [TestMethod]
    public async Task ANewStandardGetsItsFeatures()
    {
        var (count, _, saved) = await new ModuleStandardService(_factory).SaveAsync(GlobalAdministrator, WithFeatures(new ModuleStandard { ShortName = "FREMO/QS", ScaleId = 3, MainThemeId = 1 }, RP25));

        Assert.IsGreaterThan(0, count);
        CollectionAssert.AreEqual(new[] { RP25 }, await SavedFeatureIdsAsync(saved!.Id));
    }

    [TestMethod]
    public async Task DeletingAStandardDeletesItsFeatures()
    {
        var target = new ModuleStandardService(_factory);
        await target.SaveAsync(GlobalAdministrator, WithFeatures(Changed(), RP25));

        var (count, _) = await target.DeleteAsync(GlobalAdministrator, 1);

        Assert.IsGreaterThan(0, count);
        Assert.IsEmpty(await SavedFeatureIdsAsync(1));
    }

    [TestMethod]
    public async Task OthersMayNotSaveAStandard()
    {
        var target = new ModuleStandardService(_factory);

        Assert.AreEqual(0, (await target.SaveAsync(CountryAdministrator, Changed())).Count);
        Assert.AreEqual(0, (await target.SaveAsync(Principal(), Changed())).Count);
        Assert.AreEqual(0, (await target.SaveAsync(Principal(), new ModuleStandard { ShortName = "New", ScaleId = 3, MainThemeId = 1 })).Count);
        Assert.AreEqual("Track", (await _factory.FindAsync<ModuleStandard>(1))!.TrackSystem);
    }

    [TestMethod]
    [DataRow(0, 0, DisplayName = "Any")]
    [DataRow(2, 2, DisplayName = "North America")]
    [DataRow(7, 0, DisplayName = "Not a theme")]
    public async Task AStandardIsForAnyThemeOrOne(int themeId, int expected)
    {
        var standard = Changed();
        standard.MainThemeId = themeId;

        await new ModuleStandardService(_factory).SaveAsync(GlobalAdministrator, standard);

        Assert.AreEqual(expected, (await _factory.FindAsync<ModuleStandard>(1))!.MainThemeId);
    }

    [TestMethod]
    public async Task AGlobalAdministratorAddsAStandard()
    {
        var (count, _, saved) = await new ModuleStandardService(_factory).SaveAsync(GlobalAdministrator, new ModuleStandard { ShortName = "USMS", ScaleId = 3, MainThemeId = 2 });

        Assert.AreEqual(1, count);
        Assert.AreEqual("USMS", (await _factory.FindAsync<ModuleStandard>(saved!.Id))!.ShortName);
    }

    [TestMethod]
    public async Task OnlyAGlobalAdministratorSavesAnEndProfile()
    {
        var target = new ModuleEndProfileService(_factory);

        Assert.AreEqual(0, (await target.SaveAsync(CountryAdministrator, new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "Changed" })).Count);
        Assert.AreEqual(1, (await target.SaveAsync(GlobalAdministrator, new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = " FREMO 2 " })).Count);
        Assert.AreEqual("FREMO 2", (await _factory.FindAsync<ModuleEndProfile>(1))!.Designation);
    }

    [TestMethod]
    public async Task AStandardExtendsAnother()
    {
        var target = new ModuleStandardService(_factory);
        var (_, _, extension) = await target.SaveAsync(GlobalAdministrator, new ModuleStandard { ShortName = "FREMO/H0-NE", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 1 });

        Assert.AreEqual(1, (await _factory.FindAsync<ModuleStandard>(extension!.Id))!.ExtendsStandardId);
    }

    [TestMethod]
    public async Task AStandardDoesNotExtendItselfOrItsExtensions()
    {
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "Extension", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 1 });
        await _factory.SeedAsync(new ModuleStandard { Id = 3, ShortName = "Extension2", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 2 });
        var target = new ModuleStandardService(_factory);

        var loop = Changed();
        loop.ExtendsStandardId = 3;
        Assert.AreEqual(0, (await target.SaveAsync(GlobalAdministrator, loop)).Count, "1 ← 2 ← 3 ← 1 is a loop.");
        Assert.IsNull((await _factory.FindAsync<ModuleStandard>(1))!.ExtendsStandardId);

        var itself = Changed();
        itself.ExtendsStandardId = 1;
        await target.SaveAsync(GlobalAdministrator, itself);
        Assert.IsNull((await _factory.FindAsync<ModuleStandard>(1))!.ExtendsStandardId, "Not an extension of itself.");

        var items = await target.ExtendableStandardsListboxItemsAsync(GlobalAdministrator, 1);
        Assert.IsEmpty(items, "Only itself and its extensions.");
        var forNew = await target.ExtendableStandardsListboxItemsAsync(GlobalAdministrator, 0);
        Assert.HasCount(3, forNew);
        var forExtension = await target.ExtendableStandardsListboxItemsAsync(GlobalAdministrator, 2);
        CollectionAssert.AreEqual(new[] { 1 }, forExtension.Select(i => i.Id).ToArray());
    }

    [TestMethod]
    public async Task AnExtensionOfANationalStandardIsNationalInTheSameCountry()
    {
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "National", ScaleId = 3, MainThemeId = 1, OnlyValidInCountryId = Sweden });
        var target = new ModuleStandardService(_factory);

        var (_, _, international) = await target.SaveAsync(GlobalAdministrator, new ModuleStandard { ShortName = "Intl", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 2 });
        var (_, _, otherCountry) = await target.SaveAsync(GlobalAdministrator, new ModuleStandard { ShortName = "Norwegian", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 2, OnlyValidInCountryId = Norway });
        var (_, _, ofInternational) = await target.SaveAsync(GlobalAdministrator, new ModuleStandard { ShortName = "Norwegian2", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 1, OnlyValidInCountryId = Norway });

        Assert.AreEqual(Sweden, (await _factory.FindAsync<ModuleStandard>(international!.Id))!.OnlyValidInCountryId);
        Assert.AreEqual(Sweden, (await _factory.FindAsync<ModuleStandard>(otherCountry!.Id))!.OnlyValidInCountryId);
        Assert.AreEqual(Norway, (await _factory.FindAsync<ModuleStandard>(ofInternational!.Id))!.OnlyValidInCountryId, "An extension of an international standard may be national.");
    }

    [TestMethod]
    public async Task AStandardThatBecomesNationalMakesItsExtensionsNational()
    {
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "Extension", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 1 });
        await _factory.SeedAsync(new ModuleStandard { Id = 3, ShortName = "Extension2", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 2, OnlyValidInCountryId = Sweden });
        await _factory.SeedAsync(new ModuleStandard { Id = 4, ShortName = "Other", ScaleId = 3, MainThemeId = 1 });
        var national = Changed();
        national.OnlyValidInCountryId = Norway;

        Assert.IsGreaterThan(0, (await new ModuleStandardService(_factory).SaveAsync(GlobalAdministrator, national)).Count);

        Assert.AreEqual(Norway, (await _factory.FindAsync<ModuleStandard>(1))!.OnlyValidInCountryId);
        Assert.AreEqual(Norway, (await _factory.FindAsync<ModuleStandard>(2))!.OnlyValidInCountryId);
        Assert.AreEqual(Norway, (await _factory.FindAsync<ModuleStandard>(3))!.OnlyValidInCountryId, "Also through another extension.");
        Assert.IsNull((await _factory.FindAsync<ModuleStandard>(4))!.OnlyValidInCountryId);
    }

    [TestMethod]
    public async Task DeletingAStandardEndsItsExtensions()
    {
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "Extension", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 1 });

        var (count, _) = await new ModuleStandardService(_factory).DeleteAsync(GlobalAdministrator, 1);

        Assert.IsGreaterThan(0, count);
        Assert.IsNull(await _factory.FindAsync<ModuleStandard>(1));
        Assert.IsNull((await _factory.FindAsync<ModuleStandard>(2))!.ExtendsStandardId);
    }

    [TestMethod]
    public async Task AnEndProfileDefinedByAStandardHasItsScale()
    {
        await _factory.SeedAsync(new Scale { Id = 4, ShortName = "N", Denominator = 160 });
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "FREMO/N", ScaleId = 4, MainThemeId = 1 });
        var target = new ModuleEndProfileService(_factory);

        Assert.AreEqual(1, (await target.SaveAsync(GlobalAdministrator, new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "FREMO 1", ModuleStandardId = 2 })).Count);
        var saved = (await _factory.FindAsync<ModuleEndProfile>(1))!;
        Assert.AreEqual(2, saved.ModuleStandardId);
        Assert.AreEqual(4, saved.ScaleId, "The scale follows the standard.");

        await target.SaveAsync(GlobalAdministrator, new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "FREMO 1", ModuleStandardId = 0 });
        Assert.IsNull((await _factory.FindAsync<ModuleEndProfile>(1))!.ModuleStandardId, "Zero is no standard.");

        var (_, _, added) = await target.SaveAsync(GlobalAdministrator, new ModuleEndProfile { ScaleId = 3, Designation = "Gone", ModuleStandardId = 99 });
        Assert.IsNull((await _factory.FindAsync<ModuleEndProfile>(added!.Id))!.ModuleStandardId, "A standard that doesn't exist is dropped.");
    }

    [TestMethod]
    public async Task DeletingAStandardKeepsItsEndProfiles()
    {
        await new ModuleEndProfileService(_factory).SaveAsync(GlobalAdministrator, new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "FREMO 1", ModuleStandardId = 1 });

        var (count, _) = await new ModuleStandardService(_factory).DeleteAsync(GlobalAdministrator, 1);

        Assert.IsGreaterThan(0, count);
        Assert.IsNull((await _factory.FindAsync<ModuleEndProfile>(1))!.ModuleStandardId);
    }

    [TestMethod]
    public async Task TheScaleOfEachStandardLocksAnEndProfilesScale()
    {
        await _factory.SeedAsync(new Scale { Id = 4, ShortName = "N", Denominator = 160 });
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "FREMO/N", ScaleId = 4, MainThemeId = 1 });

        var scaleIds = await new ModuleStandardService(_factory).ScaleIdsAsync(GlobalAdministrator);

        Assert.AreEqual(3, scaleIds[1]);
        Assert.AreEqual(4, scaleIds[2]);
        Assert.IsEmpty(await new ModuleStandardService(_factory).ScaleIdsAsync(null));
    }

    [TestMethod]
    public async Task AllStandardsCanDefineAnEndProfile()
    {
        await _factory.SeedAsync(new ModuleStandard { Id = 2, ShortName = "Extension", ScaleId = 3, MainThemeId = 1, ExtendsStandardId = 1 });

        var items = await new ModuleStandardService(_factory).ListboxItemsAsync(GlobalAdministrator);

        CollectionAssert.AreEqual(new[] { "Extension (1:87)", "FREMO/H0-RE (1:87)" }, items.Select(i => i.Description).ToArray());
    }

}