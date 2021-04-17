using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Shared;
using System;
using System.Text;

namespace ModulesRegistry.Extensions
{
    public static class LocalizedStringExtensions
    {
        [Obsolete]
        public static string AddOrEdit(this IStringLocalizer me, string? objectName, bool isAdd) =>
            $"{me.AddOrEdit(isAdd)} {me.ObjectName(objectName, true)}";

        [Obsolete]
        public static string AddOrEdit(this IStringLocalizer me, string? label, string? objectName, bool isAdd) =>
            label.HasValue() ? $"{me.AddOrEdit(isAdd)} {me.ObjectName(label, true)}" :
            $"{me.AddOrEdit(isAdd)} {me.ObjectName(objectName, true)}";

        [Obsolete]
        private static string AddOrEdit(this IStringLocalizer me, bool isAdd) =>
            isAdd ? me["Add"].Value : me["Edit"].Value;


        public static string ActionText(this IStringLocalizer me, string? label, string? objectName, PageAction action) =>
            label.HasValue() ? $"{me.ActionName(action)} {me.ObjectName(label, action != PageAction.List)}" :
            $"{me.ActionName(action)} {me.ObjectName(objectName, action != PageAction.List)}";

        private static string ActionName(this IStringLocalizer me, PageAction action) =>
            action == PageAction.List ? string.Empty :
            me[action.ToString()].Value;

        public static string HeadingText(this IStringLocalizer me, string? label, string? objectName, object? owner, PageAction action)
        {
            var text = new StringBuilder(100);
            text.Append(me.ActionText(label, objectName, action));
            if (owner is not null)
            {
                text.Append(' ');
                text.Append(me["For"].Value.ToLowerInvariant());
                text.Append(' ');
                text.Append(owner.Name());
            }
            return text.ToString();

        }

        [Obsolete]
        public static string AddOrEdit(this IStringLocalizer me, string? label, string? objectName, object? owner, bool isAdd)
        {
            var text = new StringBuilder(100);
            text.Append(me.AddOrEdit(label, objectName, isAdd));
            if (owner is not null)
            {
                text.Append(' ');
                text.Append(me["For"].Value.ToLowerInvariant());
                text.Append(' ');
                text.Append(owner.Name());
            }
            return text.ToString();

        }

        [Obsolete]
        public static string AddOrEdit(this IStringLocalizer me, string? objectName, object? owner, bool isAdd) =>
            me.AddOrEdit(null, objectName, owner, isAdd);     

        public static string ObjectOwner(this IStringLocalizer me, string objectName, object? owner) =>
            owner switch
            {
                Person => $"{me[objectName]} {me["OwnedBy"]} {owner.Name()}",
                Group => $"{me[objectName]} {me["OwnedBy"]} {owner.Name()}",
                _ => me[objectName].ToString()
            };
        private static string ObjectName(this IStringLocalizer me, string? objectName, bool toLower) =>
            string.IsNullOrWhiteSpace(objectName) ? string.Empty : toLower ? me[objectName].ToString().ToLowerInvariant() : me[objectName].ToString();

        private static string Name(this object? me) =>
            me switch
            {
                Person p => p.FullName(),
                Group g => g.FullName,
                Station s => s.FullName,
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
