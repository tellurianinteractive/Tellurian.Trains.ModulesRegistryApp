namespace ModulesRegistry.Services.Implementations;
public class StationCustomerWaybillsService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public StationCustomerWaybillsService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public async Task<IEnumerable<StationCustomerWaybill>> GetStationCustomerWaybillsAsync(ClaimsPrincipal? principal, int stationCustomerId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomerWaybills
                .Where(x => x.StationCustomerId == stationCustomerId)
                .Include(w => w.StationCustomer)
                .Include(w => w.StationCustomerCargo).ThenInclude(c => c.Direction)
                .Include(w => w.OperatingDay)
                .Include(w => w.OtherStationCustomerCargo).ThenInclude(c => c.Cargo)
                .Include(w => w.OtherStationCustomerCargo).ThenInclude(c => c.StationCustomer).ThenInclude(c => c.Station)
                .Include(w => w.OtherExternalCustomerCargo).ThenInclude(c => c.Cargo)
                .Include(w => w.OtherExternalCustomerCargo).ThenInclude(c => c.ExternalStationCustomer).ThenInclude(c => c.ExternalStation)
                .Include(w => w.OtherRegion).ThenInclude(r => r.Country)
                .ToReadOnlyListAsync();

        }
        return Enumerable.Empty<StationCustomerWaybill>();
    }

    public async Task<int> AddGeneratedCustomerWaybills(ClaimsPrincipal? principal, int stationCustomerId)
    {
        var result = 0;
        result += await AddGeneratedModuleCustomerWaybills(principal, stationCustomerId);
        result += await AddGeneratedExternaalCustomerWaybills(principal, stationCustomerId);
        return result;
    }

    public async Task<int> AddGeneratedModuleCustomerWaybills(ClaimsPrincipal? principal, int stationCustomerId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Database.ExecuteSqlInterpolatedAsync($"EXEC [AddGeneratedModuleWaybills] @StationCustomerId={stationCustomerId}");
        }
        return 0;
    }

    public async Task<int> AddGeneratedExternaalCustomerWaybills(ClaimsPrincipal? principal, int stationCustomerId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Database.ExecuteSqlInterpolatedAsync($"EXEC [AddGeneratedExternalWaybills] @StationCustomerId={stationCustomerId}");
        }
        return 0;
    }

    public async Task<(int Count, string Message, StationCustomerWaybill? Entity)> SaveAsync(ClaimsPrincipal? principal, StationCustomerWaybill entity)
    {
        if (principal.IsAuthenticated())
        {
            var dbContext = Factory.CreateDbContext();
            var existing = dbContext.StationCustomerWaybills.FirstOrDefault(x => x.Id == entity.Id);
            if (existing is null) return principal.SaveNotAuthorised<StationCustomerWaybill>();
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing);

        }
        return principal.SaveNotAuthorised<StationCustomerWaybill>();
    }

}
