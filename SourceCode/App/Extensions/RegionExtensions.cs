using ModulesRegistry.Data;

namespace ModulesRegistry.Extensions;

public static class RegionExtensions
{
    public static string Style(this Region? it) =>
        $"background-color: {it?.BackColor ?? "white"}; color: {it?.ForeColor ?? "black"}; font-weight: bold";
}
