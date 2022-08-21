namespace ModulesRegistry.Services.Extensions;

public static class StationCustomerWaybillExtensions
{
    public static string RegionName(this StationCustomerWaybill? it) =>
        it is null || it.OtherRegion is null ? string.Empty :
        it.OtherRegion.LocalName;

    public static string OperationDayShortName(this StationCustomerWaybill? it) =>
        it is null || it.OperatingDay is null ? string.Empty :
        Resources.Strings.ResourceManager.GetString(it.OperatingDay.ShortName) ?? it.OperatingDay.ShortName;

    public static bool IsGenerated(this StationCustomerWaybill it) =>
        !it.IsManuallyCreated || it.OtherCustomerCargoId.HasValue;
}
