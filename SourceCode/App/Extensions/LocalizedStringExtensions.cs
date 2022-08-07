using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services;
using ModulesRegistry.Shared;
using System.Globalization;
using System.Text;

namespace ModulesRegistry.Extensions;

public static class LocalizedStringExtensions
{
    public static string AddOrEdit(this IStringLocalizer me, string? objectName, bool isAdd) =>
        $"{me.AddOrEdit(isAdd)} {me.ObjectName(objectName, true)}";


    private static string AddOrEdit(this IStringLocalizer me, bool isAdd) =>
        isAdd ? me["Add"].Value : me["Edit"].Value;


    public static string ActionText(this IStringLocalizer me, string? label, string? objectName, PageAction action) =>
        label.HasValue() ? $"{me.ActionName(action)} {me.ObjectName(label, !action.IsEmpty())}" :
        $"{me.ActionName(action)} {me.ObjectName(objectName, !action.IsEmpty())}";

    private static string ActionName(this IStringLocalizer me, PageAction action) =>
        action.IsEmpty() ? string.Empty :
        me[action.ToString()].Value;

    public static string HeadingText(this IStringLocalizer me, string? label, string? objectName, object? owner, PageAction action)
    {
        var text = new StringBuilder(100);
        text.Append(me.ActionText(label, objectName, action));
        if (owner is not null)
        {
            text.Append(' ');
            //text.Append(me["For"].Value.ToLowerInvariant());
            //text.Append(' ');
            text.Append(me.Name(owner));
        }
        return text.ToString();

    }

    private static bool IsEmpty(this PageAction me) => me == PageAction.List || me == PageAction.Unknown || me == PageAction.Error;

    private static string ObjectName(this IStringLocalizer me, string? objectName, bool toLower) =>
        string.IsNullOrWhiteSpace(objectName) ? string.Empty : toLower ? me[objectName].ToString().ToLowerInvariant() : me[objectName].ToString();

    private static string Name(this IStringLocalizer me, object? owner) =>
        owner switch
        {
            Person p => p.FullName(),
            Group g => g.FullName,
            Station s => s.FullName,
            ExternalStation es => es.FullName,
            Meeting m => $"{m.Description} {m.PlaceName}, {m.StartDate:MMMM yyyy}",
            Module mo => mo.FullName,
            Region r => r.LocalName,
            Layout l => $"{me["Layout"].ObjectNameToLower()} {l?.PrimaryModuleStandard?.ShortName}",
            _ => string.Empty
        };

    public static string Select(this IStringLocalizer me, string objectName) =>
        $"{me["Select"]} {me[objectName].Value.ToLowerInvariant()}";

    public static string ShowAll(this IStringLocalizer me, string objectName) =>
        $"{me["ShowAll"]} {me[objectName].ToString().ToLowerInvariant()}";
    public static string ShowAllInAll(this IStringLocalizer me, string objectName, string inObjectName) =>
        $"{me["ShowAll"]} {me[objectName].ToString().ToLowerInvariant()} {me["InAll"]} {me[inObjectName].ToString().ToLowerInvariant()}";

    public static string SearchObject(this IStringLocalizer me, string objectName, int minimumTypedCharactersCount = 1) =>
        $"{me["Search"]} {me[objectName].Value.ToLowerInvariant()}. {string.Format(me["TypeMinimumCharachers"].Value, minimumTypedCharactersCount)}.";

    public static string EditObject(this IStringLocalizer me, string objectName) =>
       $"{me["Edit"]} {me[objectName].Value.ToLowerInvariant()}";

    public static string EditOrViewObject(this IStringLocalizer me, string objectName, bool isEdit) =>
         isEdit ? me.EditObject(objectName) : $"{me[objectName].Value}";


    public static string NotFound<T>(this IStringLocalizer me) => $"{me[typeof(T).Name]} {me["NotFound"].Value.ToLowerInvariant()}";
    public static string Saved<T>(this IStringLocalizer me) => $"{me[typeof(T).Name]} {me["Saved"].Value.ToLowerInvariant()}";

    public static string Value(this IStringLocalizer me, string key) => me[key].Value;


    private static string ObjectNameToLower(this LocalizedString me) =>
        CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "de" => me.Value,
            _ => me.Value.ToLowerInvariant()
        };

    public static string DocumentIconHref(this int? id, string fileExtension) =>
        id.HasValue ? $"/images/{fileExtension}.png" : string.Empty;


    public static string DualLanguageLabel(this IStringLocalizer localizer, string? langaugeCode, string resourceKey, string? englishText) =>
        DualLanguageLabel(localizer, null, langaugeCode, resourceKey, englishText);

    public static string DualLanguageLabel(this IStringLocalizer localizer, IEnumerable<LanguageLabels>? languageLabels, string? langaugeCode, string resourceKey, string? englishText)
    {
        englishText ??= resourceKey;
        var localText = localizer[resourceKey].Value;
        if (languageLabels is not null && langaugeCode is not null) localText = languageLabels.GetLabelText(resourceKey, langaugeCode);
        if (string.IsNullOrWhiteSpace(localText)) localText = localizer[resourceKey];
        if (string.IsNullOrWhiteSpace(localText) || englishText == localText) return englishText!;
        return $"{englishText}/{localText}";
    }
}
