using ModulesRegistry.Services.Projections;

namespace ModulesRegistry.Services.Implementations;

public class ExternalStationService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public ExternalStationService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public async Task<IEnumerable<ExternalStation>> GetAllInRegion(ClaimsPrincipal? principal, int regionId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ExternalStations.AsNoTracking()
                .Where(es => es.RegionId == regionId)
                .Include(es => es.ExternalStationCustomers)
                .OrderBy(es => es.FullName)
                .ToListAsync();
        }
        return Array.Empty<ExternalStation>();
    }
    public async Task<IEnumerable<ExternalStation>> GetAllInCountry(ClaimsPrincipal? principal, int countryId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ExternalStations.AsNoTracking()
                .Where(es => es.Region.CountryId == countryId)
                .Include(es => es.ExternalStationCustomers)
                .OrderBy(es => es.FullName)
                .ToListAsync();
        }
        return Array.Empty<ExternalStation>();
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
