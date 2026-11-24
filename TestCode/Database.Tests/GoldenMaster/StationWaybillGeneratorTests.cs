using System.Text;
using ModulesRegistry.Services.Models;
using ModulesRegistry.Services.Waybills;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// A station's waybills as printed (<see cref="StationWaybillGenerator"/>), on the waybill test world. Until step 5.3 the result was
/// compared with the procedure <c>GetStationCustomerWaybills</c>, which it replaced in 1.17.2; the approved file is the reference since.
/// </summary>
[TestClass]
public class StationWaybillGeneratorTests : DatabaseTest
{
    [TestMethod]
    public async Task GeneratedWaybillsAreApproved()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await WaybillScenario.GenerateWaybillsAsync(Transaction);
        var result = new StringBuilder();
        foreach (var query in WaybillScenario.StationQueries)
            result.Append(WaybillScenario.QueryTitle(query)).Append(WaybillText.Render(await GeneratedWaybillsAsync(query))).Append('\n');
        Approvals.Verify("StationWaybillObjects", result.ToString());
    }

    private async Task<IReadOnlyList<Waybill>> GeneratedWaybillsAsync((int StationId, int? CustomerId, bool Sending, bool Receiving) query)
    {
        await using var dbContext = Transaction.CreateDbContext();
        return await dbContext.StationWaybillsAsync(query.StationId, query.CustomerId, query.Sending, query.Receiving);
    }
}