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

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, bool allSupportedCountries = false, bool allCountries = false)
    {
        if (principal is null) return Array.Empty<ListboxItem>();
        using var dbContext = Factory.CreateDbContext();
        var countries = await dbContext.Countries.ToReadOnlyListAsync();
        return countries.AsEnumerable()
            .Where(c => allCountries || (c.IsFullySupported && allSupportedCountries) || (principal.IsGlobalAdministrator() || principal.IsAuthorisedInCountry(c.Id)))
            .Select(c => new ListboxItem(c.Id, c.EnglishName.AsLocalized()))
            .OrderBy(l => l.Description)
            .ToList();
    }

    public async Task<IEnumerable<CountryStatistics>> GetCountryStatisticsAsync()
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.CountriesStatistics
            .Where(cs => cs.ModulesCount > 0)
            .ToReadOnlyListAsync();
    }
}
