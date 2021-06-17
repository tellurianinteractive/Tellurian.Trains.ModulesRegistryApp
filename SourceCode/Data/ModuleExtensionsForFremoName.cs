using ModulesRegistry.Data.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace ModulesRegistry.Data
{
    public static class ModuleExtensionsForFremoName
    {
        public static string? FremoName(this Module me)
        {
            if (me.FremoNumber.HasValue && me.ModuleOwnerships?.Count > 0)
            {
                if (me.ModuleOwnerships.First().Person?.FremoOwnerSignature is not null)
                {
                    return $"{me.ModuleOwnerships.First().Person.FremoOwnerSignature}{me.FremoNumber.Value}";
                }
                else if (me.ModuleOwnerships.First().Group is not null)
                {
                    return $"{me.ModuleOwnerships.First().Group.ShortName}{me.FremoNumber.Value}";
                }
            }
            return null;
        }

        public static bool HasAnyFremoName(this IEnumerable<Module>? modules) =>
            modules?.Any(m => m.FremoNumber.HasValue && m.ModuleOwnerships.Any(mo => (mo.Person?.FremoOwnerSignature.HasValue() == true) || (mo.Group?.ShortName.HasValue() == true))) == true;
    }
}
