using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Data.Extensions;

public static class HexColorExtensions
{
    public static string TextColor(this string? backColor)
    {
        if (backColor.IsHexColor())
        {
            var r = int.Parse(backColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            var g = int.Parse(backColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            var b = int.Parse(backColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;
            return (yiq >= 128) ? "#000000" : "#FFFFFF";
        }
        return "#000000";
    }

    private static string HexColorRegEx => "^#([A-Fa-f0-9]{6})$";
    public static bool IsHexColor([NotNullWhen(true)] this string? maybeColor) =>
        !string.IsNullOrWhiteSpace(maybeColor) && Regex.IsMatch(maybeColor, HexColorRegEx);

    public static bool IsWhiteColor([NotNullWhen(true)] this string? maybeColor) => 
        string.IsNullOrWhiteSpace(maybeColor) || 
        maybeColor.ToUpperInvariant() == "#FFFFFF" ||
        maybeColor.Equals("white", StringComparison.OrdinalIgnoreCase);

}
