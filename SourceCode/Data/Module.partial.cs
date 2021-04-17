using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ModulesRegistry.Data
{
    public static class ModuleExtensions
    {
        public static string? FremoName(this Module me) =>
            me.FremoNumber.HasValue &&
            me.ModuleOwnerships is not null &&
            me.ModuleOwnerships.Any() &&
            me.ModuleOwnerships.First().Person is not null &&
            me.ModuleOwnerships.First().Person.FremoOwnerSignature is not null ? 
            $"{me.ModuleOwnerships.First().Person.FremoOwnerSignature}{me.FremoNumber.Value}" : null;

        public static bool IsPartOfStation([NotNullWhen(true)]this Module me) => me.StationId.HasValue;

        public static IEnumerable<int> DocumentIds (this Module me)
        {
            if (me.PdfDocumentationId.HasValue) yield return me.PdfDocumentationId.Value;
            if (me.DwgDrawingId.HasValue) yield return me.DwgDrawingId.Value;
            if (me.SkpDrawingId.HasValue) yield return me.SkpDrawingId.Value;
        }
    }
}
