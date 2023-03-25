using ModulesRegistry.Data;

namespace ModulesRegistry.Extensions;

public static class RegionExtensions
{
    public static string Style(this Region? region) =>
        $"background-color: {region?.BackColor ?? "white"}; color: {region?.ForeColor ?? "black"}; font-weight: bold";
}
