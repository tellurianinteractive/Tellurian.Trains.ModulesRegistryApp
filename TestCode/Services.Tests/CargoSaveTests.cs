using Microsoft.Extensions.Caching.Memory;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Cargo types are reference data shared by all countries: only global administrators change them (Migration step 3.6; before,
/// any signed-in user could save, although only global administrators got the edit links). The names and years are corrected.
/// </summary>
[TestClass]
public sealed class CargoSaveTests
{
    private SqliteDbContextFactory _factory = null!;
    private MemoryCache _cache = null!;

    private static ClaimsPrincipal Principal(params Claim[] claims) => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.CountryId, "1"), .. claims], "Test"));
    private static ClaimsPrincipal GlobalAdministrator => Principal(new Claim(AppClaimTypes.GlobalAdministrator, "true"));
    private static ClaimsPrincipal CountryAdministrator => Principal(new Claim(AppClaimTypes.CountryAdministrator, "true"));
    private static ClaimsPrincipal OrdinaryUser => Principal();

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        _cache = new MemoryCache(new MemoryCacheOptions());
        await _factory.SeedAsync(new NHM { Id = 1, Code = "1", LevelDigits = 2 });
        await _factory.SeedAsync(new Cargo { Id = 1, NhmCode = 1, EN = "Coal", DE = "Kohle", DA = "Kul", SV = "Kol", NB = "Kull" });
    }

    [TestCleanup]
    public void Cleanup()
    {
        _cache.Dispose();
        _factory.Dispose();
    }

    [TestMethod]
    public async Task OnlyAGlobalAdministratorSavesOrDeletes()
    {
        var target = new CargoService(_factory, _cache);
        Cargo Changed(string name) => new() { Id = 1, NhmCode = 1, EN = name, DE = "Kohle", DA = "Kul", SV = "Kol", NB = "Kull" };

        Assert.AreEqual(0, (await target.SaveAsync(OrdinaryUser, Changed("Changed"))).Count);
        Assert.AreEqual(0, (await target.SaveAsync(CountryAdministrator, Changed("Changed"))).Count);
        Assert.AreEqual(0, (await target.DeleteAsync(OrdinaryUser, 1)).Count);
        Assert.AreEqual(0, (await target.DeleteAsync(CountryAdministrator, 1)).Count);
        Assert.AreEqual("Coal", (await _factory.FindAsync<Cargo>(1))!.EN);
        Assert.IsGreaterThan(0, (await target.SaveAsync(GlobalAdministrator, Changed("Hard coal"))).Count);
    }

    [TestMethod]
    public async Task NamesAndYearsAreCorrected()
    {
        var target = new CargoService(_factory, _cache);

        var result = await target.SaveAsync(GlobalAdministrator, new Cargo
        {
            NhmCode = 1,
            EN = "\u201CWood\u201D \u2013 logs", DE = "  Holz  ", DA = "Tr\u00E6", SV = "Tr\u00E4", NB = "Tre",
            FR = " ", DefaultClasses = "", FromYear = 1990, UptoYear = 1950,
        });

        var saved = (await _factory.FindAsync<Cargo>(result.Entity!.Id))!;
        Assert.AreEqual("\"Wood\" - logs", saved.EN);
        Assert.AreEqual("Holz", saved.DE);
        Assert.IsNull(saved.FR, "An empty optional name is saved as empty.");
        Assert.IsNull(saved.DefaultClasses);
        Assert.AreEqual((short)1950, saved.FromYear, "Reversed years are swapped.");
        Assert.AreEqual((short)1990, saved.UptoYear);
    }
}