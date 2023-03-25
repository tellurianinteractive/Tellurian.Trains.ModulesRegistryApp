namespace ModulesRegistry.Services.Extensions;

public static class StationCustomerWaybillExtensions
{
    public static string CargoName(this StationCustomerWaybill? waybill) =>
        waybill is null ? string.Empty :
        waybill.StationCustomerCargo.Cargo.Localized() ??
        Resources.Strings.None;

    public static string OtherCustomerName(this StationCustomerWaybill? waybill) =>
        waybill is null ? string.Empty :
        waybill.OtherStationCustomerCargo?.StationCustomer?.CustomerName ??
        Resources.Strings.NotApplicable;

    public static string OtherStationName(this StationCustomerWaybill? waybill) =>
        waybill is null ? string.Empty :
        waybill.OtherStationCustomerCargo?.StationCustomer?.Station.FullName ?? 
        waybill.OtherExternalCustomerCargo?.ExternalStationCustomer?.ExternalStation.FullName ??
        waybill.OtherRegion.RepresentativeExternalStation.FullName;

    public static string OtherRegionName(this StationCustomerWaybill? waybill) =>
        waybill?.OtherRegion is null ? string.Empty :
        waybill.OtherRegion.LocalName;

    public static string OperationDayShortName(this StationCustomerWaybill? waybill) =>
        waybill is null || waybill.OperatingDay is null ? string.Empty :
        Resources.Strings.ResourceManager.GetString(waybill.OperatingDay.ShortName) ?? waybill.OperatingDay.ShortName;

    public static bool IsGenerated(this StationCustomerWaybill waybill) =>
        !waybill.IsManuallyCreated || waybill.OtherStationCustomerCargoId.HasValue;
}
