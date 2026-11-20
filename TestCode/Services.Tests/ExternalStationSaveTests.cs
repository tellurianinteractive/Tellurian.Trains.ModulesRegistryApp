using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Saving external stations (Migration step 3.6): any signed-in user may add and edit them, the texts are corrected by the
/// rules of the region's country, a saved station keeps its region, and an unchanged save is "no modification" for everyone.
/// </summary>
[TestClass]
public sealed class ExternalStationSaveTests
{
    private const int Germany = 4, Hessen = 1, Bayern = 2;
    private SqliteDbContextFactory _factory = null!;
    private ExternalStationService Target => new(_factory);

    private static ClaimsPrincipal User(int userId = 5) => new(new ClaimsIdentity(
        [new Claim(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new Claim(AppClaimTypes.UserId, userId.ToString()), new Claim(AppClaimTypes.CountryId, "1")], "Test"));

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _factory = new SqliteDbContextFactory();
        await _factory.SeedAsync(new Country { Id = Germany, EnglishName = "Germany", DomainSuffix = "de", Languages = "de" });
        await _factory.SeedAsync(
            new Region { Id = Hessen, CountryId = Germany, LocalName = "Hessen", BackColor = "#FFFFE0", ForeColor = "#000000" },
            new Region { Id = Bayern, CountryId = Germany, LocalName = "Bayern", BackColor = "#FFFFE0", ForeColor = "#000000" });
        await _factory.SeedAsync(new ExternalStation { Id = 1, RegionId = Hessen, FullName = "Frankfurt Hbf", Signature = "FF" });
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    private static ExternalStation Stored() => new() { Id = 1, RegionId = Hessen, FullName = "Frankfurt Hbf", Signature = "FF" };

    [TestMethod]
    [DataRow(5, DisplayName = "A user")]
    [DataRow(1, DisplayName = "The super user")]
    public async Task AnUnchangedSaveIsNoModification(int userId) =>
        Assert.AreEqual(-1, (await Target.SaveAsync(User(userId), Stored())).Count);

    [TestMethod]
    public async Task ANewStationIsCorrectedByTheRulesOfTheRegionsCountry()
    {
        var result = await Target.SaveAsync(User(), new ExternalStation { RegionId = Hessen, FullName = "darmstadt Hbf", Signature = "fd", InternationalName = "" });

        Assert.IsGreaterThan(0, result.Count);
        var saved = (await _factory.FindAsync<ExternalStation>(result.Entity!.Id))!;
        Assert.AreEqual("Darmstadt Hbf", saved.FullName);
        Assert.AreEqual("FD", saved.Signature, "German signatures are in capitals.");
        Assert.IsNull(saved.InternationalName, "An empty optional name is saved as empty.");
    }

    [TestMethod]
    public async Task ASavedStationKeepsItsRegion()
    {
        var moved = Stored();
        moved.RegionId = Bayern;

        Assert.AreEqual(0, (await Target.SaveAsync(User(), moved)).Count);
        Assert.AreEqual(Hessen, (await _factory.FindAsync<ExternalStation>(1))!.RegionId);
    }
}
