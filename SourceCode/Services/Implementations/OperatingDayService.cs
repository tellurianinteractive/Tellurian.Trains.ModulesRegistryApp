using Microsoft.Extensions.Caching.Memory;

namespace ModulesRegistry.Services.Implementations;

public class OperatingDayService(IDbContextFactory<ModulesDbContext> factory, IMemoryCache cache)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly IMemoryCache Cache = cache;

    public async Task<IEnumerable<ListboxItem>> BasicDaysItemsAsync()
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.OperatingDays.AsNoTracking()
            .Where(od => od.IsBasicDay)
            .OrderBy(od => od.Flag)
            .Select(od => new ListboxItem(od.Id, od.ShortNameLocalized()))
            .ToListAsync();
    }

    public async Task<IEnumerable<ListboxItem>> AllDaysItemsAsync()
    {
        // Kept in memory (ReferenceDataCache); the texts depend on the user's language, so the items are made on each call.
        var days = await Cache.GetReferenceDataAsync(ReferenceDataCache.OperatingDays, Factory, db => db.OperatingDays);
        return days.OrderBy(od => od.DisplayOrder).Select(od => new ListboxItem(od.Id, od.ShortNameLocalized())).ToList();
    }
}
