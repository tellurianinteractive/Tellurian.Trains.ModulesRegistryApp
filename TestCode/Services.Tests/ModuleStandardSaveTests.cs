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

}