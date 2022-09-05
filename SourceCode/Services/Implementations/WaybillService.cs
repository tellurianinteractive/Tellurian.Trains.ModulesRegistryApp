using Microsoft.Data.SqlClient;
using System.Data;
using System.Resources;

namespace ModulesRegistry.Services.Implementations;

public class WaybillService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public WaybillService(IDbContextFactory<ModulesDbContext> factory)
    {
        Factory = factory;
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
                        var last = waybills.Last();
                        if (last is null) continue;
                        if (last.EmptyReturn)
                        {
                            var empty = new Waybill
                            {
                                Destination = last.Origin,
                                Epoch = last.Epoch,
                                OperatorName = last.OperatorName,
                                Origin = last.Destination,
                                Quantity = 0,
                                DefaultWagonClass = last.DefaultWagonClass,
                            };
                            var language = new CultureInfo(last.Origin?.Languages ?? "en");
                            if (language is not null && empty.Destination is not null)
                                empty.Destination.CargoName = resourceManager.GetString("Empty", language) ?? string.Empty;
                            waybills.Add(empty);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        return waybills;

    }

    public async Task<IEnumerable<Waybill>?> GetWaybills(ClaimsPrincipal? principal, int layoutId, int? stationId, bool matchShadowYards = false)
    {
        List<Waybill> waybills = new List<Waybill>(200);
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            using var connection = dbContext.Database.GetDbConnection() as SqlConnection;
            if (connection is not null)
            {
                var command = new SqlCommand("GetWaybills", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@LayoutId", layoutId);
                if (stationId.HasValue)
                {
                    command.Parameters.AddWithValue("@StationId", stationId);
                    command.Parameters.AddWithValue("@Receiving", true);
                    command.Parameters.AddWithValue("@Sending", matchShadowYards);
                }
                command.Parameters.AddWithValue("@MatchShadowYard", matchShadowYards);
                try
                {
                    connection.Open();
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));
                    while (await reader.ReadAsync())
                    {
                        waybills.Add(reader.MapWaybill(stationId ?? 0));
                        var last = waybills.Last();
                        if (last is null) continue;
                        if (last.EmptyReturn)
                        {
                            var empty = new Waybill
                            {
                                Destination = last.Origin,
                                Epoch = last.Epoch,
                                OperatorName = last.OperatorName,
                                Origin = last.Destination,
                                Quantity = 0,
                                DefaultWagonClass = last.DefaultWagonClass,
                            };
                            var language = new CultureInfo(last.Origin?.Languages ?? "en");
                            if (language is not null && empty.Destination is not null)
                                empty.Destination.CargoName = resourceManager.GetString("Empty", language) ?? string.Empty;
                            waybills.Add(empty);
                        }
                    }
                }
                catch (Exception)
                {
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
        string OriginLanguageColumnName = record.GetString("OriginLanguages", "EN").FirstItem("EN");
        string DestinationLanguageColumnName = record.GetString("DestinationLanguages", "EN").FirstItem("EN");
        return new()
        {
            Id = record.GetInt("Id"),
            OwnerStationId = stationId,
            Origin = new()
            {
                Name = record.GetString("SenderName"),
                StationId = record.GetInt("OriginStationId"),
                StationName = record.GetString("OriginStationName"),
                Languages = record.GetString("OriginLanguages", "en"),
                DomainSuffix = record.GetString("OriginDomainSuffix"),
                ForeColor = record.GetString("OriginForeColor"),
                BackColor = record.GetString("OriginBackColor"),
                CargoName = record.GetString(OriginLanguageColumnName),
                PackagingUnitResourceKey = record.GetString("PackagingUnitResourceName"),
                QuantityUnitResourceKey = record.GetString("QuanityUnitResourceName"),
                IsModuleStation = record.GetBool("OriginIsModuleStation"),
                OperationDaysFlags = record.GetByte("SendingDayFlag"),
                ReadyTimeResourceKey = record.GetString("SenderReadyTime"),
                TrackOrArea = record.GetString("SenderTrackOrArea"),
                TrackOrAreaColor = record.GetString("SenderTrackOrAreaColor"),
                FromYear = record.GetNullableInt("SenderFromYear", null),
                UptoYear = record.GetNullableInt("SenderUptoYear", null)
            },
            Destination = new()
            {
                Name = record.GetString("ReceiverName"),
                StationId = record.GetInt("DestinationStationId"),
                StationName = record.GetString("DestinationStationName"),
                Languages = record.GetString("DestinationLanguages", "en"),
                DomainSuffix = record.GetString("DestinationDomainSuffix"),
                ForeColor = record.GetString("DestinationForeColor"),
                BackColor = record.GetString("DestinationBackColor"),
                CargoName = record.GetString(DestinationLanguageColumnName),
                PackagingUnitResourceKey = record.GetString("PackagingUnitResourceName"),
                QuantityUnitResourceKey = record.GetString("QuanityUnitResourceName"),
                IsModuleStation = record.GetBool("DestinationIsModuleStation"),
                OperationDaysFlags = record.GetByte("ReceivingDayFlag"),
                ReadyTimeResourceKey = record.GetString("ReceiverReadyTime"),
                TrackOrArea = record.GetString("ReceiverTrackOrArea"),
                TrackOrAreaColor = record.GetString("ReceiverTrackOrAreaColor"),
                FromYear = record.GetNullableInt("ReceiverFromYear", null),
                UptoYear = record.GetNullableInt("ReceiverUptoYear", null)
            },
            Quantity = record.GetInt("Quantity"),
            OperatorName = string.Empty, // To be supported
            DefaultWagonClass = record.GetString("DefaultClasses"),
            SpecialWagonClass = record.GetString("SpecificWagonClass"),
            //EmptyReturn = record.GetBool("EmptyReturn"),
            //MatchReturn = record.GetBool("MatchReturn")
        };
    }
}


