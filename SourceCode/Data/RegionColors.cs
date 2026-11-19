namespace ModulesRegistry.Data;

/// <summary>
/// The colours a region may have, in the order they are offered (decided 2026-11-18), with the standard web hex codes of their
/// names (decided 2026-11-20; <c>ToNamedColors</c> in Tellurian.Utilities). A region's colour is its <see cref="Region.BackColor"/>;
/// the text colour follows from it.
/// </summary>
public static class RegionColors
{
    // Constants for the [RegularExpression] pattern, since an attribute argument must be constant; a test checks them against All.
    public const string Yellow = "#FFFF00";
    public const string Red = "#FF0000";
    public const string Green = "#008000";
    public const string Blue = "#0000FF";
    public const string Brown = "#A52A2A";
    public const string Black = "#000000";
    public const string Orange = "#FFA500";
    public const string Purple = "#800080";
    public const string Pink = "#FFC0CB";
    public const string White = "#FFFFFF";

    /// <summary>Matches only the colours above, for <c>[RegularExpression]</c>, which is also checked in the browser.</summary>
    public const string Pattern = "^(" + Yellow + "|" + Red + "|" + Green + "|" + Blue + "|" + Brown + "|" + Black + "|" + Orange + "|" + Purple + "|" + Pink + "|" + White + ")$";

    /// <summary>The colours with their names, which are also resource keys, in the order they are offered.</summary>
    public static IReadOnlyList<NamedColor> All { get; } =
        new[] { nameof(Yellow), nameof(Red), nameof(Green), nameof(Blue), nameof(Brown), nameof(Black), nameof(Orange), nameof(Purple), nameof(Pink), nameof(White) }.ToNamedColors();

    /// <summary>The app's own shades until 2026-11-20, corrected to the standard colour of the same name.</summary>
    private static readonly Dictionary<string, string> Previous = new(StringComparer.OrdinalIgnoreCase)
    {
        ["#009933"] = Green,
        ["#0066FF"] = Blue,
        ["#993300"] = Brown,
        ["#FF8000"] = Orange,
        ["#FF66CC"] = Pink,
    };

    /// <summary>The permitted colour with this value, ignoring case, also for a colour of the earlier shades; or null.</summary>
    public static string? Permitted(string? hex)
    {
        if (hex.IsEmpty) return null;
        var value = hex.Trim();
        return Previous.TryGetValue(value, out var corrected) ? corrected : All.FirstOrDefault(c => c.HexColor.EqualsCaseInsensitive(value))?.HexColor;
    }
}