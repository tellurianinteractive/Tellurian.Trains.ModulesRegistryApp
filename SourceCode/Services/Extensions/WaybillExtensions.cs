using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static class WaybillExtensions
{
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

    internal static string Language(this CargoCustomer me)
    {
        if (me.Languages.Length == 2) return me.Languages;
        var languages = me.Languages.Split(',');
        return languages.Length > 0 ? languages[0] : LanguageUtility.DefaultLanguage;
    }

    public static bool HasLimitedYearsInOperation([NotNullWhen(true)] this CargoCustomer? me) =>
         me is not null && (me.FromYear.HasValue || me.UptoYear.HasValue);

    public static string OperationDays(this CargoCustomer? me) =>
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
         me is null || me.IsInternal(waybill) ? "#FFFFFF" : me.ForeColor;


    internal static string QuantityUnit(this CargoCustomer me) =>
        me.QuantityUnitResourceKey.GetLocalizedString(me.Language());

    internal static string PackagingUnit(this CargoCustomer me) =>
         me.PackagingUnitResourceKey.GetLocalizedString(me.Language());

    public static string LoadingInstructions(this CargoCustomer it) =>
        it.ReadyTimeResourceKey.HasValue() ? "" : "";

    public static string LoadingReady(this CargoCustomer? me) =>
        me.HasReadyTime() ?
        me.TrackOrArea.HasValue() ? $"{"LoadingReadyTime".GetLocalizedString(me.Languages)} {me.ReadyTime()} " :
        $"{"Load".GetLocalizedString(me.Languages)} " :
        string.Empty;

    public static string UnloadingReady(this CargoCustomer? me) =>
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
