using Microsoft.Extensions.Caching.Memory;

namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Lookup rows kept in memory, so the pages that show them don't make a round trip to the database for each list
/// (the freight customer pages made six of their ten). The cache only holds data: the rows are read with the calling
/// service's own context factory, which is scoped (the change log needs the user), so the cache never holds a factory.
/// A row changed in the app removes its key; other changes (in the database) show within <see cref="Duration"/>.
/// The app runs as one instance, so a cache in memory is enough.
/// </summary>
public static class ReferenceDataCache
{
    public static readonly TimeSpan Duration = TimeSpan.FromHours(1);

    public const string Cargos = "ReferenceData.Cargos";
    public const string CargoDirections = "ReferenceData.CargoDirections";
    public const string CargoUnits = "ReferenceData.CargoUnits";
    public const string CargoReadyTimes = "ReferenceData.CargoReadyTimes";
    public const string CargoPackagingUnits = "ReferenceData.CargoPackagingUnits";
    public const string OperatingDays = "ReferenceData.OperatingDays";

    extension(IMemoryCache cache)
    {
        /// <summary>
        /// The rows of <paramref name="query"/>, read without tracking. Callers make new objects from them and must not change them.
        /// </summary>
        public async Task<IReadOnlyList<T>> GetReferenceDataAsync<T>(string key, IDbContextFactory<ModulesDbContext> factory, Func<ModulesDbContext, IQueryable<T>> query) where T : class =>
            await cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Duration;
                await using var dbContext = await factory.CreateDbContextAsync().ConfigureAwait(false);
                IReadOnlyList<T> rows = await query(dbContext).AsNoTracking().ToListAsync().ConfigureAwait(false);
                return rows;
            }).ConfigureAwait(false) ?? [];
    }
}