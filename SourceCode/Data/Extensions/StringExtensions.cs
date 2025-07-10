using ModulesRegistry.Data.Resources;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data.Extensions;

public static class StringExtensions
{
    public static string[]? AsArray(this string? value) =>
        value is null ? null : new string[] { value };

    public static bool ContainsCaseInsensitive(this string? value, string? find) =>
        value is null || find.HasNoValue() || value.Contains(find, StringComparison.OrdinalIgnoreCase);

    public static bool StartsWithCaseInsensitive(this string? value, string? start) =>
        value is null || start.HasNoValue() || start.Length > 0 && value.StartsWith(start, StringComparison.OrdinalIgnoreCase);

    public static bool HasValue([NotNullWhen(true)] this string? value) =>
        !string.IsNullOrWhiteSpace(value);

    public static bool HasValue(this string? value, params string[] any) =>
        any is not null && !string.IsNullOrEmpty(value) && any.Contains(value, StringComparer.OrdinalIgnoreCase);

    public static bool HasValueExcept([NotNullWhen(true)] this string? value, params string[] except) =>
        except is not null ? !string.IsNullOrEmpty(value) && !except.Contains(value, StringComparer.OrdinalIgnoreCase) :
        !string.IsNullOrWhiteSpace(value);

    public static bool HasNoValue([NotNullWhen(false)] this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static string FirstItem(this string? value, string defaultValue = "", char delimiter = ',') =>
        value is null || value.Length == 0 ? defaultValue :
        value.Split(delimiter)[0] ?? defaultValue;

    public static string ToFirstLowerInvariant(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty :
        value.Length == 1 ? value.ToLowerInvariant() :
        string.Concat(value[0].ToString().ToLowerInvariant(), value.AsSpan(1));

    public static string ToFirstUpperInvariant(this string? value) =>
         string.IsNullOrWhiteSpace(value) ? string.Empty :
         value.Length == 1 ? value.ToUpperInvariant() :
         string.Concat(value[0].ToString().ToUpperInvariant(), value.AsSpan(1));

    public static string ToFirstUpperInvariant(this string[]? values) =>
    
        values is null || values.Length == 0 ? string.Empty :
        values.Length == 1 ? values[0] :
        $"{values[0]} {string.Join(' ',values.Skip(1)).ToLowerInvariant()}";
    

    public static string[] Items(this string? value, char separator = ';') =>
        string.IsNullOrWhiteSpace(value) ? [] :
        value.Trim().Split(separator);

    public static string ToLowerLanguageSensitive(this string? value) =>
        value is null ? string.Empty :
        Strings.Casing.Equals("LOWER", StringComparison.OrdinalIgnoreCase) ? value.ToLowerInvariant() :
        value;

    public static string MaxLenght(this string? value, int max, bool endDotted = false) =>
        value is null ? string.Empty :
        value.Length <= max ? value :
        endDotted ? $"{value[..value.LengtUptoLastSpace(max)]}..." :
        value[..max];

    private static int LengtUptoLastSpace(this string? value, int max)
    {
        if (value is null) return 0;
        var pos = value[..max].LastIndexOf(' ');
        return pos > 0 ? pos : max;
    }


}
