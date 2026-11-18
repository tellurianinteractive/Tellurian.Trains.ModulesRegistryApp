using ModulesRegistry.Resources;
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

    /// <summary>Whether the user may change the station's freight customers and their waybills.</summary>
    public async Task<bool> MayEditAsync(ClaimsPrincipal? principal, int stationId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayEditStationAsync(principal, stationId);
    }

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

    /// <summary>
    /// Adds or updates a freight customer of the station, if the user may edit the station. Saving an existing customer
    /// with another station moves it there (a correction when it was added to the wrong station), which also requires
    /// that the user may edit the station it is moved from.
    /// </summary>
    public async Task<(int Count, string Message, StationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, int stationId, StationCustomer entity)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationAsync(principal, stationId)) return principal.SaveNotAuthorised<StationCustomer>();
        var station = await dbContext.Stations.FindAsync(stationId);
        if (station is null) return principal.SaveNotAuthorised<StationCustomer>();
        var existing = await dbContext.StationCustomers.Include(sc => sc.Cargos).SingleOrDefaultAsync(sc => sc.Id == entity.Id);
        if (existing is not null && existing.StationId != stationId && !await dbContext.MayEditStationAsync(principal, existing.StationId))
            return principal.SaveNotAuthorised<StationCustomer>();

        entity.StationId = stationId;
        entity.TrackOrAreaColor = entity.TrackOrAreaColor?.ToLowerInvariant();
        entity.TrackOrArea = entity.TrackOrArea.ValueOrNull();
        if (entity.Cargos is not null)
        {
            foreach (var cargo in entity.Cargos)
            {
                // Only the customer's own cargo flows are updated; any other id is a new cargo flow.
                cargo.Id = existing?.Cargos.Any(c => c.Id == cargo.Id) == true ? cargo.Id : 0;
                cargo.TrackOrArea = cargo.TrackOrArea.ValueOrNull();
                cargo.SpecialCargoName = cargo.SpecialCargoName.ValueOrNull();
                cargo.SpecificWagonClass = cargo.SpecificWagonClass.ValueOrNull();
            }
        }
        return existing is null ?
            await AddNew(dbContext, principal, station, entity) :
            await UpdateExisting(dbContext, principal, station, entity, existing);

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
            if (existing is null) return Strings.NothingToDelete.DeleteResult();
            if (!await dbContext.MayEditStationAsync(principal, existing.StationId)) return Strings.NotAuthorised.DeleteResult();
            foreach (var cargoflow in existing.Cargos) dbContext.StationCustomerCargos.Remove(cargoflow);
            dbContext.StationCustomers.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return Strings.NotAuthorised.DeleteResult();
    }
}