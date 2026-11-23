using System.Text;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using ModulesRegistry.Services.Waybills;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// The station waybills made in code (<see cref="StationWaybillGenerator"/>, Migration step 5.2) must be the same <see cref="Waybill"/> objects
/// as those the app made from the procedure <c>GetStationCustomerWaybills</c> (with <c>MapWaybill</c>), on the waybill test world.
/// </summary>
[TestClass]
public class StationWaybillGeneratorTests : DatabaseTest
{
    [TestMethod]
    public async Task ProcedureWaybillsAreApproved()
    {
        await SeedAndGenerateAsync();
        Approvals.Verify("StationWaybillObjects", await RenderAsync(ProcedureWaybillsAsync));
    }

    [TestMethod]
    public async Task GeneratedWaybillsAreTheProcedureWaybills()
    {
        await SeedAndGenerateAsync();
        var expected = await RenderAsync(ProcedureWaybillsAsync);
        var actual = await RenderAsync(GeneratedWaybillsAsync);
        Assert.AreEqual(expected, actual);
        Approvals.Verify("StationWaybillObjects", actual);
    }

    private async Task SeedAndGenerateAsync()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await WaybillScenario.GenerateWaybillsAsync(Transaction);
    }

    private static async Task<string> RenderAsync(Func<(int StationId, int? CustomerId, bool Sending, bool Receiving), Task<IReadOnlyList<Waybill>>> waybills)
    {
        var result = new StringBuilder();
        foreach (var query in WaybillScenario.StationQueries)
            result.Append(WaybillScenario.QueryTitle(query)).Append(WaybillText.Render(await waybills(query))).Append('\n');
        return result.ToString();
    }

    private async Task<IReadOnlyList<Waybill>> ProcedureWaybillsAsync((int StationId, int? CustomerId, bool Sending, bool Receiving) query)
    {
        List<Waybill> waybills = [];
        await using var reader = await StationWaybillProcedure.ExecuteAsync(Transaction, query);
        while (await reader.ReadAsync()) waybills.Add(reader.MapWaybill(query.StationId));
        return waybills;
    }

    private async Task<IReadOnlyList<Waybill>> GeneratedWaybillsAsync((int StationId, int? CustomerId, bool Sending, bool Receiving) query)
    {
        await using var dbContext = Transaction.CreateDbContext();
        return await dbContext.StationWaybillsAsync(query.StationId, query.CustomerId, query.Sending, query.Receiving);
    }
}