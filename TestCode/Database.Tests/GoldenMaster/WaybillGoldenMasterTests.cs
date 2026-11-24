using System.Text;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// Golden master of the remaining waybill procedures (Migration step 5.1), on the test world in <c>WaybillScenario.sql</c>.
/// The approved results pin today's behaviour, including its defects (System Design Documents/Waybills.md), so the code that replaces
/// the procedures can be compared with them, and every intended change of behaviour shows up as a reviewed change of an approved file.
/// A station's printed waybills are made in code since 1.17.2 (<see cref="StationWaybillGeneratorTests"/>).
/// </summary>
[TestClass]
public class WaybillGoldenMasterTests : DatabaseTest
{
    private const string StoredWaybillsQuery = """
        SELECT
            SC.CustomerName AS Customer,
            C.EN AS Cargo,
            CD.FullName AS Direction,
            OSC.CustomerName AS OtherModuleCustomer,
            OEC.CustomerName AS OtherExternalCustomer,
            R.LocalName AS OtherRegion,
            OD.FullName AS OperatingDay,
            W.IsManuallyCreated, W.HasEmptyReturn, W.HasSameCargoReturn, W.HideLoadingTimes, W.HideUnloadingTimes,
            W.PrintPerOperatingDay, W.PrintCount, W.SequenceNumber
        FROM
            StationCustomerWaybill AS W
            INNER JOIN StationCustomer AS SC ON SC.Id = W.StationCustomerId
            INNER JOIN StationCustomerCargo AS SCC ON SCC.Id = W.StationCustomerCargoId
            INNER JOIN Cargo AS C ON C.Id = SCC.CargoId
            INNER JOIN CargoDirection AS CD ON CD.Id = SCC.DirectionId
            INNER JOIN OperatingDay AS OD ON OD.Id = W.OperatingDayId
            LEFT JOIN StationCustomerCargo AS OSCC ON OSCC.Id = W.OtherStationCustomerCargoId
            LEFT JOIN StationCustomer AS OSC ON OSC.Id = OSCC.StationCustomerId
            LEFT JOIN ExternalStationCustomerCargo AS OECC ON OECC.Id = W.OtherExternalCustomerCargoId
            LEFT JOIN ExternalStationCustomer AS OEC ON OEC.Id = OECC.ExternalStationCustomerId
            LEFT JOIN Region AS R ON R.Id = W.OtherRegionId
        """;

    [TestMethod]
    public async Task GeneratedWaybills()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await WaybillScenario.GenerateWaybillsAsync(Transaction);
        Approvals.Verify(nameof(GeneratedWaybills), await StoredWaybillsAsync());
    }

    [TestMethod]
    public async Task RegeneratedWaybills()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await WaybillScenario.GenerateWaybillsAsync(Transaction);
        // Mark all as printed, and change the operating days of a receiving and a sending flow.
        await Transaction.ExecuteScriptAsync("""
            UPDATE StationCustomerWaybill SET PrintCount = 0;
            UPDATE StationCustomerCargo SET OperatingDayId = 1 WHERE Id = 101;
            UPDATE StationCustomerCargo SET OperatingDayId = 10 WHERE Id = 105;
            """);
        await WaybillScenario.GenerateWaybillsAsync(Transaction);
        Approvals.Verify(nameof(RegeneratedWaybills), await StoredWaybillsAsync());
    }

    [TestMethod]
    public async Task LayoutWaybills()
    {
        await WaybillScenario.SeedAsync(Transaction);
        var result = new StringBuilder();
        foreach (var (layoutId, stationId) in new (int, int?)[] { (1, null), (1, 1), (2, null), (3, null) })
        {
            await using var command = Transaction.CreateProcedureCommand("GetLayoutWaybills2");
            command.Parameters.AddWithValue("@LayoutId", layoutId);
            if (stationId.HasValue) command.Parameters.AddWithValue("@StationId", stationId.Value);
            await using var reader = await command.ExecuteReaderAsync();
            result.Append($"=== Layout {layoutId}, station {stationId?.ToString() ?? "all"}: ").Append(await ResultText.RenderAsync(reader)).Append('\n');
        }
        Approvals.Verify(nameof(LayoutWaybills), result.ToString());
    }

    [TestMethod]
    public async Task GroupWaybills()
    {
        await WaybillScenario.SeedAsync(Transaction);
        await using var command = Transaction.CreateProcedureCommand("GetGroupWaybills");
        command.Parameters.AddWithValue("@GroupId", 1);
        await using var reader = await command.ExecuteReaderAsync();
        Approvals.Verify(nameof(GroupWaybills), await ResultText.RenderAsync(reader));
    }

    [TestMethod]
    public async Task EmptyWagonOrders()
    {
        await WaybillScenario.SeedAsync(Transaction);
        var result = new StringBuilder();
        foreach (var stationId in WaybillScenario.StationIds)
        {
            await using var command = Transaction.CreateProcedureCommand("GetEmptyWagonOrders");
            command.Parameters.AddWithValue("@StationId", stationId);
            await using var reader = await command.ExecuteReaderAsync();
            result.Append($"=== Station {stationId}: ").Append(await ResultText.RenderAsync(reader)).Append('\n');
        }
        Approvals.Verify(nameof(EmptyWagonOrders), result.ToString());
    }

    private async Task<string> StoredWaybillsAsync()
    {
        await using var command = Transaction.CreateCommand(StoredWaybillsQuery);
        await using var reader = await command.ExecuteReaderAsync();
        return await ResultText.RenderAsync(reader);
    }
}