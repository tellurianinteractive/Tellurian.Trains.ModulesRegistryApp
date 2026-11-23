using System.Reflection;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using ModulesRegistry.Services.Waybills;

namespace ModulesRegistry.Database.Tests.GoldenMaster;

/// <summary>
/// Compares the station waybills made in code with those of the views, for every station with stored waybills in the production database
/// (Migration step 5.2). Read only, with the read-only login (user secret <c>ConnectionStrings:TimetablePlanningDatabaseReadOnly</c>);
/// runs only when the environment variable <c>RUN_PRODUCTION_COMPARISON</c> is 1. The read-only login can't execute procedures,
/// so the procedure <c>GetStationCustomerWaybills</c> is run as its query on the six views.
/// </summary>
[TestClass]
public class ProductionStationWaybillComparison
{
    public TestContext TestContext { get; set; } = null!;

    private const string ProcedureQuery = """
        SELECT * FROM ModuleSupplierWaybill WHERE DestinationStationId = @StationId
        UNION SELECT * FROM ExternalSupplierWaybill WHERE DestinationStationId = @StationId
        UNION SELECT * FROM ShadowYardSupplierWaybill WHERE DestinationStationId = @StationId
        UNION SELECT * FROM ModuleConsumerWaybill WHERE OriginStationId = @StationId
        UNION SELECT * FROM ExternalConsumerWaybill WHERE OriginStationId = @StationId
        UNION SELECT * FROM ShadowYardConsumerWaybill WHERE OriginStationId = @StationId
        """;

    [TestMethod]
    public async Task StationWaybillsAreTheSame()
    {
        if (Environment.GetEnvironmentVariable("RUN_PRODUCTION_COMPARISON") != "1") Assert.Inconclusive("Set RUN_PRODUCTION_COMPARISON=1 to run.");
        var connectionString = new ConfigurationBuilder().AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true).Build()
            .GetConnectionString("TimetablePlanningDatabaseReadOnly");
        if (string.IsNullOrWhiteSpace(connectionString)) Assert.Inconclusive("No read-only connection string.");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        List<int> stationIds = [];
        await using (var command = new SqlCommand("SELECT DISTINCT SC.StationId FROM StationCustomerWaybill W INNER JOIN StationCustomerCargo SCC ON SCC.Id = W.StationCustomerCargoId INNER JOIN StationCustomer SC ON SC.Id = SCC.StationCustomerId ORDER BY SC.StationId", connection))
        await using (var reader = await command.ExecuteReaderAsync())
            while (await reader.ReadAsync()) stationIds.Add(reader.GetInt32(0));

        var report = new StringBuilder();
        var differing = 0;
        var total = 0;
        foreach (var stationId in stationIds)
        {
            List<Waybill> expected = [];
            await using (var command = new SqlCommand(ProcedureQuery, connection) { CommandTimeout = 120 })
            {
                command.Parameters.AddWithValue("@StationId", stationId);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync()) expected.Add(reader.MapWaybill(stationId));
            }
            await using var dbContext = new ModulesDbContext(new DbContextOptionsBuilder<ModulesDbContext>().UseSqlServer(connection).Options);
            var actual = await dbContext.StationWaybillsAsync(stationId, null, sending: true, receiving: true);
            total += expected.Count;
            var expectedById = expected.ToLookup(w => w.Id);
            var actualById = actual.ToLookup(w => w.Id);
            foreach (var id in expectedById.Select(g => g.Key).Union(actualById.Select(g => g.Key)).Order())
            {
                var e = WaybillText.Render(expectedById[id]);
                var a = WaybillText.Render(actualById[id]);
                if (e == a) continue;
                differing++;
                report.Append($"=== Station {stationId}, waybill {id}\n--- views:\n{e}\n--- code:\n{a}\n");
            }
        }
        var file = Path.Combine(Path.GetTempPath(), "ProductionStationWaybillComparison.txt");
        await File.WriteAllTextAsync(file, $"{stationIds.Count} stations, {total} waybills from the views, {differing} differing waybill ids\n\n{report}");
        TestContext.WriteLine($"{stationIds.Count} stations, {total} waybills, {differing} differing. Report: {file}");
        Assert.AreEqual(0, differing, $"See {file}");
    }
}