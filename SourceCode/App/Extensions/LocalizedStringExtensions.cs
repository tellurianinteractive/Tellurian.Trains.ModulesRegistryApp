using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Shared;
using System.Globalization;
using System.Text;

namespace ModulesRegistry.Extensions;

public static class LocalizedStringExtensions
{
    public static string AddOrEdit(this IStringLocalizer localizer, string? objectName, bool isAdd) =>
        $"{localizer.AddOrEdit(isAdd)} {localizer.ObjectName(objectName, true)}";

    private static string AddOrEdit(this IStringLocalizer localizer, bool isAdd) =>
        isAdd ? localizer["Add"].Value : localizer["Edit"].Value;

    public static string ActionText(this IStringLocalizer localizer, string? label, string? objectName, PageAction action) =>
        label.HasValue() ? $"{localizer.ActionName(action)} {localizer.ObjectName(label, !action.IsEmpty())}" :
        $"{localizer.ActionName(action)} {localizer.ObjectName(objectName, !action.IsEmpty())}";

    private static string ActionName(this IStringLocalizer me, PageAction action) =>
        action.IsEmpty() ? string.Empty :
        me[action.ToString()].Value;

    public static string HeadingText(this IStringLocalizer localizer, string? label, string? objectName, object? owner, PageAction action)
    {
        var text = new StringBuilder(100);
        text.Append(localizer.ActionText(label, objectName, action));
        if (owner is not null)
        {
            text.Append(' ');
            //text.Append(me["For"].Value.ToLowerInvariant());
            //text.Append(' ');
            text.Append(localizer.Name(owner));
        }
        return text.ToString();
    }
    
    public static string Select(this IStringLocalizer localizer, string objectName) =>
        $"{localizer["Select"]} {localizer[objectName].Value.ToLowerInvariant()}";

    public static string ShowAll(this IStringLocalizer localizer, string objectName) =>
        $"{localizer["ShowAll"]} {localizer[objectName].ToString().ToLowerInvariant()}";
    public static string ShowAllInAll(this IStringLocalizer localizer, string objectName, string inObjectName) =>
        $"{localizer["ShowAll"]} {localizer[objectName].ToString().ToLowerInvariant()} {localizer["InAll"]} {localizer[inObjectName].ToString().ToLowerInvariant()}";

    public static string SearchObject(this IStringLocalizer localizer, string objectName, int minimumTypedCharactersCount = 1) =>
        $"{localizer["Search"]} {localizer[objectName].Value.ToLowerInvariant()}. {string.Format(localizer["TypeMinimumCharachers"].Value, minimumTypedCharactersCount)}.";

    public static string EditObject(this IStringLocalizer localizer, string objectName) =>
       $"{localizer["Edit"]} {localizer[objectName].Value.ToLowerInvariant()}";

    public static string EditOrViewObject(this IStringLocalizer localizer, string objectName, bool isEdit) =>
         isEdit ? localizer.EditObject(objectName) : $"{localizer[objectName].Value}";


    public static string NotFound<T>(this IStringLocalizer localizer) => $"{localizer[typeof(T).Name]} {localizer["NotFound"].Value.ToLowerInvariant()}";

    public static string Saved<T>(this IStringLocalizer localizer) => $"{localizer[typeof(T).Name]} {localizer["Saved"].Value.ToLowerInvariant()}";

    public static string Value(this IStringLocalizer localizer, string key) => localizer[key].Value;    
    
    private static bool IsEmpty(this PageAction pageAction) => pageAction == PageAction.List || pageAction == PageAction.Unknown || pageAction == PageAction.Error;

    private static string ObjectName(this IStringLocalizer localizer, string? objectName, bool toLower) =>
        string.IsNullOrWhiteSpace(objectName) ? string.Empty : toLower ? localizer[objectName].ToString().ToLowerInvariant() : localizer[objectName].ToString();

    private static string Name(this IStringLocalizer me, object? owner) =>
        owner switch
        {
            Person p => p.FullName(),
            Group g => g.FullName,
            Station s => s.FullName,
            ExternalStation es => es.FullName,
            Meeting m => $"{m.Name} {m.CityName}, {m.StartDate:MMMM yyyy}",
            Module mo => mo.FullName,
            Region r => r.LocalName,
            Layout l => $"{me["Layout"].ObjectNameToLower()} {l?.PrimaryModuleStandard?.ShortName}",
            StationCustomer sc => $"{sc.CustomerName}",
            _ => string.Empty
        };

   private static string ObjectNameToLower(this LocalizedString localizer) =>
        CultureInfo.CurrentCulture.TwoLetterISOLanguageName switch
        {
            "de" => localizer.Value,
            _ => localizer.Value.ToLowerInvariant()
        };    

}
