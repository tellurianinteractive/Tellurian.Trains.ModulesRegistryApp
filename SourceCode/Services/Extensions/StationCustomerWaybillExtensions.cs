using Microsoft.Extensions.Localization;
using ModulesRegistry.Services.Implementations;

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
        waybill.OtherExternalCustomerCargo?.ExternalStationCustomer?.CustomerName ??
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

    public static string HasEmptyReturn(this StationCustomerWaybill? waybill) =>
        waybill is null ? string.Empty :
        waybill.StationCustomerCargo.QuantityUnit.IsBearer ? waybill.HasEmptyReturn.AsYesOrNo() :
        Resources.Strings.NotApplicable;

    public static string QuantityAndUnit(this StationCustomerWaybill? waybill) =>
        waybill is null ? string.Empty :
         $"{waybill.StationCustomerCargo.Quantity} {waybill.StationCustomerCargo.UnitOfCargo().ToLowerInvariant()}";

    private static string UnitOfCargo(this StationCustomerCargo cargo) =>
        cargo.QuantityUnit.SingularResourceCode.HasValue("Piece") ? cargo.PackageUnitName() :
        cargo.QuantityUnit.Designation.HasValue() ? cargo.QuantityUnit.Designation :
        cargo.QuantityUnitName();

    private static string PackageUnitName(this StationCustomerCargo cargo) =>
        cargo.Quantity > 1 ? cargo.PackageUnit.PluralResourceCode.Localized() :
        cargo.PackageUnit.SingularResourceCode.Localized();
    private static string QuantityUnitName(this StationCustomerCargo cargo) =>
        cargo.Quantity > 1 ? cargo.QuantityUnit.PluralResourceCode.Localized() :
        cargo.QuantityUnit.SingularResourceCode.Localized();
}
