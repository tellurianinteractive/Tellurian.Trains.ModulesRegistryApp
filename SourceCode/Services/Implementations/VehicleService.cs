namespace ModulesRegistry.Services.Implementations;

public sealed class VehicleService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;


    public async Task<Vehicle?> GetPrincipalOwnedVehicle(ClaimsPrincipal? principal, int vehicleId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Vehicles
                .Where(v => v.Id == vehicleId && v.OwningPersonId == principal.PersonId())
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<IEnumerable<Vehicle>?> GetPersonsOwnedVehiclesAsync(ClaimsPrincipal? principal, int? maybeOwningPersonId)
    {
        if (principal.IsAuthenticated())
        {
            var personId = maybeOwningPersonId.HasValue && maybeOwningPersonId.Value > 0 ? maybeOwningPersonId.Value : principal.PersonId();
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Vehicles
                .Include(v => v.CouplingFeature)
                .Include(v => v.TractionFeature)
                .Include(v => v.WheelsFeature)
                .Include(v => v.Scale)
                .Where(v => v.OwningPersonId == personId).ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<IEnumerable<ListboxItem>> GetTractionFeaturesListboxDataAsync()
    {
        using var dbContext = Factory.CreateDbContext();
        var items = await GetVehicleFeaturesDataAsync("TRACTION", 0);
        return items.Select(vf => AsListbox(vf)).OrderBy(lb => lb.Description);
    }

    public async Task<IEnumerable<ListboxItem>> GetCouplingFeaturesListboxDataAsync(int scaleId)
    {
        using var dbContext = Factory.CreateDbContext();
        var items = await GetVehicleFeaturesDataAsync("COUPLINGS", scaleId);
        return items.Select(vf => AsListbox(vf)).OrderBy(lb => lb.Description);
    }
    public async Task<IEnumerable<ListboxItem>> GetWheelsFeaturesListboxDataAsync(int scaleId)
    {
        using var dbContext = Factory.CreateDbContext();
        var items = await GetVehicleFeaturesDataAsync("WHEELS", scaleId);
        return items.Select(vf => AsListbox(vf)).OrderBy(lb => lb.Description);
    }

    private ListboxItem AsListbox(VehicleFeature vf) =>
        new(vf.Id, vf.IsResourceCode ? LanguageExtensions.GetLocalizedString(vf.Description) : vf.Description);

    private async Task<IEnumerable<VehicleFeature>> GetVehicleFeaturesDataAsync(string category, int scaleId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.VehicleFeatures
            .Where(vf => vf.Category == category && (scaleId == 0 || !vf.OnlyScaleId.HasValue || vf.OnlyScaleId.Value == scaleId))
            .ToReadOnlyListAsync();
    }

    public async Task<(int Count, string Message, Vehicle? Entity)> SaveAsync(ClaimsPrincipal? principal, Vehicle entity)
    {
        if (!principal.IsAuthenticated()) return principal.SaveNotAuthorised<Vehicle>();

        entity.FormatData();
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.Vehicles.FindAsync(entity.Id).ConfigureAwait(false);
        if (existing is null)
        {
            dbContext.Add(entity);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(entity);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing);
        }
    }
}
