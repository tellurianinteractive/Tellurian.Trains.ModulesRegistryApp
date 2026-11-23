namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// The test world of the waybill tests (<c>WaybillScenario.sql</c>) and the stored waybills generated from it.
/// </summary>
public static class WaybillScenario
{
    public static readonly int[] ModuleCustomerIds = [11, 12, 13, 14, 15, 16, 17, 18, 19];
    public static readonly int[] StationIds = [1, 2, 3, 4, 5, 6];

    /// <summary>
    /// The station waybill queries: every station sending and receiving, one customer receiving only, one station sending only.
    /// </summary>
    public static readonly (int StationId, int? CustomerId, bool Sending, bool Receiving)[] StationQueries =
        [.. StationIds.Select(id => (id, (int?)null, true, true)), (1, 11, false, true), (2, null, true, false)];

    public static async Task SeedAsync(TestTransaction transaction) =>
        await transaction.ExecuteScriptAsync(await File.ReadAllTextAsync(RepositoryFiles.PathOf("TestCode", "Database.Tests", "GoldenMaster", "WaybillScenario.sql")));

    /// <summary>
    /// What <c>StationCustomerWaybillsService.AddGeneratedCustomerWaybills</c> does, for every module customer in turn.
    /// </summary>
    public static async Task GenerateWaybillsAsync(TestTransaction transaction)
    {
        foreach (var customerId in ModuleCustomerIds)
        {
            foreach (var procedure in new[] { "AddGeneratedModuleWaybills", "AddGeneratedExternalWaybills", "AddGeneratedShadowYardWaybills" })
            {
                await using var command = transaction.CreateProcedureCommand(procedure);
                command.Parameters.AddWithValue("@StationCustomerId", customerId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public static string QueryTitle((int StationId, int? CustomerId, bool Sending, bool Receiving) query) =>
        $"=== Station {query.StationId}, customer {query.CustomerId?.ToString() ?? "all"}, sending {query.Sending}, receiving {query.Receiving}: ";
}