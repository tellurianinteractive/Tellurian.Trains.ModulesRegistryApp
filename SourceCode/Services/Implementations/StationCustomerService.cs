using ModulesRegistry.Data.Resources;
using ModulesRegistry.Services.Projections;

namespace ModulesRegistry.Services.Implementations;

public class StationCustomerService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public StationCustomerService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public async Task<IEnumerable<StationCustomer>> AllAsync(ClaimsPrincipal? principal, int stationId, int customerId = 0)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomers.AsNoTracking()
                .Include(sc => sc.Cargos).ThenInclude(scc => scc.Cargo)
                .Where(sc => sc.StationId == stationId && (customerId == 0 || sc.Id == customerId))
                .ToListAsync();
        }
        return Array.Empty<StationCustomer>();
    }

    public async Task<StationCustomer?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomers.Include(sc => sc.Cargos).AsNoTracking().SingleOrDefaultAsync(sc => sc.Id == id);
        }
        return null;
    }
    public Task<(int Count, string Message, StationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, int stationId, StationCustomer entity) =>
        SaveAsync(principal, stationId, entity, principal.AsModuleOwnershipRef());

    public async Task<IEnumerable<FreightCustomerInfo>> CustomersAsync(ClaimsPrincipal? principal, int? maybeCountryId)
    {
        if (principal.IsAuthenticated())
        {
            var countryId = maybeCountryId ?? principal.CountryId();
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.StationCustomers.AsNoTracking()
                .Where(sc => (countryId == 0 || sc.Station.Region.CountryId == countryId))
                .OrderBy(sc => sc.Station.FullName).ThenBy(esc => esc.CustomerName)
                .Include(sc => sc.Cargos).ThenInclude(escc => escc.Direction)
                .Include(sc => sc.Cargos).ThenInclude(escc => escc.Cargo)
                .Include(sc => sc.Cargos).ThenInclude(escc => escc.QuantityUnit)
                .Include(sc => sc.Station).ThenInclude(es => es.Region).ThenInclude(r => r.Country)
                .ToListAsync();
            return items.Select(i => i.ToFreightCustomerInfo());
        }
        return Array.Empty<FreightCustomerInfo>();
    }

    public async Task<(int Count, string Message, StationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, int stationId, StationCustomer entity, ModuleOwnershipRef ownerRef)
    {
        if (principal.IsAuthenticated())
        {
            entity.StationId = stationId;
            entity.TrackOrAreaColor = entity.TrackOrAreaColor?.ToLowerInvariant();
            if (ownerRef.IsGroup)
            {
                using var dbContext = Factory.CreateDbContext();
                var isDataAdministrator = await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.IsDataAdministrator && gm.GroupId == ownerRef.GroupId && gm.PersonId == principal.PersonId());
                if (isDataAdministrator)
                {
                    return await AddOrUpdate(dbContext, principal, entity);
                }
            }
            else if (principal.MaySave(ownerRef))
            {
                using var dbContext = Factory.CreateDbContext();
                return await AddOrUpdate(dbContext, principal, entity);
            }
        }
        return principal.SaveNotAuthorised<StationCustomer>();

        static async Task<(int Count, string Message, StationCustomer? Entity)> AddOrUpdate(ModulesDbContext dbContext, ClaimsPrincipal? principal, StationCustomer entity)
        {
            var station = await dbContext.Stations.FindAsync(entity.StationId);
            if (station is null) return principal.SaveNotAuthorised<StationCustomer>();
            var existing = dbContext.StationCustomers.Include(sc => sc.Cargos).SingleOrDefault(sc => sc.Id == entity.Id);
            return existing is null ?
                await AddNew(dbContext, principal, station, entity) :
                await UpdateExisting(dbContext, principal, station, entity, existing);
        }

        static async Task<(int Count, string Message, StationCustomer? Entity)> AddNew(ModulesDbContext dbContext, ClaimsPrincipal? principal, Station station, StationCustomer entity)
        {
            station.StationCustomers.Add(entity);
            var result = await dbContext.SaveChangesAsync();
            return result.SaveResult(entity);
        }

        static async Task<(int Count, string Message, StationCustomer? Entity)> UpdateExisting(ModulesDbContext dbContext, ClaimsPrincipal? principal, Station station, StationCustomer entity, StationCustomer existing)
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            AddOrRemoveCustomerCargos(dbContext, entity, existing);
            if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.SaveResult(existing);
        }

        static void AddOrRemoveCustomerCargos(ModulesDbContext dbContext, StationCustomer entity, StationCustomer existing)
        {
            foreach (var cargo in entity.Cargos)
            {
                cargo.TrackOrAreaColor = cargo.TrackOrAreaColor?.ToLowerInvariant();
                var current = existing.Cargos.AsQueryable().FirstOrDefault(scc => scc.Id == cargo.Id);
                if (current is null) existing.Cargos.Add(cargo);
                else dbContext.Entry(current).CurrentValues.SetValues(cargo);
            }
            foreach (var cargo in existing.Cargos) if (!entity.Cargos.Any(st => st.Id == cargo.Id)) dbContext.Remove(cargo);
        }

        static bool IsUnchanged(ModulesDbContext dbContext, StationCustomer customer) =>
            dbContext.Entry(customer).State == EntityState.Unchanged &&
            customer.Cargos.All(scc => dbContext.Entry(scc).State == EntityState.Unchanged);
    }

    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int customerId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.StationCustomers.Include(sc => sc.Cargos).SingleOrDefaultAsync(sc => sc.Id == customerId);
            if (existing is not null)
            {
                foreach (var cargoflow in existing.Cargos) dbContext.StationCustomerCargos.Remove(cargoflow);
                dbContext.StationCustomers.Remove(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.DeleteResult();
            }
            return Strings.NothingToDelete.DeleteResult();
        }
        return Strings.NotAuthorised.DeleteResult();

    }
}
