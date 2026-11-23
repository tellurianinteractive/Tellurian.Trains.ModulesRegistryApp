using System.Text;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// Golden master of the current waybill procedures and views (Migration step 5.1), on the test world in <c>WaybillScenario.sql</c>.
/// The approved results pin today's behaviour, including its defects (System Design Documents/Waybills.md), so the code that replaces
/// the procedures can be compared with them, and every intended change of behaviour shows up as a reviewed change of an approved file.
/// </summary>
[TestClass]
public class WaybillGoldenMasterTests : DatabaseTest
{
    private static readonly int[] ModuleCustomerIds = [11, 12, 13, 14, 15, 16, 17, 18];
    private static readonly int[] StationIds = [1, 2, 3, 4, 5, 6];

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
        await SeedScenarioAsync();
        await GenerateWaybillsAsync();
        Approvals.Verify(nameof(GeneratedWaybills), await StoredWaybillsAsync());
    }

    [TestMethod]
    public async Task RegeneratedWaybills()
    {
        await SeedScenarioAsync();
        await GenerateWaybillsAsync();
        // Mark all as printed, and change the operating days of a receiving and a sending flow.
        await Transaction.ExecuteScriptAsync("""
            UPDATE StationCustomerWaybill SET PrintCount = 0;
            UPDATE StationCustomerCargo SET OperatingDayId = 1 WHERE Id = 101;
            UPDATE StationCustomerCargo SET OperatingDayId = 10 WHERE Id = 105;
            """);
        await GenerateWaybillsAsync();
        Approvals.Verify(nameof(RegeneratedWaybills), await StoredWaybillsAsync());
    }

    [TestMethod]
    public async Task StationWaybills()
    {
        await SeedScenarioAsync();
        await GenerateWaybillsAsync();
        var result = new StringBuilder();
        foreach (var stationId in StationIds) await AppendStationWaybillsAsync(result, stationId, null, sending: true, receiving: true);
        await AppendStationWaybillsAsync(result, 1, 11, sending: false, receiving: true);
        await AppendStationWaybillsAsync(result, 2, null, sending: true, receiving: false);
        Approvals.Verify(nameof(StationWaybills), result.ToString());
    }

    [TestMethod]
    public async Task LayoutWaybills()
    {
        await SeedScenarioAsync();
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
        await SeedScenarioAsync();
        await using var command = Transaction.CreateProcedureCommand("GetGroupWaybills");
        command.Parameters.AddWithValue("@GroupId", 1);
        await using var reader = await command.ExecuteReaderAsync();
        Approvals.Verify(nameof(GroupWaybills), await ResultText.RenderAsync(reader));
    }

    [TestMethod]
    public async Task EmptyWagonOrders()
    {
        await SeedScenarioAsync();
        var result = new StringBuilder();
        foreach (var stationId in StationIds)
        {
            await using var command = Transaction.CreateProcedureCommand("GetEmptyWagonOrders");
            command.Parameters.AddWithValue("@StationId", stationId);
            await using var reader = await command.ExecuteReaderAsync();
            result.Append($"=== Station {stationId}: ").Append(await ResultText.RenderAsync(reader)).Append('\n');
        }
        Approvals.Verify(nameof(EmptyWagonOrders), result.ToString());
    }

    private async Task SeedScenarioAsync() =>
        await Transaction.ExecuteScriptAsync(await File.ReadAllTextAsync(RepositoryFiles.PathOf("TestCode", "Database.Tests", "GoldenMaster", "WaybillScenario.sql")));

    /// <summary>
    /// What <c>StationCustomerWaybillsService.AddGeneratedCustomerWaybills</c> does, for every module customer in turn.
    /// </summary>
    private async Task GenerateWaybillsAsync()
    {
        foreach (var customerId in ModuleCustomerIds)
        {
            foreach (var procedure in new[] { "AddGeneratedModuleWaybills", "AddGeneratedExternalWaybills", "AddGeneratedShadowYardWaybills" })
            {
                await using var command = Transaction.CreateProcedureCommand(procedure);
                command.Parameters.AddWithValue("@StationCustomerId", customerId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    private async Task<string> StoredWaybillsAsync()
    {
        await using var command = Transaction.CreateCommand(StoredWaybillsQuery);
        await using var reader = await command.ExecuteReaderAsync();
        return await ResultText.RenderAsync(reader);
    }

    /// <summary>
    /// The waybills as printed for a station; <c>Id</c> is the stored waybill's identity value, which isn't stable between tests.
    /// </summary>
    private async Task AppendStationWaybillsAsync(StringBuilder result, int stationId, int? customerId, bool sending, bool receiving)
    {
        await using var command = Transaction.CreateProcedureCommand("GetStationCustomerWaybills");
        command.Parameters.AddWithValue("@StationId", stationId);
        command.Parameters.AddWithValue("@StationCustomerId", customerId.HasValue ? customerId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@Sending", sending);
        command.Parameters.AddWithValue("@Receiving", receiving);
        await using var reader = await command.ExecuteReaderAsync();
        result.Append($"=== Station {stationId}, customer {customerId?.ToString() ?? "all"}, sending {sending}, receiving {receiving}: ")
            .Append(await ResultText.RenderAsync(reader, "Id")).Append('\n');
    }
}