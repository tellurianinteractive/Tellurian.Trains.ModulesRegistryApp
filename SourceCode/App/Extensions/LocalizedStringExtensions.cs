using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using System.Text;

namespace ModulesRegistry.Extensions
{
    public static class LocalizedStringExtensions
    {
        public static string AddOrEdit(this IStringLocalizer me, string? objectName, bool isAdd) =>
            $"{me.AddOrEdit(isAdd)} {me.ObjectName(objectName, true)}";

        private static string AddOrEdit(this IStringLocalizer me, bool isAdd) =>
            isAdd ? me["Add"].Value : me["Edit"].Value;

        private static string ObjectName(this IStringLocalizer me, string? objectName, bool toLower) =>
            string.IsNullOrWhiteSpace(objectName) ? string.Empty : toLower ? me[objectName].ToString().ToLowerInvariant() : me[objectName].ToString();

        public static string AddOrEdit(this IStringLocalizer me, string? objectName, object? owner, bool isAdd)
        {
            var text = new StringBuilder(100);
            text.Append(me.AddOrEdit(objectName, isAdd));
            if (owner is not null)
            {
                text.Append(' ');
                text.Append(me["For"].Value.ToLowerInvariant());
                text.Append(' ');
                text.Append(owner.Name());
            }
            return text.ToString();
        }
        public static string ObjectOwner(this IStringLocalizer me, string objectName, object? owner) =>
            owner switch
            {
                Person => $"{me[objectName]} {me["OwnedBy"]} {owner.Name()}",
                Group => $"{me[objectName]} {me["OwnedBy"]} {owner.Name()}",
                _ => me[objectName].ToString()
            };

        private static string Name(this object? me) =>
            me switch
            {
                Person p => p.FullName(),
                Group g => g.FullName,
                _ => string.Empty
            };

        public static string Select(this IStringLocalizer me, string objectName) =>
            $"{me["Select"]} {me[objectName].Value.ToLowerInvariant()}";

        public static string ShowAll(this IStringLocalizer me, string objectName) =>
            $"{me["ShowAll"]} {me[objectName].ToString().ToLowerInvariant()}";
        public static string ShowAllInAll(this IStringLocalizer me, string objectName, string inObjectName) =>
            $"{me["ShowAll"]} {me[objectName].ToString().ToLowerInvariant()} {me["InAll"]} {me[inObjectName].ToString().ToLowerInvariant()}";

        public static string SearchObject(this IStringLocalizer me, string objectName) =>
            $"{me["Search"]} {me[objectName].Value.ToLowerInvariant()}";

        public static string EditObject(this IStringLocalizer me, string objectName) =>
           $"{me["Edit"]} {me[objectName].Value.ToLowerInvariant()}";

        public static string EditOrViewObject(this IStringLocalizer me, string objectName, bool isEdit) =>
             isEdit ? me.EditObject(objectName) : $"{me[objectName].Value}";


        public static string NotFound<T>(this IStringLocalizer me) => $"{me[typeof(T).Name]} {me["NotFound"].Value.ToLowerInvariant()}";
        public static string Saved<T>(this IStringLocalizer me) => $"{me[typeof(T).Name]} {me["Saved"].Value.ToLowerInvariant()}";

        public static string Value(this IStringLocalizer me, string key) => me[key].Value;
        public static string DocumentIconHref(this int? id, string fileExtension) =>
            id.HasValue ? $"/images/{fileExtension}.png" : string.Empty;

    }
}
