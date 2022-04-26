using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Data.Extensions;

public static class StringExtensions
{
    public static bool HasValue([NotNullWhen(true)] this string? me) =>
        !string.IsNullOrWhiteSpace(me);

    public static bool HasNoValue([NotNullWhen(false)] this string? me) =>
        string.IsNullOrWhiteSpace(me);

    public static string FirstItem(this string? me, string defaultValue = "") =>
        me is null || me.Length == 0 ? defaultValue : me.Split(',')[0] ?? defaultValue;

    #region Color 
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

    public static bool IsWhiteColor([NotNullWhen(true)] this string? maybeColor) => string.IsNullOrWhiteSpace(maybeColor) || maybeColor.ToUpperInvariant() == "#FFFFFF";

    #endregion
}

public static class DateTimeExtensions
{
    public static string AsPeriod(this (DateTime? from, DateTime? to) period, string format = "d") =>
        period.from.HasValue && period.to.HasValue ? $"{period.from.Value.ToString(format)} - {period.to.Value.ToString(format)}" :
        period.from.HasValue ? $"{period.from.Value.ToString(format)} - " :
        period.to.HasValue ? $"- {period.to.Value.ToString(format)}" :
        string.Empty;

    public static string AsPeriod(this (short? from, short? to) period) =>
        period.from.HasValue && period.to.HasValue ? $"{period.from.Value} - {period.to.Value}" :
        period.from.HasValue ? $"{period.from.Value} - " :
        period.to.HasValue ? $"- {period.to.Value}" :
        string.Empty;

}
