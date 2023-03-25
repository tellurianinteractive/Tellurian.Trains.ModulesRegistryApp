using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data.Extensions;

public static class StringExtensions
{
    public static string[]? AsArray(this string? value) =>
        value is null ? null : new string[] {value};

    public static bool HasValue([NotNullWhen(true)] this string? value) =>
        !string.IsNullOrWhiteSpace(value);

    public static bool HasNoValue([NotNullWhen(false)] this string? value) =>
        string.IsNullOrWhiteSpace(value);

    public static string FirstItem(this string? value, string defaultValue = "") =>
        value is null || value.Length == 0 ? defaultValue : value.Split(',')[0] ?? defaultValue;
}
