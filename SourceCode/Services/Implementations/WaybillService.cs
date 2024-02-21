using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using ModulesRegistry.Services.Models;
using System.Data;
using System.Resources;

namespace ModulesRegistry.Services.Implementations;

public class WaybillService(IDbContextFactory<ModulesDbContext> factory, ILogger<WaybillService> logger)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ILogger<WaybillService> Logger = logger;

    public async Task<string> GetStationOwnerNames(ClaimsPrincipal? principal, int stationId)
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

    public async Task<IEnumerable<Waybill>> GetStationCustomerWaybills(ClaimsPrincipal? principal, int stationId, int? stationCustomerId, bool receiving, bool sending)
    {
        List<Waybill> waybills = new List<Waybill>(200);
        var ownerNames = await GetStationOwnerNames(principal, stationId);
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var command = new SqlCommand("GetStationCustomerWaybills", connection)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                command.Parameters.AddWithValue("@StationId", stationId);
                command.Parameters.AddWithValue("StationCustomerId", stationCustomerId);
                command.Parameters.AddWithValue("@Sending", sending);
                command.Parameters.AddWithValue("@Receiving", receiving);
                try
                {
                    connection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));
                    while (await reader.ReadAsync())
                    {
                        waybills.Add(reader.MapWaybill(stationId));
                        waybills.Last().OwnerNames = ownerNames;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "An error occured: {SqlErrorMessage}", ex.Message);
                    throw;
                }
            }
        }
        return waybills;

    }


    public async Task<IEnumerable<Waybill>> GetStationCustomerWaybills2(ClaimsPrincipal? principal, int stationId, int? stationCustomerId, bool receiving, bool sending)
    {
        List<Waybill> waybills = new List<Waybill>(200);
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var sql = getSql(receiving, sending);
                if (sql.HasNoValue()) return waybills;
                var command = new SqlCommand(sql, connection)
                {
                    CommandType = CommandType.Text,
                    CommandTimeout = 120
                };
                command.Parameters.AddWithValue("@StationId", stationId);
                command.Parameters.AddWithNullableValue("@StationCustomerId", stationCustomerId);

                try
                {
                    connection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));
                    while (await reader.ReadAsync())
                    {
                        waybills.Add(reader.MapWaybill(stationId));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "An error occured: {ErrorMessage}", ex.Message);
                    throw;
                }

            }
        }
        return waybills;

        static string getSql(bool receiving, bool sending) =>
            receiving && sending ? sqlSupplier() + " UNION " + sqlConsumer() :
            receiving ? sqlSupplier() :
            sending ? sqlConsumer() :
            string.Empty;

        static string sqlSupplier() =>
            $"""
            SELECT * FROM ModuleSupplierWaybill AS MSW
                WHERE MSW.DestinationStationId = @StationId AND ( @StationCustomerId IS NULL OR MSW.DestinationStationCustomerId = @StationCustomerId )
            UNION
            SELECT * FROM ExternalSupplierWaybill AS ESW
            	WHERE ESW.DestinationStationId = @StationId AND ( @StationCustomerId IS NULL OR ESW.DestinationStationCustomerId = @StationCustomerId )
            UNION
            SELECT * FROM ShadowYardSupplierWaybill AS SYSW 
                WHERE SYSW.DestinationStationId = @StationId AND ( @StationCustomerId IS NULL OR SYSW.DestinationStationCustomerId = @StationCustomerId )
            """;

        static string sqlConsumer() =>
            $"""
            SELECT * FROM ModuleConsumerWaybill AS MCW
                WHERE MCW.OriginStationId = @StationId AND ( @StationCustomerId IS NULL OR MCW.OriginStationCustomerId = @StationCustomerId )
            UNION
            SELECT * FROM ExternalConsumerWaybill AS ECW
            	WHERE ECW.OriginStationId = @StationId AND ( @StationCustomerId IS NULL OR ECW.OriginStationCustomerId = @StationCustomerId )
            UNION
            SELECT * FROM ShadowYardConsumerWaybill AS SYCW 
                WHERE SYCW.OriginStationId = @StationId AND ( @StationCustomerId IS NULL OR SYCW.OriginStationCustomerId = @StationCustomerId )
            """;
    }
    public async Task<IEnumerable<Waybill>> GetLayoutWaybills(ClaimsPrincipal? principal, int layoutId, int? stationId)
    {
        List<Waybill> waybills = new List<Waybill>(200);
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var command = new SqlCommand("GetLayoutWaybills", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@LayoutId", layoutId);
                if (stationId.HasValue)
                {
                    command.Parameters.AddWithValue("@StationId", stationId);
                }
                try
                {
                    connection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));

                    while (await reader.ReadAsync())
                    {
                        waybills.Add(reader.MapWaybill(stationId ?? 0, true));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "An error occured: {ErrorMessage}", ex.Message);
                    return waybills;
                }
            }
        }
        return waybills;
    }
}
internal static class WaybillMapper
{
    public static Waybill MapWaybill(this IDataRecord record, int stationId, bool isLayoutInternal = false)
    {
        var waybill = new Waybill(record.MapOriginCargoCustomer(), record.MapDestinationCargoCustomer())
        {
            Id = record.GetInt("Id"),
            OwnerStationId = stationId,
            Quantity = GetQuantity(record),
            QuantityShortUnit = record.GetString("QuantityShortUnit"),
            OperatorName = string.Empty, // To be supported
            DefaultWagonClass = record.GetString("DefaultClasses"),
            SpecialWagonClass = record.GetString("SpecificWagonClass"),
            PrintCount = record.GetInt("PrintCount"),
            PrintPerOperatingDay = record.GetBool("PrintPerOperatingDay"),
            HasEmptyReturn = record.GetBool("HasEmptyReturn"),
            //MatchReturn = record.GetBool("MatchReturn")
        };
        waybill.Origin.Waybill = waybill;
        waybill.Destination.Waybill = waybill;
        waybill.Origin.DisplayReadyTime = waybill.Origin.ReadyTimeResourceKey.HasValueExcept("n/a");
        waybill.Destination.DisplayReadyTime = waybill.Destination.ReadyTimeResourceKey.HasValueExcept("n/a");
        waybill.IsLayoutInternal = isLayoutInternal;
        if (isLayoutInternal)
        {
            waybill.OwnerNames = record.GetString("OwnerNames");
        }

        return waybill;

        // Fix until the measning of 'quantity' has a clearer definition. 
        static int GetQuantity(IDataRecord record)
        {
            if (record.GetBool("QuantityIsBearer")) return 1;
            return record.GetInt("Quantity");
        }
    }

    internal static CargoCustomer MapOriginCargoCustomer(this IDataRecord record) =>
        new()
        {
            IsOrigin = true,
            Name = record.GetString("SenderName"),
            StationId = record.GetInt("OriginStationId"),
            StationName = record.GetString("OriginStationName"),
            InternationalStationName = record.GetString("OriginInternationalStationName"),
            Languages = record.GetString("OriginLanguages", "en"),
            DomainSuffix = record.GetString("OriginDomainSuffix"),
            ForeColor = record.GetString("OriginForeColor"),
            BackColor = record.GetString("OriginBackColor"),
            CargoName = record.GetString(record.GetCargoColumnName("OriginLanguages")),
            PackagingUnitResourceKey = record.GetString("PackagingUnitResourceName"),
            QuantityUnitResourceKey = record.GetString("QuanityUnitResourceName"),
            IsModuleStation = record.GetBool("OriginIsModuleStation"),
            OperationDaysFlags = record.GetByte("SendingDayFlag"),
            ReadyTimeResourceKey = record.GetString("SenderReadyTime"),
            TrackOrArea = record.GetString("SenderTrackOrArea"),
            TrackOrAreaColor = record.GetString("SenderTrackOrAreaColor"),
            CargoTrackOrArea = record.GetString("SenderCargoTrackOrArea"),
            CargoTrackOrAreaColor = record.GetString("SenderCargoTrackOrAreaColor"),
            FromYear = record.GetNullableInt("SenderFromYear", null),
            UptoYear = record.GetNullableInt("SenderUptoYear", null)
        };

    internal static CargoCustomer MapDestinationCargoCustomer(this IDataRecord record) =>
        new()
        {
            Name = record.GetString("ReceiverName"),
            StationId = record.GetInt("DestinationStationId"),
            StationName = record.GetString("DestinationStationName"),
            InternationalStationName = record.GetString("DestinationInternationalStationName"),
            Languages = record.GetString("DestinationLanguages", "en"),
            DomainSuffix = record.GetString("DestinationDomainSuffix"),
            ForeColor = record.GetString("DestinationForeColor"),
            BackColor = record.GetString("DestinationBackColor"),
            CargoName = record.GetString(record.GetCargoColumnName("DestinationLanguages")),
            PackagingUnitResourceKey = record.GetString("PackagingUnitResourceName"),
            PackagingPrepositionResourceCode = record.GetString("PackagingPrepositionResourceCode"),
            QuantityUnitResourceKey = record.GetString("QuanityUnitResourceName"),
            IsModuleStation = record.GetBool("DestinationIsModuleStation"),
            OperationDaysFlags = record.GetByte("ReceivingDayFlag"),
            ReadyTimeResourceKey = record.GetString("ReceiverReadyTime"),
            TrackOrArea = record.GetString("ReceiverTrackOrArea"),
            TrackOrAreaColor = record.GetString("ReceiverTrackOrAreaColor"),
            CargoTrackOrArea = record.GetString("ReceiverCargoTrackOrArea"),
            CargoTrackOrAreaColor = record.GetString("ReceiverCargoTrackOrAreaColor"),
            FromYear = record.GetNullableInt("ReceiverFromYear", null),
            UptoYear = record.GetNullableInt("ReceiverUptoYear", null)
        };
    private static string GetCargoColumnName(this IDataRecord record, string columnName)
    {
        var language = record.GetString(columnName, "EN").FirstItem("EN");
        if (LanguageExtensions.CargoSupportedLanguages.Any(l => l.Equals(language, StringComparison.OrdinalIgnoreCase))) return language;
        return "EN";
    }
}

internal static class SqlHelpers
{
    public static string OrNull(this int? value) =>
        value.HasValue ? value.Value.ToString() :
        "NULL";
}


