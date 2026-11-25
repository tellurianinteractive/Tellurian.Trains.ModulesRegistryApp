namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// Waybills are generated only between stations with the same theme: the primary theme of the station's primary module, not the
/// theme of its standard (since the themes moved to the modules). A secondary theme gives no waybills.
/// </summary>
[TestClass]
public class WaybillThemeTests : DatabaseTest
{
    private const int EastvilleModule = 5, EastvilleTimber = 113; // Eastville has the US standard; the other stations are European.

    [TestMethod]
    public async Task TheUSModuleGetsNoWaybillsWithEuropeanStations()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await WaybillScenario.GenerateWaybillsAsync(Transaction);

        Assert.AreEqual(0, await TimberWaybillsAsync());
    }

    [TestMethod]
    public async Task TheModulesPrimaryThemeDecidesNotItsStandards()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await Transaction.ExecuteScriptAsync($"UPDATE Module SET PrimaryThemeId = 1 WHERE Id = {EastvilleModule};");
        await WaybillScenario.GenerateWaybillsAsync(Transaction);

        Assert.IsGreaterThan(0, await TimberWaybillsAsync());
    }

    [TestMethod]
    public async Task ASecondaryThemeGivesNoWaybills()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await Transaction.ExecuteScriptAsync($"UPDATE Module SET SecondaryThemeId = 1 WHERE Id = {EastvilleModule};");
        await WaybillScenario.GenerateWaybillsAsync(Transaction);

        Assert.AreEqual(0, await TimberWaybillsAsync());
    }

    private async Task<int> TimberWaybillsAsync()
    {
        await using var command = Transaction.CreateCommand(
            $"SELECT COUNT(*) FROM StationCustomerWaybill WHERE (StationCustomerCargoId = {EastvilleTimber} OR OtherStationCustomerCargoId = {EastvilleTimber}) AND OtherStationCustomerCargoId IS NOT NULL");
        return (int)(await command.ExecuteScalarAsync())!;
    }
}