using ModulesRegistry.Resources;
using Tellurian.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data.Extensions;

/// <summary>
/// The app's own string helpers that Tellurian.Utilities doesn't have; prefer the package's (HasValue, IsEmpty, OrEmpty, ToItems,
/// IsAnyOf, Truncated, WithInitialCapital, ...).
/// </summary>
public static class StringExtensions
{
    /// <summary>A filter rule: true when there is no value or nothing to find, or the value contains it, ignoring case.</summary>
    public static bool ContainsCaseInsensitive(this string? value, string? find) =>
        value is null || find.IsEmpty || value.Contains(find, StringComparison.OrdinalIgnoreCase);

    /// <summary>A filter rule: true when there is no value or nothing to find, or the value starts with it, ignoring case.</summary>
    public static bool StartsWithCaseInsensitive(this string? value, string? start) =>
        value is null || start.IsEmpty || start.Length > 0 && value.StartsWith(start, StringComparison.OrdinalIgnoreCase);

    public static string ToLowerLanguageSensitive(this string? value) =>
        value is null ? string.Empty :
        Strings.Casing.EqualsCaseInsensitive("LOWER") ? value.ToLowerInvariant() :
        value;
}
