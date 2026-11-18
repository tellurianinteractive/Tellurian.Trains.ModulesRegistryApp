namespace ModulesRegistry.Services.Implementations;
public class StationCustomerWaybillsService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<StationCustomerWaybill>> GetStationCustomerWaybillsAsync(ClaimsPrincipal? principal, int stationCustomerId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.StationCustomerWaybills
                .Where(x => x.StationCustomerId == stationCustomerId)
                .Include(w => w.StationCustomer)
                .Include(w => w.StationCustomerCargo).ThenInclude(c => c.Direction)
                .Include(w => w.StationCustomerCargo).ThenInclude(c => c.Cargo)
                .Include(w => w.StationCustomerCargo).ThenInclude(c => c.QuantityUnit)
                .Include(w => w.StationCustomerCargo).ThenInclude(c => c.PackageUnit)
                .Include(w => w.OperatingDay)
                .Include(w => w.OtherStationCustomerCargo).ThenInclude(c => c.StationCustomer).ThenInclude(c => c.Station)
                .Include(w => w.OtherExternalCustomerCargo).ThenInclude(c => c.ExternalStationCustomer).ThenInclude(c => c.ExternalStation)
                .Include(w => w.OtherRegion).ThenInclude(r => r.Country)
                .Include(w =>w.OtherRegion).ThenInclude(r => r.RepresentativeExternalStation)
                .ToReadOnlyListAsync();

        }
        return Enumerable.Empty<StationCustomerWaybill>();
    }

    public async Task<int> AddGeneratedCustomerWaybills(ClaimsPrincipal? principal, int stationCustomerId)
    {
        var result = 0;
        result += await AddGeneratedModuleCustomerWaybills(principal, stationCustomerId);
        result += await AddGeneratedExternalCustomerWaybills(principal, stationCustomerId);
        result += await AddGeneratedShadowYardCustomerWaybills(principal, stationCustomerId);
        return result;
    }

    public async Task<int> AddGeneratedModuleCustomerWaybills(ClaimsPrincipal? principal, int stationCustomerId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationOfCustomerAsync(principal, stationCustomerId)) return 0;
        return await dbContext.Database.ExecuteSqlAsync($"EXEC [AddGeneratedModuleWaybills] @StationCustomerId={stationCustomerId}");
    }

    public async Task<int> AddGeneratedExternalCustomerWaybills(ClaimsPrincipal? principal, int stationCustomerId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationOfCustomerAsync(principal, stationCustomerId)) return 0;
        return await dbContext.Database.ExecuteSqlAsync($"EXEC [AddGeneratedExternalWaybills] @StationCustomerId={stationCustomerId}");
    }
    public async Task<int> AddGeneratedShadowYardCustomerWaybills(ClaimsPrincipal? principal, int stationCustomerId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationOfCustomerAsync(principal, stationCustomerId)) return 0;
        return await dbContext.Database.ExecuteSqlAsync($"EXEC [AddGeneratedShadowYardWaybills] @StationCustomerId={stationCustomerId}");
    }

    public async Task<(int Count, string Message, StationCustomerWaybill? Entity)> SaveAsync(ClaimsPrincipal? principal, StationCustomerWaybill entity)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.StationCustomerWaybills.FirstOrDefaultAsync(x => x.Id == entity.Id).ConfigureAwait(false);
        if (existing is null || !await dbContext.MayEditStationOfCustomerAsync(principal, existing.StationCustomerId).ConfigureAwait(false))
            return principal.SaveNotAuthorised<StationCustomerWaybill>();
        // A waybill stays with its customer and its cargo flow.
        entity.StationCustomerId = existing.StationCustomerId;
        entity.StationCustomerCargoId = existing.StationCustomerCargoId;
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
        return result.SaveResult(existing);
    }

}
