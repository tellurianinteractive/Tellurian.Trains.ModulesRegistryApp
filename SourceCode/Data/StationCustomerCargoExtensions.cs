#nullable disable

using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public static class StationCustomerCargoExtensions
{
    public static bool IsUnloading(this StationCustomerCargo me) => me.DirectionId == 1 || me.DirectionId == 3;
    public static bool IsLoading(this StationCustomerCargo me) => me.DirectionId == 2 || me.DirectionId == 4;

    public static StationCustomerCargo Clone(this StationCustomerCargo me) =>
        new()
        {
            CargoId = me.CargoId,
            DirectionId = me.DirectionId,
            OperatingDayId = me.OperatingDayId,
            PackageUnitId = me.PackageUnitId,
            FromYear = me.FromYear,
            UptoYear = me.UptoYear,
            QuantityUnitId = me.QuantityUnitId,
            Quantity = me.Quantity,
            ReadyTimeId = me.ReadyTimeId,
            MaxTrainsetLength = me.MaxTrainsetLength,
            SpecialCargoName = me.SpecialCargoName,
            SpecificWagonClass = me.SpecificWagonClass,
            StationCustomerId = me.StationCustomerId,
            TrackOrArea = me.TrackOrArea,
            TrackOrAreaColor = me.TrackOrAreaColor
        };
}
