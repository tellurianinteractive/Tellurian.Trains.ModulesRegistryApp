using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static class WaybillExtensions
{
    public static IEnumerable<Waybill> AsPrintableWaybills(this IEnumerable<Waybill> items)
    {
        var result = new List<Waybill>();
        // All waybills that prints double items prints first to place them evenly on a page.
        foreach (var item in items.OrderByDescending(i => i.HasEmptyReturn))
        {
            item.OperatingDayFlag = item.WaybillSendingDaysFlags();
            for (var i = 0; i < item.PrintCount; i++)
            {
                if (item.PrintPerOperatingDay)
                {
                    var operationDays = (byte)(item.Destination!.OperationDaysFlags & item.Origin!.OperationDaysFlags);
                    if (operationDays > 0)
                    {
                        foreach (var day in item.OperatingDayFlag.GetDays(false, true))
                        {
                            var clone = item.Clone;
                            item.OperatingDayFlag = day.Flag;

                            result.Add(clone);
                            if (clone.HasEmptyReturn) result.Add(clone.EmptyReturn());
                        }
                    }
                }
                else
                {
                    result.Add(item);
                    if (item.HasEmptyReturn) result.Add(item.EmptyReturn());
                }
            }
        }
        return result;
    }

    private static byte WaybillSendingDaysFlags(this Waybill it) =>
        ((byte)(it.Destination!.OperationDaysFlags & it.Origin!.OperationDaysFlags));
    private static Waybill EmptyReturn(this Waybill waybill)
    {
        var emptyReturn = waybill.Clone;
        var newOrigin = emptyReturn.Destination;
        var newDestination = emptyReturn.Origin;
        emptyReturn.OwnerStationId = 0;
        emptyReturn.Origin = newOrigin;
        emptyReturn.Origin!.CargoName = LanguageUtility.GetLocalizedString("Empty", emptyReturn.Origin.Language());
        emptyReturn.Origin!.SpecialCargoName = string.Empty;
        emptyReturn.Origin!.ReadyTimeResourceKey = string.Empty;
        emptyReturn.Origin!.TrackOrArea = string.Empty;
        emptyReturn.Quantity = 0;
        emptyReturn.Destination = newDestination;
        //emptyReturn.Destination!.OperationDaysFlags = waybill.Origin!.OperationDaysFlags;
        emptyReturn.Destination!.CargoName = LanguageUtility.GetLocalizedString("Empty", emptyReturn.Destination.Language());
        emptyReturn.Destination!.SpecialCargoName = string.Empty;
        emptyReturn.Destination!.ReadyTimeResourceKey = string.Empty;
        emptyReturn.IsEmptyReturn = true;
        return emptyReturn;
    }

    public static string DestinationQuantityUnit(this Waybill waybill) =>
        waybill is null || waybill.Destination is null ? string.Empty :
        waybill.QuantityUnitId == 3 && waybill.Quantity > 0 ? $"{waybill.Quantity} {waybill.Destination.PackagingUnit()}" :
        waybill.Destination.QuantityUnit() ?? string.Empty;

    public static bool IsCrossBorder([NotNullWhen(true)] this Waybill? me) =>
        me is not null &&
        me.Origin is not null &&
        me.Destination is not null &&
        !me.Origin.DomainSuffix.Equals(me.Destination?.DomainSuffix, StringComparison.OrdinalIgnoreCase);

    public static bool HasDifferentCargoTranslations(this Waybill? me) =>
        me is not null && !me.Origin.CargoName().Equals(me.Destination.CargoName());

    public static string SendingDays(this Waybill? me) =>
       me is not null && me.PrintPerOperatingDay ?
        me.IsEmptyReturn ? string.Format(Resources.Strings.SendDays, LanguageUtility.GetLocalizedString("WhenNeeded", me.Origin!.Language()).ToLowerInvariant()) :
        string.Format(Resources.Strings.SendDays, LanguageUtility.GetLocalizedString(me.OperatingDayFlag.OperationDays().FullName, me.Origin!.Language())) : string.Empty;

    public static string LoadingReady(this Waybill? me) =>
        me is null || me.HideLoadingTimes ? string.Empty :
        me.Origin.LoadingReady();

    public static string UnloadingReady(this Waybill? me) =>
    me is null || me.HideUnloadingTimes ? string.Empty :
    me.Destination.UnloadingReady();
}

public static class CargoCustomerExtensions
{
    public static string CargoName(this CargoCustomer? me) =>
         me is null ? string.Empty :
         me.PackagingUnitResourceKey.HasValue() && me.PackagingUnitResourceKey != "NotApplicable" ?
             $"{me.CargoName.GetLocalizedString(me.Language())} {"In".GetLocalizedString(me.Language())} {me.PackagingUnitResourceKey.GetLocalizedString(me.Language()).ToLowerInvariant()}" :
             me.CargoName.GetLocalizedString(me.Language());

    public static string CustomerName(this CargoCustomer? me) =>
        me is null ? string.Empty :
        me.Name == StationCustomer.ImportingCustomerResourceName ||
        me.Name == StationCustomer.ExportingCustomerResourceName ? me.Name.GetLocalizedString(me.Language()) :
        me.Name;

    public static string FlagSrc(this CargoCustomer? me) =>
        me is null ? string.Empty :
        $"images/flags/{me.DomainSuffix}.png";

    public static bool IsInternal(this CargoCustomer? me, Waybill waybill) =>
        me is not null && me.IsModuleStation && me.StationId == waybill.OwnerStationId;

    internal static string Language(this CargoCustomer? me)
    {
        if (me is null) return LanguageUtility.DefaultLanguage;
        if (me.Languages.Length == 2) return me.Languages;
        var languages = me.Languages.Split(',');
        return languages.Length > 0 ? languages[0] : LanguageUtility.DefaultLanguage;
    }

    public static bool HasLimitedYearsInOperation([NotNullWhen(true)] this CargoCustomer? me) =>
         me is not null && (me.FromYear.HasValue || me.UptoYear.HasValue);

    public static string OperationShortDays(this CargoCustomer? me) =>
        me is null || me.OperationDaysFlags == 0 ? string.Empty :
        me.OperationDaysFlags.OperationDays(me.Language().AsCultureInfo()).ShortName;

    public static string YearsInOperation(this CargoCustomer? me) =>
        me.HasLimitedYearsInOperation() ? $"{me.FromYear}-{me.UptoYear}" : string.Empty;

    public static string DualLanguageLabel(this CargoCustomer it, string resourceKey, string? otherEnglishText = null)
    {
        var resourceManager = Resources.Strings.ResourceManager;
        var culture = new CultureInfo(it.Languages);
        var localizedText = resourceManager.GetString(resourceKey, culture);
        var englishText = otherEnglishText ?? resourceKey;
        if (localizedText.HasValue() && englishText != localizedText) return $"{englishText}/{localizedText}";
        return englishText;
    }

    public static string ForeColor(this CargoCustomer? me, Waybill waybill) =>
        me is null || me.IsInternal(waybill) ? "#000000" : me.ForeColor;

    public static string BackColor(this CargoCustomer? me, Waybill waybill) =>
         me is null || me.IsInternal(waybill) ? "#FFFFFF" : me.BackColor;


    internal static string QuantityUnit(this CargoCustomer me) =>
        me.QuantityUnitResourceKey.GetLocalizedString(me.Language());

    internal static string PackagingUnit(this CargoCustomer me) =>
         me.PackagingUnitResourceKey.GetLocalizedString(me.Language());



    internal static string LoadingReady(this CargoCustomer? me) =>
        me.HasReadyTime() ?
        me.TrackOrArea.HasValue() ? $"{"LoadingReadyTime".GetLocalizedString(me.Languages)} {me.ReadyTime()} " :
        $"{"Load".GetLocalizedString(me.Languages)} " :
        string.Empty;

    internal static string UnloadingReady(this CargoCustomer? me) =>
        me.HasReadyTime() ?
        me.TrackOrArea.HasValue() ? $"{"UnloadingReadyTime".GetLocalizedString(me.Languages)} {me.ReadyTime()} " :
        $"{"Unload".GetLocalizedString(me.Languages)} " :
        string.Empty;

    private static string ReadyTime(this CargoCustomer me) =>
        me.ReadyTimeResourceKey.GetLocalizedString(me.Languages).ToLowerInvariant();

    public static bool HasReadyTime([NotNullWhen(true)] this CargoCustomer? me) =>
        me is not null && me.DisplayReadyTime && me.ReadyTimeResourceKey.HasValue() && me.ReadyTimeResourceKey != "n/a";

    public static string TrackOrAreaColor(this CargoCustomer? me) =>
        me is null || me.TrackOrAreaColor.HasNoValue() || me.TrackOrAreaColor.Equals("#ffffff", StringComparison.OrdinalIgnoreCase) ? "#fffff0" :
        me.TrackOrAreaColor;

}
