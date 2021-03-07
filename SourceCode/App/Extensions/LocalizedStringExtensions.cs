using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using System.Text;

namespace ModulesRegistry.Extensions
{
    public static class LocalizedStringExtensions
    {
        public static string AddOrEdit(this IStringLocalizer me, string objectName, bool isAdd) =>
            (isAdd ? me["Add"].Value : me["Edit"].Value) + " " + me[objectName].ToString().ToLowerInvariant();

        public static string HeadingAddOrEdit(this IStringLocalizer me, bool isCreate, string objectName, object? owner)
        {
            var text = new StringBuilder(100);
            text.Append(me.AddOrEdit(objectName, isCreate));
            if (owner is not null)
            {
                text.Append(' ');
                text.Append(me["For"].Value.ToLowerInvariant());
                text.Append(' ');
                text.Append(owner.Name());
            }
            return text.ToString();
        }

        public static string ObjectOwnerByOwner(this IStringLocalizer me, string objectName, object? owner) =>
            owner switch
            {
                Person => $"{me[objectName]} {me["OwnedBy"]} {owner.Name()}",
                Group => $"{me[objectName]} {me["OwnedBy"]} {owner.Name()}",
                _ => me[objectName].ToString()
            };


        public static string SearchObject(this IStringLocalizer me, string objectName) =>
            $"{me["Search"]} {me[objectName].Value.ToLowerInvariant()}";

        public static string EditObject(this IStringLocalizer me, string objectName) =>
           $"{me["Edit"]} {me[objectName].Value.ToLowerInvariant()}";

        private static string Name(this object? me) =>
            me switch
            {
                Person p => p.FullName(),
                Group g => g.FullName,
                _ => string.Empty
            };
    }
}
