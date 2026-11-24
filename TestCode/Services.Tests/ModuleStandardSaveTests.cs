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
        await _factory.SeedAsync(new ModuleStandard { Id = 1, ShortName = "FREMO/H0-RE", ScaleId = 3, TrackSystem = "Track", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC", PreferredTheme = "Europe", AcceptedNorm = "Fremo", MainThemeId = 1 });
        await _factory.SeedAsync(new ModuleEndProfile { Id = 1, ScaleId = 3, Designation = "FREMO 1" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static ModuleStandard Changed() => new()
    {
        Id = 1, ShortName = " FREMO/H0-RE ", ScaleId = 3, TrackSystem = "", Wheelset = "RP25", Couplings = "Hooks", Electricity = "DC",
        PreferredTheme = "Europe", AcceptedNorm = " https://fremo-net.eu/norm.pdf ", MainThemeId = 1, NormalGauge = 0, NarrowGauge = 6.5,
    };

    [TestMethod]
    public async Task AGlobalAdministratorSavesACorrectedStandard()
    {
        var (count, _, _) = await new ModuleStandardService(_factory).SaveAsync(GlobalAdministrator, Changed());

        Assert.AreEqual(1, count);
        var saved = (await _factory.FindAsync<ModuleStandard>(1))!;
        Assert.AreEqual("FREMO/H0-RE", saved.ShortName);
        Assert.IsNull(saved.TrackSystem, "An empty optional text is saved as empty.");
        Assert.AreEqual("https://fremo-net.eu/norm.pdf", saved.AcceptedNorm);
        Assert.IsNull(saved.NormalGauge, "Zero is no gauge.");
        Assert.AreEqual(6.5, saved.NarrowGauge);
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

}