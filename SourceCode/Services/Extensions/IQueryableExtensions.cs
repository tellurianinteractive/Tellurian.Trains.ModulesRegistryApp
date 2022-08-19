using System.Runtime.CompilerServices;

namespace ModulesRegistry.Services.Extensions;

internal static class IQueryableExtensions
{
    public static ConfiguredTaskAwaitable<List<T>> ToReadOnlyListAsync<T>(this IQueryable<T> queryable) where T : class =>
        queryable.AsNoTracking().ToListAsync<T>().ConfigureAwait(false);

}
