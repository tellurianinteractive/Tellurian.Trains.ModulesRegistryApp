using ModulesRegistry.Resources;
using ModulesRegistry.Services.Implementations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static class WaybillExtensions
{
    public static IEnumerable<Waybill> AsPrintableWaybills(this IEnumerable<Waybill> waybills)
    {
        var result = new List<Waybill>();
       waybills = waybills.ApplyAdditionalRules();

        // All waybills that prints double items prints first to place them evenly on a page.
        foreach (var waybill in waybills.OrderByDescending(i => i.HasEmptyReturn).ThenBy(i => i.Destination.StationName))
        {
            for (var i = 0; i < waybill.PrintCount; i++)
            {
                if (waybill.PrintPerOperatingDay)
                {
                    if (waybill.OperatingDayFlag > 0)
                    {
                        foreach (var day in waybill.OperatingDayFlag.GetDays(false, true))
                        {
                            var clone = waybill.Clone;
                            clone.OperatingDayFlag = day.Flag;

                            result.Add(clone);
                            if (clone.HasReturn()) result.Add(clone.Return());
                        }
                    }
                }
                if (waybill.QuantityUnitId==3)
                {
                    var printCount = waybill.Quantity;
                    waybill.Quantity = 1;
                    for(var j = 0; j < printCount; j++)
                    {
                        var clone = waybill.Clone;
                        result.Add(clone);
                        if (clone.HasReturn()) result.Add(clone.Return());
                    }
                }
                else
                {
                    result.Add(waybill);
                    if (waybill.HasReturn()) result.Add(waybill.Return());
                }
            }
        }

        return result;
    }

    private static IEnumerable<Waybill> ApplyAdditionalRules(this IEnumerable<Waybill> items)
    {
        foreach (var item in items)
        {
            // A stored waybill has its own days (station waybills); the others are sent on the sender's days.
            if (item.OperatingDayFlag == 0) item.OperatingDayFlag = item.Origin.OperationDaysFlags;
            if (item.QuantityIsBearer)
            {
                item.PrintCount = item.Quantity * item.PrintCount;
                item.Quantity = 1;
            }
            else
            {
                item.HasEmptyReturn = false;
            }
        }
        return items;
    }

    /// <summary>
    /// The days a waybill is sent: the sender's days. A return with the same cargo is sent on its own sender's days
    /// (the receiver of the outgoing waybill), an empty return daily.
    /// </summary>
    private static Waybill Return(this Waybill waybill)
    {
        var returnWaybill = waybill.Clone;
        returnWaybill.OperatingDayFlag = waybill.HasSameCargoReturn ? waybill.Destination.OperationDaysFlags : OperationDaysExtensions.DailyFlag;
        var newOrigin = returnWaybill.Destination;
        var newDestination = returnWaybill.Origin;
        returnWaybill.Origin = newOrigin;
        returnWaybill.Origin!.IsOrigin = true;

        returnWaybill.Origin!.CargoName = waybill.HasSameCargoReturn ? newOrigin.CargoName :
            LanguageExtensions.GetLocalizedString("Empty", returnWaybill.Origin.Language());

        if (returnWaybill.HasEmptyReturn)
        {
            returnWaybill.Origin!.SpecialCargoName = string.Empty;
            returnWaybill.Origin!.ReadyTimeResourceKey = string.Empty;
            returnWaybill.Origin!.TrackOrArea = string.Empty;

        }
        returnWaybill.Destination = newDestination;
        returnWaybill.Destination!.IsOrigin = false;
        returnWaybill.Destination!.CargoName = waybill.HasSameCargoReturn ? newDestination.CargoName :
            LanguageExtensions.GetLocalizedString("Empty", returnWaybill.Destination.Language());

        if (returnWaybill.HasEmptyReturn)
        {
            returnWaybill.Destination!.SpecialCargoName = string.Empty;
            returnWaybill.Destination!.ReadyTimeResourceKey = string.Empty;
            returnWaybill.Destination!.PackagingUnitResourceKey = string.Empty;
            returnWaybill.IsEmptyReturn = true;
        }
        return returnWaybill;
    }

    public static string DestinationQuantity(this Waybill waybill) =>
        waybill is null || waybill.Destination is null ? string.Empty :
        waybill.IsEmptyReturn && waybill.Origin!.QuantityUnitResourceKey.IsAnyOf("Wagon", "Wagons") ? waybill.DestinationWagonQuantity() :
        waybill.Destination.QuantityUnitResourceKey.IsAnyOf("Wagon", "Wagons") ? waybill.DestinationWagonQuantity() :
        waybill.Destination.QuantityUnitResourceKey.IsAnyOf("Piece", "Pieces") ? $"{waybill.Quantity} " :
        $"{waybill.Quantity} {waybill.Destination.QuantityUnit()}";

    public static string OriginQuantity(this Waybill waybill) =>
         waybill is null || waybill.Destination is null ? string.Empty :
         waybill.IsEmptyReturn && waybill.Origin!.QuantityUnitResourceKey.StartsWith("Wagon") ? waybill.OriginWagonQuantity() :
         waybill.Destination.QuantityUnitResourceKey.StartsWith("Wagon") ? waybill.OriginWagonQuantity() :
         waybill.QuantityUnitId == 3 && waybill.Quantity > 0 ? $"{waybill.Quantity} {waybill.Destination.PackagingUnit(waybill.Origin.Language())}" :
         $"{waybill.Quantity} {waybill.Destination.QuantityUnit(waybill.Origin.Language())}";



    private static string DestinationWagonQuantity(this Waybill waybill) =>
        waybill.Quantity > 1 ?
            $"{LanguageExtensions.GetLocalizedString("TrainsetWith", waybill.Destination.Language())} {waybill.Quantity} {waybill.Destination.QuantityUnit()}" : // NOTE: Temporary fix until wagons and trainset are redefined.
            $"{waybill.Quantity} {waybill.Destination.QuantityUnit()}";
    private static string OriginWagonQuantity(this Waybill waybill)
    {
        var language = waybill.Origin.Language();
        return waybill.Quantity > 1 ?
             $"{LanguageExtensions.GetLocalizedString("TrainsetWith", language)} {waybill.Quantity} {waybill.Destination.QuantityUnit(language)}" : // NOTE: Temporary fix until wagons and trainset are redefined.
             $"{waybill.Quantity} {waybill.Destination.QuantityUnit(language)}";
    }

    public static bool IsCrossBorder([NotNullWhen(true)] this Waybill? me) =>
        me is not null &&
        me.Origin is not null &&
        me.Destination is not null &&
        !me.Origin.DomainSuffix.Equals(me.Destination?.DomainSuffix, StringComparison.OrdinalIgnoreCase);

    public static bool ShowFlag([NotNullWhen(true)] this Waybill? me) =>
        me.IsCrossBorder();

    public static bool HasDifferentCargoNameTranslations(this Waybill? me) =>
        me is not null && !me.Origin.CargoName().Equals(me.Destination.CargoName());

    public static bool HasDifferentQuantityTranslations(this Waybill? me) =>
        me is not null && !me.Origin.QuantityUnit().Equals(me.Destination.QuantityUnit());

    public static string SendingDays(this Waybill? me)
    {
        if (me is null || me.Origin is null) return string.Empty;
        var culture = new CultureInfo(me.Origin.Language());
        return me.OperatingDayFlag.OperationDays(culture).ShortName;
    }

    public static string LoadingReady(this Waybill? me) =>
        me is null || me.HideLoadingTimes ? string.Empty :
        me.Origin.LoadingReady();

    public static string UnloadingReady(this Waybill? me) =>
    me is null || me.HideUnloadingTimes ? string.Empty :
    me.Destination.UnloadingReady();

    public static string WagonClass(this Waybill? me) =>
        me is null ? string.Empty :
        me.SpecialWagonClass.HasValue ? me.SpecialWagonClass :
        me.DefaultWagonClass;

    public static bool HasReturn(this Waybill? me) => me is not null && (me.HasSameCargoReturn || me.HasEmptyReturn);
}

public static class CargoCustomerExtensions
{
    public static string StationName(this CargoCustomer? me) =>
        me is null ? string.Empty :
        me.Waybill.IsCrossBorder() && me.InternationalStationName.HasValue ? me.InternationalStationName :
        me.StationName;

    public static string CargoName(this CargoCustomer? me) =>
         me is null ? string.Empty :
         me.CargoName.GetLocalizedString(me.Language());

    public static bool HasPackagingUnit([NotNullWhen(true)] this CargoCustomer? me) =>
        me is not null && me.PackagingUnitResourceKey.HasValueExcept("NotApplicable");

    public static string PackagingUnit(this CargoCustomer? me) =>
         me is null || me.PackagingUnitResourceKey.IsAnyOf("NotApplicable") ? string.Empty :
         $"{me.PackagingUnitResourceKey.GetLocalizedString(me.Language())}";

    public static string PackagingUnitWithPreposition(this CargoCustomer? me) =>
         me is null || me.PackagingUnitResourceKey.IsAnyOf("NotApplicable") ? string.Empty :
         me.QuantityUnitResourceKey.IsAnyOf("Piece", "Pieces") ? $"{me.PackagingUnitResourceKey.GetLocalizedString(me.Language()).ToLowerInvariant()}" :
            $"{me.PackagingPrepositionResourceCode.GetLocalizedString(me.Language())} {me.PackagingUnitResourceKey.GetLocalizedString(me.Language()).ToLowerInvariant()}";

    public static string CustomerName(this CargoCustomer? me) =>
        me is null ? string.Empty :
        me.Name == StationCustomer.ImportingCustomerResourceName ||
        me.Name == StationCustomer.ExportingCustomerResourceName ? me.Name.GetLocalizedString(me.Language()) :
        me.Name;

    public static string FlagSrc(this CargoCustomer? me) =>
        me is null ? string.Empty :
        $"images/flags/{me.DomainSuffix}.png";


    internal static string Language(this CargoCustomer? me)
    {
        if (me is null) return LanguageExtensions.DefaultLanguage;
        if (me.Languages.Length == 2) return me.Languages;
        var languages = me.Languages.Split(',');
        return languages.Length > 0 ? languages[0] : LanguageExtensions.DefaultLanguage;
    }

    public static bool HasLimitedYearsInOperation([NotNullWhen(true)] this CargoCustomer? me) =>
         me is not null && (me.FromYear.HasValue || me.UptoYear.HasValue);

    public static string OperationShortDays(this CargoCustomer? me) =>
        me is null || me.OperationDaysFlags == 0 ? string.Empty :
        me.OperationDaysFlags.OperationDays(me.Language().AsCultureInfo()).ShortName;

    public static string YearsInOperation(this CargoCustomer? me) =>
        me.HasLimitedYearsInOperation() ? $"{me.FromYear}-{me.UptoYear}" : string.Empty;

    public static string DualLanguageLabel(this CargoCustomer it, string resourceKey, string? otherEnglishText = null) =>
        it.Languages.DualLanguageText(resourceKey, otherEnglishText);

    public static string DualLanguageText(this string languageTwoLetterISOCode, string resourceKey, string? otherEnglishText = null)
    {
        var culture = new CultureInfo(languageTwoLetterISOCode);
        var localizedText = AppTexts.Find(resourceKey, culture);
        var englishText = otherEnglishText ?? resourceKey;
        if (localizedText.HasValue && englishText != localizedText) return $"{englishText}/{localizedText}";
        return englishText;
    }

    public static string TranslatedText(this string languageTwoLetterISOCode, string resourceKey)
    {
        var culture = new CultureInfo(languageTwoLetterISOCode);
        var localizedText = AppTexts.Find(resourceKey, culture);
        return localizedText ?? resourceKey;
    }

    public static string ForeColor(this CargoCustomer? me) =>
        me is null || me.IsUncolored() ? "#000000" : me.ForeColor;

    public static string BackColor(this CargoCustomer? me) =>
         me is null || me.IsUncolored() ? "#FFFFFF" : me.BackColor;

    private static bool IsUncolored([NotNullWhen(false)] this CargoCustomer? me) =>
        me is null || me.Waybill is null || me.IsInternal();

    public static bool IsInternal(this CargoCustomer? me) =>
        me is not null && me.IsModuleStation && (!me.IsOrigin || me.StationId == me.Waybill.OwnerStationId || me.Waybill.HasEmptyReturn);
    //(me.IsOrigin && waybill.Origin.StationId == waybill.OwnerStationId && waybill.IsEmptyReturn));

    internal static string QuantityUnit(this CargoCustomer me, string? otherLanguage = null) =>
        me.Waybill.QuantityShortUnit.HasValue ? me.Waybill.QuantityShortUnit :
        me.QuantityUnitResourceKey.GetLocalizedString(otherLanguage ?? me.Language());

    internal static string PackagingUnit(this CargoCustomer me, string? otherLanguage = null) =>
         me.PackagingUnitResourceKey.GetLocalizedString(otherLanguage ?? me.Language());



    internal static string LoadingReady(this CargoCustomer? me) =>
        me.HasReadyTime() ?
        $"{"LoadingReadyTime".GetLocalizedString(me.Languages)} {me.ReadyTime()} " :
        string.Empty;

    internal static string UnloadingReady(this CargoCustomer? me) =>
        me.HasReadyTime() ?
        $"{"UnloadingReadyTime".GetLocalizedString(me.Languages)} {me.ReadyTime()} " :
        string.Empty;

    private static string ReadyTime(this CargoCustomer me) =>
        me.ReadyTimeResourceKey.GetLocalizedString(me.Languages).ToLowerInvariant();

    public static bool HasReadyTime([NotNullWhen(true)] this CargoCustomer? me) =>
        me is not null && me.DisplayReadyTime && me.ReadyTimeResourceKey.HasValue && me.ReadyTimeResourceKey != "n/a";

    public static string TrackOrAreaColor(this CargoCustomer? me) =>
        me is null || me.TrackOrAreaColor.IsEmpty || me.TrackOrAreaColor.Equals("#ffffff", StringComparison.OrdinalIgnoreCase) ? "#fffff0" :
        me.TrackOrAreaColor;

    public static string TrackOrAreaBackColor(this CargoCustomer item) =>
    item == null ? string.Empty :
    item.CargoTrackOrAreaColor.IsHexColor && !item.CargoTrackOrAreaColor.IsWhite ? item.CargoTrackOrAreaColor :
    item.TrackOrAreaColor.IsHexColor ? item.TrackOrAreaColor :
    "#808080";

    public static string TrackOrAreaTextColor(this CargoCustomer item) =>
        item.TrackOrAreaBackColor().TextColor;

    public static string TrackOrAreaDesignation(this CargoCustomer item) =>
        item is null ? string.Empty :
        item.CargoTrackOrArea.HasValue ? item.CargoTrackOrArea :
        item.TrackOrArea.HasValue ? item.TrackOrArea :
        string.Empty;

}
