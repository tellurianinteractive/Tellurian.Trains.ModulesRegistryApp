using ModulesRegistry.Services.Projections;

namespace ModulesRegistry.Services.Implementations;

public sealed class ExternalStationService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int? countryId, int? regionId)
    {
        if (principal is null) return [];
        var actualCountryId = principal.CountryOrDefaultId(countryId);
        using var dbContext = Factory.CreateDbContext();
        var stations = dbContext.ExternalStations.AsNoTracking();
        if (regionId > 0) stations = stations.Where(s => s.RegionId == regionId.Value);
        else if (actualCountryId > 0) stations = stations.Where(s => s.Region.CountryId == actualCountryId);
        // The same items as the former view ListExternalStation: the station's name, region and country domain, in the region's colours.
        return await stations
            .Select(s => new { s.Id, Description = s.FullName + ", " + s.Region.LocalName + " (" + s.Region.Country.DomainSuffix + ")", s.Region.BackColor, s.Region.ForeColor })
            .OrderBy(s => s.Description)
            .Select(s => new ListboxItem(s.Id, s.Description) { BackColor = s.BackColor, ForeColor = s.ForeColor })
            .ToListAsync();
    }

    public async Task<IEnumerable<ExternalStation>> GetAsync(ClaimsPrincipal? principal, int countryId = 0, int regionId = 0)
    {
        if (principal.IsAuthenticated())
        {
            countryId = countryId > 0 ? countryId : principal.CountryId();
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ExternalStations.AsNoTracking()
                .Where(es => countryId == 0 || es.Region.CountryId == countryId && regionId == 0 || (countryId == 0 && es.RegionId == regionId))
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
                .Include(es => es.Region)
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
            // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5); the signature by the country's language.
            entity.CorrectInput(await dbContext.CountryCasingsAsync(entity.RegionId, principal));
            var existing = await dbContext.ExternalStations
                .Include(es => es.Region)
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
                var regionId = existing.RegionId;
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
                if (existing.RegionId != regionId) { return (0, Resources.Strings.NotAuthorized, entity); }
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing);
            }
        }
        return principal.SaveNotAuthorised<ExternalStation>();
    }

    /// <summary>
    /// Deletes an external station, if the user is a country administrator in the station's country or a global administrator.
    /// </summary>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (!principal.IsAuthenticated()) return principal.NotAuthorized<ExternalStation>();
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.ExternalStations.Include(es => es.Region).SingleOrDefaultAsync(es => es.Id == id);
        if (existing is null) return (0, Resources.Strings.NothingToDelete);
        if (!principal.IsCountryAdministratorInCountry(existing.Region.CountryId)) return principal.NotAuthorized<ExternalStation>();
        dbContext.Remove(existing);
        var result = await dbContext.SaveChangesAsync();
        return result.DeleteResult();
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
                .Where(esc => esc.ExternalStationId == stationId && (customerId == 0 || esc.Id == customerId))
                .ToListAsync();
        }
        return Array.Empty<ExternalStationCustomer>();
    }

    public async Task<(int Count, string Message, ExternalStationCustomer? Entity)> SaveAsync(ClaimsPrincipal? principal, ExternalStationCustomer entity)
    {
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
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

    /// <summary>
    /// Saves one cargo flow of a customer (Migration step 3.6: each flow has its own form). An edited flow must belong to the
    /// customer the page is for (<paramref name="customerId"/>); a new flow is added to that customer.
    /// </summary>
    public async Task<(int Count, string Message, ExternalStationCustomerCargo? Entity)> SaveCargoAsync(ClaimsPrincipal? principal, int customerId, ExternalStationCustomerCargo entity)
    {
        if (!principal.IsAuthenticated()) return principal.SaveNotAuthorised<ExternalStationCustomerCargo>();
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.ExternalStationCustomers.AnyAsync(esc => esc.Id == customerId)) return (0, Resources.Strings.SaveFailed, null);
        if (entity.Id == 0)
        {
            entity.ExternalStationCustomerId = customerId;
            dbContext.ExternalStationCustomerCargos.Add(entity);
            return (await dbContext.SaveChangesAsync()).SaveResult(entity);
        }
        var existing = await dbContext.ExternalStationCustomerCargos.SingleOrDefaultAsync(escc => escc.Id == entity.Id && escc.ExternalStationCustomerId == customerId);
        if (existing is null) return (0, Resources.Strings.SaveFailed, null);
        entity.ExternalStationCustomerId = customerId;
        dbContext.Entry(existing).CurrentValues.SetValues(entity);
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        return (await dbContext.SaveChangesAsync()).SaveResult(existing);
    }

    /// <summary>Deletes one cargo flow, only of the customer the page is for.</summary>
    public async Task<(int Count, string Message)> DeleteCargoAsync(ClaimsPrincipal? principal, int customerId, int cargoId)
    {
        if (!principal.IsAuthenticated()) return principal.NotAuthorized<ExternalStationCustomerCargo>();
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.ExternalStationCustomerCargos.SingleOrDefaultAsync(escc => escc.Id == cargoId && escc.ExternalStationCustomerId == customerId);
        if (existing is null) return (0, Resources.Strings.NothingToDelete);
        dbContext.Remove(existing);
        return (await dbContext.SaveChangesAsync()).DeleteResult();
    }

    /// <summary>
    /// Moves a customer to another external station; only the super user and global administrators may.
    /// </summary>
    public async Task<(int Count, string Message)> MoveCustomerAsync(ClaimsPrincipal? principal, ExternalStationCustomer? customer, int toStationId)
    {
        if (!principal.MayMoveExternalStationCustomers()) return principal.NotAuthorized<ExternalStationCustomer>();
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.ExternalStations.AnyAsync(es => es.Id == toStationId)) return principal.NotFoundResult<ExternalStationCustomer>();
        var existing = await dbContext.ExternalStationCustomers.SingleOrDefaultAsync(c => c.Id == customer!.Id);
        if (existing is null) return principal.NotFoundResult<ExternalStationCustomer>();
        existing.ExternalStationId = toStationId;
        var result = await dbContext.SaveChangesAsync();
        return result.UpdateResult();
    }
    #endregion
}
