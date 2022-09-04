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
                    CommandTimeout=120
                };
                command.Parameters.AddWithValue("@StationId", stationId);
                command.Parameters.AddWithValue("StationCustomerId", stationCustomerId);
                command.Parameters.AddWithValue("@Sending", sending);
                command.Parameters.AddWithValue("@Receiving", receiving);
                try
                {
                    connection.Open( );
                    var reader = await command.ExecuteReaderAsync();
                    var resourceManager = new ResourceManager(typeof(Resources.Strings));
                    while (await reader.ReadAsync())
                    {
                        waybills.Add(MapWaybill(reader, resourceManager,  stationId));
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
                                WagonClass = last.WagonClass,
                            };
                            var language = new CultureInfo(last.Origin?.Language ?? "en");
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
                        waybills.Add(MapWaybill(reader, resourceManager,  stationId ?? 0));
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
                                WagonClass = last.WagonClass,
                            };
                            var language = new CultureInfo(last.Origin?.Language ?? "en");
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

    private static Waybill MapWaybill(IDataRecord record, ResourceManager resourceManager, int stationId)
    {
        string originLanguage = record.GetString("OriginLanguages", "EN").FirstItem("EN");
        string destinationLanguage = record.GetString("DestinationLanguages", "EN").FirstItem("EN");
        var wagonClass = record.GetString("DefaultClasses");
        var specialWagonClass = record.GetString("SpecificWagonClass");
        var originIsInternal = record.GetBool("OriginIsModuleStation") && record.GetInt("OriginStationId") == stationId;
        var destinationIsInternal = record.GetBool("DestinationIsModuleStation") && record.GetInt("DestinationStationId") == stationId;
        return new()
        {
            Origin = new CargoCustomer
            {
                StationName = record.GetString("OriginStationName"),
                Name = record.GetString("SenderName"),
                Language = originLanguage,
                DomainSuffix = record.GetString("OriginDomainSuffix"),
                ForeColor = originIsInternal ? "#000000" : record.GetString("OriginForeColor"),
                BackColor = originIsInternal ? "#FFFFFF" : record.GetString("OriginBackColor"),
                CargoName = record.GetString(originLanguage),
                PackageUnitName = record.GetLocalizedString("PackagingUnitResourceName", resourceManager, originLanguage),
                QuantityUnitName = record.GetLocalizedString("QuanityUnitResourceName", resourceManager, originLanguage),
                IsInternal = originIsInternal,
                OperationDaysFlags = record.GetByte("SendingDayFlag"),
                ReadyTime = record.GetLocalizedString("SenderReadyTime", resourceManager, originLanguage),
                TrackOrArea = record.GetString("SenderTrackOrArea"),
                TrackOrAreaColor = record.GetString("SenderTrackOrAreaColor"),
                FromYear = record.GetNullableInt("SenderFromYear", null),
                UptoYear = record.GetNullableInt("SenderUptoYear", null)
            },
            Destination = new CargoCustomer
            {
                Name = record.GetString("ReceiverName"),
                StationName = record.GetString("DestinationStationName"),
                Language = destinationLanguage,
                DomainSuffix = record.GetString("DestinationDomainSuffix"),
                ForeColor = destinationIsInternal ? "#000000" : record.GetString("DestinationForeColor"),
                BackColor = destinationIsInternal ? "#FFFFFF" : record.GetString("DestinationBackColor"),
                CargoName = record.GetString(destinationLanguage),
                PackageUnitName = record.GetLocalizedString("PackagingUnitResourceName", resourceManager, destinationLanguage),
                QuantityUnitName = record.GetLocalizedString("QuanityUnitResourceName", resourceManager, destinationLanguage),
                IsInternal = destinationIsInternal,
                OperationDaysFlags = record.GetByte("ReceivingDayFlag"),
                ReadyTime = record.GetLocalizedString("ReceiverReadyTime", resourceManager, destinationLanguage),
                TrackOrArea = record.GetString("ReceiverTrackOrArea"),
                TrackOrAreaColor = record.GetString("ReceiverTrackOrAreaColor"),
                FromYear = record.GetNullableInt("ReceiverFromYear", null),
                UptoYear = record.GetNullableInt("ReceiverUptoYear", null)
            },
            Quantity = record.GetInt("Quantity"),
            OperatorName = string.Empty, // To be supported
            WagonClass = string.IsNullOrWhiteSpace(specialWagonClass) ? wagonClass : specialWagonClass,
            Id = record.GetInt("Id"),
            //EmptyReturn = record.GetBool("EmptyReturn"),
            //MatchReturn = record.GetBool("MatchReturn")
        };
    }
}
