using Microsoft.Data.SqlClient;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// The procedure <c>GetStationCustomerWaybills</c>, which <c>StationWaybillGenerator</c> replaces (Migration step 5.2);
/// kept in the database until step 5.3, for rollback.
/// </summary>
public static class StationWaybillProcedure
{
    public static async Task<SqlDataReader> ExecuteAsync(TestTransaction transaction, (int StationId, int? CustomerId, bool Sending, bool Receiving) query)
    {
        await using var command = transaction.CreateProcedureCommand("GetStationCustomerWaybills");
        command.Parameters.AddWithValue("@StationId", query.StationId);
        command.Parameters.AddWithValue("@StationCustomerId", query.CustomerId.HasValue ? query.CustomerId.Value : DBNull.Value);
        command.Parameters.AddWithValue("@Sending", query.Sending);
        command.Parameters.AddWithValue("@Receiving", query.Receiving);
        return await command.ExecuteReaderAsync();
    }
}