using ModulesRegistry.Data.Resources;
using ModulesRegistry.Services.Projections;

namespace ModulesRegistry.Services.Implementations;

public class StationCustomerService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<StationCustomer>> AllAsync(ClaimsPrincipal? principal, int stationId, int customerId = 0)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomers
                .Include(sc => sc.Cargos).ThenInclude(scc => scc.Cargo)
                .Where(sc => sc.StationId == stationId && (customerId == 0 || sc.Id == customerId))
                .ToReadOnlyListAsync();
        }
        return Array.Empty<StationCustomer>();
    }

    // SELECT * FROM ModuleCustomerCargo WHERE CountryId = 1 AND CargoId = 4 AND MainTheme = 'EUROPE' ORDER BY StationName
    public async Task<IEnumerable<StationCustomerCargo>> GetGetCustomerCargoAsync(ClaimsPrincipal? principal, int cargoId, string mainTheme = "EUROPE", int countryId = 0 )
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomerCargos
                .Where(scc => scc.CargoId == cargoId && scc.StationCustomer.Station.PrimaryModule.Standard.MainTheme == mainTheme && (countryId == 0 || scc.StationCustomer.Station.Region.CountryId == countryId))
                .Include(c => c.Cargo)
                .Include(scc => scc.Direction)
                .Include(scc => scc.QuantityUnit)
                .Include(sc => sc.StationCustomer)
                    .ThenInclude(s => s.Station)
                    .ThenInclude(s => s.Region)
                    .ThenInclude(r => r.Country)
                .ToReadOnlyListAsync();
        }
        return Array.Empty<StationCustomerCargo>();
    }


    public async Task<StationCustomer?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomers.AsNoTracking()
                .Include(sc => sc.Cargos).ThenInclude(c => c.Cargo)
                .ReadOnlySingleOrDefaultAsync(sc => sc.Id == id);
        }
        return null;
    }

    public Task<(int Count, string Message, StationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, int stationId, StationCustomer entity) =>
        SaveAsync(principal, stationId, entity, principal.AsModuleOwnershipRef());

    public async Task<IEnumerable<FreightCustomerInfo>> GetCustomersAsync(ClaimsPrincipal? principal, int? maybeCountryId)
    {
        if (principal.IsAuthenticated())
        {
            var countryId = maybeCountryId ?? principal.CountryId();
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.StationCustomers
                .Where(sc => (countryId == 0 || sc.Station.Region.CountryId == countryId))
                .OrderBy(sc => sc.Station.FullName).ThenBy(esc => esc.CustomerName)
                .Include(sc => sc.Cargos).ThenInclude(escc => escc.Direction)
                .Include(sc => sc.Cargos).ThenInclude(escc => escc.Cargo)
                .Include(sc => sc.Cargos).ThenInclude(escc => escc.QuantityUnit)
                .Include(sc => sc.Station).ThenInclude(es => es.Region).ThenInclude(r => r.Country)
                .ToReadOnlyListAsync();
            return items.Select(i => i.ToFreightCustomerInfo());
        }
        return Array.Empty<FreightCustomerInfo>();
    }

    public async Task<IEnumerable<StationCustomerWaybill>> GetCustomerWaybills(ClaimsPrincipal? principal, int customerId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomerWaybills
                .Where(s => s.StationCustomerId == customerId)
                .Include(s => s.OtherRegion)
                .Include(s => s.OperatingDay)
                .Include(s => s.StationCustomerCargo)
                .Include(s => s.OtherStationCustomerCargo)
                .ToReadOnlyListAsync();
            ;

        }
        return Array.Empty<StationCustomerWaybill>();
    }

    public async Task<(int Count, string Message, StationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, int stationId, StationCustomer entity, ModuleOwnershipRef ownerRef)
    {
        if (principal.IsAuthenticated())
        {
            entity.StationId = stationId;
            entity.TrackOrAreaColor = entity.TrackOrAreaColor?.ToLowerInvariant();
            entity.TrackOrArea = entity.TrackOrArea.ValueOrNull();
            if (entity.Cargos is not null)
            {
                foreach (var cargo in entity.Cargos)
                {
                    cargo.Id = cargo.Id < 0 ? 0 : cargo.Id;
                    cargo.TrackOrArea = cargo.TrackOrArea.ValueOrNull();
                    cargo.SpecialCargoName = cargo.SpecialCargoName.ValueOrNull();
                    cargo.SpecificWagonClass = cargo.SpecificWagonClass.ValueOrNull();
                }
            }
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
            foreach (var cargo in entity.Cargos.Where(c => c.IsValid()))
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
