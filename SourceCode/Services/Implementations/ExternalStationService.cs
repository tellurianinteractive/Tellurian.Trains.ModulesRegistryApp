using ModulesRegistry.Services.Projections;

namespace ModulesRegistry.Services.Implementations;

public sealed class ExternalStationService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int? countryId, int? regionId)
    {
        if (principal is null ) return Enumerable.Empty<ListboxItem>();
        var actualCountryId = principal.CountryId(countryId);
        var sql = string.Empty;
        if (regionId.HasValue) sql = $"SELECT * FROM ListExternalStation WHERE [RegionId] = {regionId.Value}";
        else if (countryId > 0) sql = $"SELECT * FROM ListExternalStation WHERE [CountryId] = {actualCountryId}";
        else sql = $"SELECT * FROM ListExternalStation";
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.ListboxItems.FromSqlRaw(sql).OrderBy(l => l.Description).ToListAsync();
    }

    public async Task<IEnumerable<ExternalStation>> GetAsync(ClaimsPrincipal? principal, int countryId = 0, int regionId =0)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ExternalStations.AsNoTracking()
                .Where(es => countryId ==0 || es.Region.CountryId == countryId && regionId==0 || (countryId == 0 && es.RegionId==regionId))
                .Include(es => es.ExternalStationCustomers)
                .OrderBy(es => es.FullName)
                .ToListAsync();
        }
        return [];
    }
    public async Task<ExternalStation?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ExternalStations.AsNoTracking()
                .Include(es => es.ExternalStationCustomers).ThenInclude(esc => esc.ExternalStationCustomerCargos)
                .SingleOrDefaultAsync(es => es.Id == id);
        }
        return null;
    }

    public async Task<(int Count, string Message, ExternalStation? Entity)> SaveAsync(ClaimsPrincipal? principal, ExternalStation entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.ExternalStations
                .Include(es => es.ExternalStationCustomers)
                .SingleOrDefaultAsync(es => es.Id == entity.Id);
            if (existing is null)
            {
                dbContext.ExternalStations.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing);
            }
        }
        return principal.SaveNotAuthorised<ExternalStation>();
    }

    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsCountryOrGlobalAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.ExternalStations
                .Include(es => es.ExternalStationCustomers)
                .ThenInclude(esc => esc.ExternalStationCustomerCargos)
                .SingleOrDefaultAsync(es => es.Id == id);
            if (existing is null) { return (0, Resources.Strings.NothingToDelete); }
            dbContext.ExternalStations.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.NotAuthorized<ExternalStation>();

    }

    #region External station customers

    public async Task<IEnumerable<FreightCustomerInfo>> CustomersAsync(ClaimsPrincipal? principal, int? maybeCountryId)
    {
        if (principal.IsAuthenticated())
        {
            var countryId = maybeCountryId ?? principal.CountryId();
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.ExternalStationCustomers.AsNoTracking()
                .Where(esc => countryId == 0 || esc.ExternalStation.Region.CountryId == countryId)
                .Include(esc => esc.ExternalStationCustomerCargos).ThenInclude(escc => escc.Direction)
                .Include(esc => esc.ExternalStationCustomerCargos).ThenInclude(escc => escc.Cargo)
                .Include(esc => esc.ExternalStationCustomerCargos).ThenInclude(escc => escc.QuantityUnit)
                .Include(esc => esc.ExternalStation).ThenInclude(es => es.Region).ThenInclude(r => r.Country)
                .ToListAsync();
            return items.Select(i => i.ToFreightCustomerInfo());
        }
        return Array.Empty<FreightCustomerInfo>();
    }

    public async Task<ExternalStationCustomer?> FindCustomerByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ExternalStationCustomers.AsNoTracking()
                .Include(esc => esc.ExternalStation)
                .Include(esc => esc.ExternalStationCustomerCargos)
                .SingleOrDefaultAsync(esc => esc.Id == id);
        }
        return null;
    }

    public async Task<IEnumerable<ExternalStationCustomer>> FindCustomersByIdAsync(ClaimsPrincipal? principal, int stationId, int customerId = 0)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ExternalStationCustomers.AsNoTracking()
                .Include(esc => esc.ExternalStation)
                .Include(esc => esc.ExternalStationCustomerCargos)
                .Where(esc => esc.ExternalStationId == stationId && (customerId==0 || esc.Id == customerId))
                .ToListAsync();
        }
        return Array.Empty<ExternalStationCustomer>();
    }

    public async Task<(int Count, string Message, ExternalStationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, ExternalStationCustomer entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.ExternalStationCustomers.Include(esc => esc.ExternalStationCustomerCargos).SingleOrDefaultAsync(esc => esc.Id == entity.Id);
            if (existing is null)
            {
                dbContext.ExternalStationCustomers.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                AddOrRemoveCustomerCargos(dbContext, entity, existing);
                if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing);
            }

            static void AddOrRemoveCustomerCargos(ModulesDbContext dbContext, ExternalStationCustomer entity, ExternalStationCustomer existing)
            {
                foreach (var cargo in entity.ExternalStationCustomerCargos)
                {
                    var current = existing.ExternalStationCustomerCargos.AsQueryable().FirstOrDefault(t => t.Id == cargo.Id);
                    if (current is null) existing.ExternalStationCustomerCargos.Add(cargo);
                    else dbContext.Entry(current).CurrentValues.SetValues(cargo);
                }
                foreach (var cargo in existing.ExternalStationCustomerCargos) if (!entity.ExternalStationCustomerCargos.Any(st => st.Id == cargo.Id)) dbContext.Remove(cargo);
            }

            static bool IsUnchanged(ModulesDbContext dbContext, ExternalStationCustomer customer) =>
                dbContext.Entry(customer).State == EntityState.Unchanged &&
                customer.ExternalStationCustomerCargos.All(scc => dbContext.Entry(scc).State == EntityState.Unchanged);


        }
        return principal.SaveNotAuthorised<ExternalStationCustomer>();
    }
    #endregion
}
