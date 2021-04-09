using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Extensions
{
    public static class ColorExtensions
    {
        public static string? TextColor(this string? backColor)
        {
            if (backColor.IsHexColor())
            {
                var r = int.Parse(backColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                var g = int.Parse(backColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                var b = int.Parse(backColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;
                return (yiq >= 128) ? "#000000" : "#FFFFFF";
            }
            return null;
        }

        private static string HexColorRegEx => "^#([A-Fa-f0-9]{6})$";
        private static bool IsHexColor([NotNullWhen(true)] this string? maybeColor) =>
            string.IsNullOrWhiteSpace(maybeColor) ? false :
            Regex.IsMatch(maybeColor, HexColorRegEx);

       
    }
}
