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

    public async Task<IEnumerable<Waybill>?> GetWaybills(ClaimsPrincipal? principal, int layoutId, int? stationId, bool matchShadowYards = false)
    {
        List<Waybill> waybills = new List<Waybill>(200);
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            var packagingUnits = await dbContext.CargoPackagingUnits.AsNoTracking().ToListAsync().ConfigureAwait(false);
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
                        waybills.Add(MapWaybill(reader, resourceManager, packagingUnits));
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
                                empty.Destination.CargoName = resourceManager.GetString("Empty",language) ?? string.Empty;
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

    private static Waybill MapWaybill(IDataRecord record, ResourceManager resourceManager, IEnumerable<CargoPackagingUnit> packagingUnits)
    {
        string originLanguage = record.GetString("OriginLanguages", "EN").FirstItem("EN");
        string destinationLanguage = record.GetString("DestinationLanguages", "EN").FirstItem("EN");
        var wagonClass = record.GetString("DefaultClasses");
        var specialWagonClass = record.GetString("SpecificWagonClass");
        return new()
        {
            Origin = new CargoCustomer
            {
                Name = record.GetString("Sender"),
                StationName = record.GetString("OriginStationName"),
                Language = originLanguage,
                DomainSuffix = record.GetString("OriginDomainSuffix"),
                ForeColor = record.GetString("OriginForeColor"),
                BackColor = record.GetString("OriginBackColor"),
                CargoName = record.GetString(originLanguage),
                PackageUnitName = PackageUnitName(packagingUnits, record.GetInt("OriginPackageUnitId"), originLanguage),
                QuantityUnitName = record.GetStringResourceForLanguage("QuanityUnitResourceName", resourceManager, originLanguage),
                IsInternal = record.GetBool("OriginIsInternal"),
                OperationDaysFlags = record.GetByte("SendingDayFlag"),
                ReadyTime = record.GetString("OriginReadyTime"),
                ReadyTimeIsSpecifiedInLayout = record.GetBool("OriginReadyTimeIsSpecifiedInLayout"),
                TrackOrArea = record.GetString("SenderTrackOrArea"),
                TrackOrAreaColor = record.GetString("SenderTrackOrAreaColor")
            },
            Destination = new CargoCustomer
            {
                Name = record.GetString("Receiver"),
                StationName = record.GetString("DesinationStationName"),
                Language = destinationLanguage,
                DomainSuffix = record.GetString("DestinationDomainSuffix"),
                ForeColor = record.GetString("DestinationForeColor"),
                BackColor = record.GetString("DestinationBackColor"),
                CargoName = record.GetString(destinationLanguage),
                PackageUnitName = PackageUnitName(packagingUnits, record.GetInt("DestinationPackageUnitId"), destinationLanguage),
                QuantityUnitName = record.GetStringResourceForLanguage("QuanityUnitResourceName", resourceManager, destinationLanguage),
                IsInternal = record.GetBool("DestinationIsInternal"),
                OperationDaysFlags = record.GetByte("ReceivingDayFlag"),
                ReadyTime = record.GetString("DestinationReadyTime"),
                ReadyTimeIsSpecifiedInLayout = record.GetBool("DestinationReadyTimeIsSpecifiedInLayout"),
                TrackOrArea = record.GetString("ReceiverTrackOrArea"),
                TrackOrAreaColor = record.GetString("ReceiverTrackOrAreaColor")

            },
            Quantity = record.GetInt("Quantity"),
            QuantityUnitId = record.GetInt("QuantityUnitId"),
            PackagingUnitId = record.GetInt("OriginPackageUnitId"),
            OperatorName = string.Empty, // To be supported
            WagonClass = string.IsNullOrWhiteSpace(specialWagonClass) ? wagonClass : specialWagonClass,
            EmptyReturn = record.GetBool("EmptyReturn"),
            MatchReturn = record.GetBool("MatchReturn")
        };
    }

    static string PackageUnitName(IEnumerable<CargoPackagingUnit>? items, int id, string language) =>
        items is null || id == 0 ? string.Empty :
        items.SingleOrDefault(i => i.Id == id)?.PluralResourceCode.LocalizedName(language.AsCultureInfo()).Value ?? string.Empty;
}
