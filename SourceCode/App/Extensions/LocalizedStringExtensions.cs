using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using System.Text;

namespace ModulesRegistry.Extensions
{
    public static class LocalizedStringExtensions
    {
        public static string AddOrEdit(this IStringLocalizer me, string objectName, bool isAdd) =>
            (isAdd ? me["Add"].Value : me["Edit"].Value) + " " + me[objectName].ToString().ToLowerInvariant();

        public static string Heading(this IStringLocalizer me, bool isCreate, string objectName, Person? person)
        {
            var text = new StringBuilder(100);
            text.Append(me.AddOrEdit(objectName, isCreate));
            if (person is not null)
            {
                text.Append(' ');
                text.Append(me["For"].Value.ToLowerInvariant());
                text.Append(' ');
                text.Append(person.FirstName);
            }
            return text.ToString();
        }
    }
}
