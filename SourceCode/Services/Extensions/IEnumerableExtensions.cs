using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;

public static class IEnumerableExtensions
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? values) =>
        values is null || !values.Any();
}
