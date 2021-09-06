namespace ModulesRegistry.Services.Implementations;

public class RegionService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public RegionService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int? maybeCountryId = null)
    {
        if (principal is not null)
        {
            var countryId = maybeCountryId.HasValue ? maybeCountryId : principal.IsGlobalAdministrator() ? 0 : principal.CountryId();
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.Regions.AsNoTracking()
                .Where(r => countryId == 0 || r.CountryId == countryId).Include(r => r.Country).ToListAsync();
            return items.Select(r => new ListboxItem(r.Id, Description(r, maybeCountryId > 0)))
                .OrderBy(l => l.Description)
                .AsEnumerable();

        }
        return Array.Empty<ListboxItem>();

        static string Description(Region region, bool singleCountry) =>
            singleCountry ? region.LocalName :
            $"{region.Country.EnglishName.Localized()}: {region.LocalName}";
    }

    public async Task<IEnumerable<Region>> AllAsync(ClaimsPrincipal? principal, int? maybeCountryId = null)
    {
        if (principal is not null)
        {
            var countryId = maybeCountryId.HasValue ? maybeCountryId : principal.IsGlobalAdministrator() ? 0 : principal.CountryId();
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Regions.AsNoTracking()
                .Where(r => countryId == 0 || r.CountryId == countryId)
                .Include(r => r.Country)
                .ToListAsync();

        }
        return Array.Empty<Region>();
    }

    public async Task<Region?> FindById(ClaimsPrincipal? principal, int id)
    {
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Regions.AsNoTracking()
                .SingleOrDefaultAsync(r => r.Id == id);
        }
        return null;
    }

    public async Task<(int Count, string Message, Region? Entity)> SaveAsync(ClaimsPrincipal? principal, Region entity)
    {
        if (principal.IsAnyAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.Regions.FindAsync(entity.Id);
            if (existing is null)
            {
                dbContext.Regions.Add(entity);
                var count = await dbContext.SaveChangesAsync();
                return count.SaveResult(entity);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
                var count = await dbContext.SaveChangesAsync();
                return count.SaveResult(existing);
            }
        }
        return principal.SaveNotAuthorised<Region>();
    }
}
