using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Resources;
using System.Text;

namespace ModulesRegistry.Services.Implementations;
public class EmptyWagonOrderService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    private readonly ILogger<EmptyWagonOrderService> Logger;

    public EmptyWagonOrderService(IDbContextFactory<ModulesDbContext> factory, ILogger<EmptyWagonOrderService> logger)
    {
        Factory = factory;
        Logger = logger;
    }

    public async Task<IEnumerable<EmptyWagonOrder>> GetEmptyWagonOrdersAsync(ClaimsPrincipal? principal, int stationId, int? stationCustomerId = null)
    {
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var emptyWagonOrders = new List<EmptyWagonOrder>(200);
                var ownerNames = await GetStationOwnerNames(principal, stationId);
                var command = new SqlCommand("GetEmptyWagonOrders", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                command.Parameters.AddWithValue("@StationId", stationId);
                command.Parameters.AddWithValue("@CustomerId", stationCustomerId);
                try
                {
                    connection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));
                    while (await reader.ReadAsync())
                    {
                        emptyWagonOrders.Add(reader.Map());
                        emptyWagonOrders.Last().OwnerNames = ownerNames;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "An error occured: {SqlErrorMessage}", ex.Message);
                    throw;
                }
                return emptyWagonOrders;
            }
        }
        return Array.Empty<EmptyWagonOrder>();
    }

    private async Task<string> GetStationOwnerNames(ClaimsPrincipal? principal, int stationId)
    {
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var command = new SqlCommand("SELECT [Names] FROM ModuleOwnerNames WHERE StationId = @StationId", connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 120
                };
                command.Parameters.AddWithValue("@StationId", stationId);
                try
                {
                    connection.Open();
                    var result = await command.ExecuteScalarAsync();
                    if (result is string names) return names;
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "An error occured: {SqlErrorMessage}", ex.Message);
                    throw;
                }
            }

        }
        return string.Empty;
    }
}

internal static class EmptyWagonOrderMapper
{
    public static EmptyWagonOrder Map(this IDataRecord record) =>
        new()
        {
            StationName = record.GetString("StationName"),
            StationSignature = record.GetString("StationSignature"),
            CustomerName = record.GetString("CustomerName"),
            NumberOfWagons = record.GetInt("NumberOfWagons"),
            SpecificWagonClasses = record.GetString("SpecificWagonClass"),
            DefaultClasses = record.GetString("DefaultClasses"),
            FromYear = record.GetNullableInt("FromYear", null),
            UptoYear = record.GetNullableInt("UptoYear", null),
            CargoTrackOrArea = record.GetString("CargoTrackOrArea"),
            CargoTrackOrAreaColor = record.GetString("CargoTrackOrAreaColor"),
            CustomerTrackOrArea = record.GetString("CustomerTrackOrArea"),
            CustomerTrackOrAreaColor = record.GetString("CustomerTrackOrAreaColor"),
            Languages = record.GetString("Languages"),
            DomainSuffix = record.GetString("DomainSuffix"),
            CargoName = record.GetCargoName(record.GetString("Languages")),
        };

    private static string GetCargoName(this IDataRecord record, string languages)
    {
        int index;
        try
        {
            index = record.GetOrdinal(languages);
            var value = record.GetString(index);
            if (value.HasValue()) return value;
        }
        catch (IndexOutOfRangeException)
        {
        }

        index = record.GetOrdinal("EN");
        return record.GetString(index);
    }
}
