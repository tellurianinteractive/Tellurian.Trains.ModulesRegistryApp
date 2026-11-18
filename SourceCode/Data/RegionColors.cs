namespace ModulesRegistry.Data;

/// <summary>A colour with a name that is also its resource key.</summary>
public sealed record NamedColor(string Name, string Hex);

/// <summary>
/// The colours a region may have, in the order they are offered (decided 2026-11-18). A region's colour is its
/// <see cref="Region.BackColor"/>; the text colour follows from it.
/// </summary>
public static class RegionColors
{
    public const string Yellow = "#FFFF00";
    public const string Red = "#FF0000";
    public const string Green = "#009933";
    public const string Blue = "#0066FF";
    public const string Brown = "#993300";
    public const string Black = "#000000";
    public const string Orange = "#FF8000";
    public const string Purple = "#800080";
    public const string Pink = "#FF66CC";
    public const string White = "#FFFFFF";

    /// <summary>Matches only the colours above, for <c>[RegularExpression]</c>, which is also checked in the browser.</summary>
    public const string Pattern = "^(" + Yellow + "|" + Red + "|" + Green + "|" + Blue + "|" + Brown + "|" + Black + "|" + Orange + "|" + Purple + "|" + Pink + "|" + White + ")$";

    public static IReadOnlyList<NamedColor> All { get; } =
    [
        new(nameof(Yellow), Yellow),
        new(nameof(Red), Red),
        new(nameof(Green), Green),
        new(nameof(Blue), Blue),
        new(nameof(Brown), Brown),
        new(nameof(Black), Black),
        new(nameof(Orange), Orange),
        new(nameof(Purple), Purple),
        new(nameof(Pink), Pink),
        new(nameof(White), White),
    ];

    /// <summary>The permitted colour with this value, ignoring case, or null.</summary>
    public static string? Permitted(string? hex) =>
        All.FirstOrDefault(c => c.Hex.Equals(hex, StringComparison.OrdinalIgnoreCase))?.Hex;
}