namespace ModulesRegistry.Services.Implementations;

public sealed class ScaleService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public ScaleService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.Scales.ToListAsync().ConfigureAwait(false);
            return items
                .Select(s => new ListboxItem(s.Id, $"{s.ShortName} (1:{s.Denominator})"))
                .OrderBy(l => l.Description);
        }
        return Array.Empty<ListboxItem>();
    }
}
