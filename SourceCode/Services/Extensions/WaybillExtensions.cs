using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Extensions;
public static class WaybillExtensions
{
    public static string DestinationQuantityUnit(this Waybill waybill) =>
         waybill is null ? string.Empty :
         waybill.QuantityUnitId == 3 && waybill.Quantity > 0 ? $"{waybill.Quantity} {waybill.PackagingUnit.ToString().AsLocalized()}" :
         waybill.Destination?.QuantityUnitName ?? string.Empty;

}
