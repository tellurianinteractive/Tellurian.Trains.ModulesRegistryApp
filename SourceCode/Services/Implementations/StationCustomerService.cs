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
    /// Adds or updates a freight customer of the station, if the user may edit the station. Only the customer's own fields are
    /// saved; each cargo flow has its own form (<see cref="SaveCargoAsync"/>, Migration step 3.6). Saving an existing customer
    /// with another station moves it there, which also requires that the user may edit the station it is moved from
    /// (<see cref="MoveCustomerAsync"/>).
    /// </summary>
    public async Task<(int Count, string Message, StationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, int stationId, StationCustomer entity)
    {
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationAsync(principal, stationId)) return principal.SaveNotAuthorised<StationCustomer>();
        var existing = entity.Id > 0 ? await dbContext.StationCustomers.SingleOrDefaultAsync(sc => sc.Id == entity.Id) : null;
        if (entity.Id > 0 && existing is null) return principal.SaveNotAuthorised<StationCustomer>();
        if (existing is not null && existing.StationId != stationId && !await dbContext.MayEditStationAsync(principal, existing.StationId))
            return principal.SaveNotAuthorised<StationCustomer>();

        entity.StationId = stationId;
        if (existing is null)
        {
            dbContext.StationCustomers.Add(entity);
            return (await dbContext.SaveChangesAsync()).SaveResult(entity);
        }
        var layoutId = existing.LayoutId; // Not a field of the form.
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        existing.LayoutId = layoutId;
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        return (await dbContext.SaveChangesAsync()).SaveResult(existing);
    }

    /// <summary>
    /// Moves a customer, with its cargo flows and waybills, to another station: a correction when it was added to the wrong
    /// station. The user must be allowed to edit both stations.
    /// </summary>
    public async Task<(int Count, string Message)> MoveCustomerAsync(ClaimsPrincipal? principal, int customerId, int toStationId)
    {
        using var dbContext = Factory.CreateDbContext();
        var customer = await dbContext.StationCustomers.SingleOrDefaultAsync(sc => sc.Id == customerId);
        if (customer is null) return principal.NotFoundResult<StationCustomer>();
        if (customer.StationId == toStationId) return (-1, Strings.NoModification);
        if (!await dbContext.MayEditStationAsync(principal, customer.StationId) || !await dbContext.MayEditStationAsync(principal, toStationId))
            return principal.NotAuthorized<StationCustomer>();
        customer.StationId = toStationId;
        return (await dbContext.SaveChangesAsync()).UpdateResult();
    }

    /// <summary>The stations of a country, for an administrator to move a customer to (<see cref="MoveCustomerAsync"/>).</summary>
    public async Task<IEnumerable<ListboxItem>> MoveToStationItemsAsync(ClaimsPrincipal? principal, int countryId)
    {
        if (!principal.IsGlobalOrCountryAdministrator(countryId)) return [];
        using var dbContext = Factory.CreateDbContext();
        var items = await dbContext.Stations.AsNoTracking()
            .Where(s => s.Region.CountryId == countryId)
            .Select(s => new ListboxItem(s.Id, s.FullName + " (" + s.Signature + ")"))
            .ToListAsync();
        return [.. items.OrderBy(i => i.Description)];
    }

    /// <summary>
    /// Saves one cargo flow of a customer (Migration step 3.6: each flow has its own form), if the user may edit the customer's
    /// station. An edited flow must belong to the customer the page is for (<paramref name="customerId"/>); a new flow is added
    /// to that customer.
    /// </summary>
    public async Task<(int Count, string Message, StationCustomerCargo? Entity)> SaveCargoAsync(ClaimsPrincipal? principal, int customerId, StationCustomerCargo entity)
    {
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationOfCustomerAsync(principal, customerId)) return principal.SaveNotAuthorised<StationCustomerCargo>();
        entity.StationCustomerId = customerId;
        if (entity.Id == 0)
        {
            dbContext.StationCustomerCargos.Add(entity);
            return (await dbContext.SaveChangesAsync()).SaveResult(entity);
        }
        var existing = await dbContext.StationCustomerCargos.SingleOrDefaultAsync(scc => scc.Id == entity.Id && scc.StationCustomerId == customerId);
        if (existing is null) return (0, Strings.SaveFailed, null);
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        return (await dbContext.SaveChangesAsync()).SaveResult(existing);
    }

    /// <summary>
    /// Deletes one cargo flow, only of the customer the page is for, if the user may edit its station. The database deletes the
    /// flow's waybills (trigger <c>DeleteStationCustomerCargo</c>).
    /// </summary>
    public async Task<(int Count, string Message)> DeleteCargoAsync(ClaimsPrincipal? principal, int customerId, int cargoId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationOfCustomerAsync(principal, customerId)) return Strings.NotAuthorised.DeleteResult();
        var existing = await dbContext.StationCustomerCargos.SingleOrDefaultAsync(scc => scc.Id == cargoId && scc.StationCustomerId == customerId);
        if (existing is null) return Strings.NothingToDelete.DeleteResult();
        dbContext.Remove(existing);
        return (await dbContext.SaveChangesAsync()).DeleteResult();
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