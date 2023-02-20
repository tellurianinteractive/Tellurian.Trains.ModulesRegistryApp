using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Resources;

namespace ModulesRegistry.Services.Implementations;

public class WaybillService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    private readonly ILogger<WaybillService> Logger;

    public WaybillService(IDbContextFactory<ModulesDbContext> factory, ILogger<WaybillService> logger)
    {
        Factory = factory;
        Logger = logger;
    }

    public async Task<IEnumerable<Waybill>> GetStationCustomerWaybills(ClaimsPrincipal? principal, int stationId, int? stationCustomerId, bool receiving, bool sending)
    {
        List<Waybill> waybills = new List<Waybill>(200);
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
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
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
                var sql = getSql(stationId, stationCustomerId, receiving, sending);
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
                    Logger.LogCritical(ex, ex.Message);
                    throw;
                }

            }
        }
        return waybills;

        static string getSql(int stationId, int? stationCustomerId, bool receiving, bool sending) =>
            receiving && sending ? sqlSupplier(stationId, stationCustomerId) + " UNION " + sqlConsumer(stationId, stationCustomerId) :
            receiving ? sqlSupplier(stationId, stationCustomerId) :
            sending ? sqlConsumer(stationId, stationCustomerId) :
            string.Empty;

        static string sqlSupplier(int stationId, int? stationCustomerId) =>
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

        static string sqlConsumer(int stationId, int? stationCustomerId) =>
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

        static string StationCustomerCriteria(int? stationCustomerId, string columnName) =>
            stationCustomerId.HasValue ? $"AND {columnName} = {stationCustomerId.Value}" :
            string.Empty;
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
                        waybills.Add(reader.MapWaybill(stationId ?? 0));
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, ex.Message);
                    return waybills;
                }
            }
        }
        return waybills;
    }
}
internal static class WaybillMapper
{
    public static Waybill MapWaybill(this IDataRecord record, int stationId)
    {
        var waybill = new Waybill(record.MapOriginCargoCustomer(), record.MapDestinationCargoCustomer())
        {
            Id = record.GetInt("Id"),
            OwnerStationId = stationId,
            Quantity = record.GetInt("Quantity"),
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
        return waybill;
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
            CargoName = record.GetString(record.GetString("OriginLanguages", "EN").FirstItem("EN")),
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
            CargoName = record.GetString(record.GetString("DestinationLanguages", "EN").FirstItem("EN")),
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
}

internal static class SqlHelpers
{
    public static string OrNull(this int? value) =>
        value.HasValue ? value.Value.ToString() :
        "NULL";
}


