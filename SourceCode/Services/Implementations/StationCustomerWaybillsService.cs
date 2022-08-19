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
                .Include(w => w.StationCustomerCargo).ThenInclude(c => c.Direction)
                .Include(w => w.OperatingDay)
                .Include(w => w.OtherCustomerCargo).ThenInclude(c => c.Cargo)
                .Include(w => w.OtherCustomerCargo).ThenInclude(c => c.StationCustomer).ThenInclude(c => c.Station)
                .Include(w => w.OtherRegion).ThenInclude(r => r.Country)
                .ToReadOnlyListAsync();

        }
        return Enumerable.Empty<StationCustomerWaybill>();
    }

}
