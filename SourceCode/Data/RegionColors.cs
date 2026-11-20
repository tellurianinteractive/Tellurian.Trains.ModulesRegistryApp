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

    /// <summary>
    /// The permitted colour nearest to this one (decided 2026-11-20 for the freight customers' track or area colours, which were
    /// free until 1.15.22): a permitted colour or an earlier shade as <see cref="Permitted"/>; otherwise named as people would
    /// name it, by its hue, saturation and lightness (distance in RGB made light green and greys pink): greys, very light and very
    /// dark colours are white or black, light reds pink and dark reds and oranges brown. White when there is no colour or it
    /// can't be read.
    /// </summary>
    public static string Nearest(string? hex)
    {
        if (Permitted(hex) is { } permitted) return permitted;
        if (hex.NormalizedHexColor is not { } normalized) return White;
        var (hue, saturation, lightness) = Hsl(normalized);
        if (lightness < 0.12) return Black;
        if (lightness > 0.85) return White;
        if (saturation < 0.2) return lightness >= 0.5 ? White : Black;
        return hue switch
        {
            < 15 or >= 345 => lightness >= 0.7 ? Pink : lightness < 0.4 ? Brown : Red,
            < 45 => lightness < 0.4 ? Brown : Orange,
            < 70 => Yellow,
            < 170 => Green,
            < 260 => Blue,
            < 300 => Purple,
            _ => lightness >= 0.6 ? Pink : Purple,
        };

        static (double Hue, double Saturation, double Lightness) Hsl(string hex)
        {
            double r = Convert.ToInt32(hex[1..3], 16) / 255.0, g = Convert.ToInt32(hex[3..5], 16) / 255.0, b = Convert.ToInt32(hex[5..7], 16) / 255.0;
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), delta = max - min;
            var lightness = (max + min) / 2;
            if (delta == 0) return (0, 0, lightness);
            var saturation = delta / (1 - Math.Abs(2 * lightness - 1));
            var hue = max == r ? 60 * ((g - b) / delta % 6) : max == g ? 60 * ((b - r) / delta + 2) : 60 * ((r - g) / delta + 4);
            return (hue < 0 ? hue + 360 : hue, saturation, lightness);
        }
    }
}