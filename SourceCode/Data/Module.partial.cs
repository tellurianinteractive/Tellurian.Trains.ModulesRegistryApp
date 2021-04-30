using ModulesRegistry.Data.Extensions;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ModulesRegistry.Data
{
    public static class ModuleExtensions
    {
        public static bool IsPartOfStation([NotNullWhen(true)] this Module me) => me.StationId.HasValue;

        public static IEnumerable<int> DocumentIds(this Module me)
        {
            if (me.PdfDocumentationId.HasValue) yield return me.PdfDocumentationId.Value;
            if (me.DwgDrawingId.HasValue) yield return me.DwgDrawingId.Value;
            if (me.SkpDrawingId.HasValue) yield return me.SkpDrawingId.Value;
        }
    }

    public static class FremoNameRulesExtension
    {
        public static string? FremoName(this Module me)
        {
            if (me.FremoNumber.HasValue && me.ModuleOwnerships is not null && me.ModuleOwnerships.Any())
            {
                if (me.ModuleOwnerships.First().Person is not null && me.ModuleOwnerships.First().Person.FremoOwnerSignature is not null)
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
            modules is not null && modules.Any(m => m.FremoNumber.HasValue && m.ModuleOwnerships.Any(mo => (mo.Person is not null && mo.Person.FremoOwnerSignature.HasValue()) || mo.Group is not null && mo.Group.ShortName.HasValue()));

    }
}
