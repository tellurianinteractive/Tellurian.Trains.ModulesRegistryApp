using ModulesRegistry.Data;

namespace ModulesRegistry.Extensions;

public static class RegionExtensions
{
    public static string Style(this Region? it) =>
        it is null ? string.Empty :
        $"backcolor: {it.BackColor}; color: {it.ForeColor}";
}
