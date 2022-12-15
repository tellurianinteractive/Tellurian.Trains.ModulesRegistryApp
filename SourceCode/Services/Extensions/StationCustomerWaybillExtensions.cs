using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Extensions;

public static class StationCustomerWaybillExtensions
{
    public static string CargoName(this StationCustomerWaybill? it) =>
        it is null ? string.Empty :
        it.StationCustomerCargo.Cargo.Localized() ??
        Resources.Strings.None;

    public static string OtherCustomerName(this StationCustomerWaybill? it) =>
        it is null ? string.Empty :
        it.OtherStationCustomerCargo?.StationCustomer?.CustomerName ??
        Resources.Strings.NotApplicable;

    public static string OtherStationName(this StationCustomerWaybill? it) =>
        it is null ? string.Empty :
        it.OtherStationCustomerCargo?.StationCustomer?.Station.FullName ?? 
        it.OtherRegion.RepresentativeExternalStation.FullName;

    public static string OtherRegionName(this StationCustomerWaybill? it) =>
        it?.OtherRegion is null ? string.Empty :
        it.OtherRegion.LocalName;

    public static string OperationDayShortName(this StationCustomerWaybill? it) =>
        it is null || it.OperatingDay is null ? string.Empty :
        Resources.Strings.ResourceManager.GetString(it.OperatingDay.ShortName) ?? it.OperatingDay.ShortName;

    public static bool IsGenerated(this StationCustomerWaybill it) =>
        !it.IsManuallyCreated || it.OtherStationCustomerCargoId.HasValue;
}
