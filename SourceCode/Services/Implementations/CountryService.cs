namespace ModulesRegistry.Services.Implementations;

public sealed class CountryService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public CountryService(IDbContextFactory<ModulesDbContext> factory)
    {
        Factory = factory;
    }

    public async Task<Country?> FindById(int id)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Countries.FindAsync(id);
    }

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, bool isAdministrator = false)
    {
        if (principal is null) return Array.Empty<ListboxItem>();
        using var dbContext = Factory.CreateDbContext();
        var countries = await dbContext.Countries.ToListAsync();
        return countries.AsEnumerable().Where(c => isAdministrator || principal.IsAuthorisedInCountry(c.Id)).Select(c => new ListboxItem(c.Id, c.EnglishName.AsLocalized())).ToList();
    }
}
