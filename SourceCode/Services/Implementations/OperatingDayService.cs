namespace ModulesRegistry.Services.Implementations;

public class OperatingDayService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

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
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.OperatingDays.AsNoTracking()
            .OrderBy(od => od.DisplayOrder)
            .Select(od => new ListboxItem(od.Id, od.ShortNameLocalized()))
            .ToListAsync();
    }
}
