using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
}
