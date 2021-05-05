#nullable disable
namespace ModulesRegistry.Data
{
    public static class LayoutModuleExtensions
    {
        public static string LayoutPosition(this LayoutModule? me) =>
            me is null ? string.Empty :
            me.LayoutLine is null ? Resources.Strings.NotPlacedInLayout :
            $"{me.LayoutLine.StretchShortName()} @ {me.LayoutLinePosition}";
    }
}
