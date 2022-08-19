using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace ModulesRegistry.Services.Extensions;

internal static class IQueryableExtensions
{
    public static ConfiguredTaskAwaitable<List<T>> ToReadOnlyListAsync<T>(this IQueryable<T> queryable) where T : class =>
        queryable.AsNoTracking().ToListAsync<T>().ConfigureAwait(false);

    public static ConfiguredTaskAwaitable<T?> ReadOnlySingleOrDefaultAsync<T>(this IQueryable<T> queryable, Expression<Func<T, bool>> expression) where T : class =>
        queryable.AsNoTracking().SingleOrDefaultAsync<T>(expression).ConfigureAwait(false);
    public static ConfiguredTaskAwaitable<T?> ReadOnlySingleOrDefaultAsync<T>(this IQueryable<T> queryable) where T : class =>
        queryable.AsNoTracking().SingleOrDefaultAsync<T>().ConfigureAwait(false);

}

public static class IEnumerableExtensions
{
    public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T>? values) =>
        values is null || !values.Any();

    public static bool IsEmpty<T>(this IEnumerable<T> values) =>
        !values.Any();
}
