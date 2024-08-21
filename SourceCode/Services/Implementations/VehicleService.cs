namespace ModulesRegistry.Services.Implementations;

public sealed class VehicleService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<Vehicle>> GetVehiclesByOwnerCountryAsync(ClaimsPrincipal? principal, int countryId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Vehicles
                .Include(v => v.OwningPerson)
                .Include(v => v.CouplingFeature)
                .Include(v => v.TractionFeature)
                .Include(v => v.WheelsFeature)
                .Include(v => v.Scale)
                .Where(v => v.OwningPerson.CountryId == countryId)
                .OrderBy(v => v.OwningPerson.FirstName).ThenBy(v => v.OwningPerson.LastName).ThenBy(v => v.KeeperSignature).ThenBy(v => v.VehicleClass).ThenBy(v => v.VehicleNumber)
                .ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<IEnumerable<Vehicle>> GetPersonsOwnedVehiclesAsync(ClaimsPrincipal? principal, int? maybeOwningPersonId)
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
                .Where(v => v.OwningPersonId == personId)
                .OrderBy(v => v.KeeperSignature).ThenBy(v => v.VehicleClass).ThenBy(v => v.VehicleNumber)
                .ToReadOnlyListAsync();
        }
        return Enumerable.Empty<Vehicle>().AsQueryable();
    }

    public async Task<Vehicle?> GetVehicleAsync(ClaimsPrincipal? principal, int vehicleId = 0, int countryId = 0)
    {
        if (countryId > 0) return await GetVehicleAsAdministratorAsync(principal, vehicleId, countryId).ConfigureAwait(false);
        if (vehicleId > 0) return await GetPrincipalOwnedVehicle(principal, vehicleId).ConfigureAwait(false);
        return null;
    }

    private async Task<Vehicle?> GetVehicleAsAdministratorAsync(ClaimsPrincipal? principal, int vehicleId, int countryId)
    {
        if (principal.IsCountryAdministratorInCountry(countryId))
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Vehicles
                .Where(v => v.Id == vehicleId)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }
        return null;
    }
    private async Task<Vehicle?> GetPrincipalOwnedVehicle(ClaimsPrincipal? principal, int vehicleId)
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
        int result;
        if (existing is null)
        {
            dbContext.Add(entity);
            result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        else
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        var vehicle = existing ?? entity;
        await AddPrototypeLengthForSameKeeperAndClass(vehicle);
        await AddPrototypeWeightForSameKeeperAndClass(vehicle);
        await AddKeeperCountryForSameKeeper(vehicle);
        await AddEnginePowerForSameKeeperAndClass(vehicle);
        return result.SaveResult(vehicle);
    }

    public async Task<int> AddPrototypeLengthForSameKeeperAndClass(Vehicle vehicle)
    {
        if (vehicle.PrototypeLength == 0) return 0; 
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Vehicles
            .Where(v => v.OwningPersonId == vehicle.OwningPersonId && v.PrototypeLength == null &&
                    vehicle.KeeperSignature.HasValue() && v.KeeperSignature == vehicle.KeeperSignature && vehicle.VehicleClass.HasValue() && v.VehicleClass == vehicle.VehicleClass)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.PrototypeLength, vehicle.PrototypeLength));
    }
    public async Task<int> AddPrototypeWeightForSameKeeperAndClass(Vehicle vehicle)
    {
        if (vehicle.PrototypeWeight == null) return 0;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Vehicles
            .Where(v => v.OwningPersonId == vehicle.OwningPersonId && v.PrototypeWeight == null &&
                    vehicle.KeeperSignature.HasValue() && v.KeeperSignature == vehicle.KeeperSignature && vehicle.VehicleClass.HasValue() && v.VehicleClass == vehicle.VehicleClass)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.PrototypeWeight, vehicle.PrototypeWeight));
    }

    public async Task<int> AddEnginePowerForSameKeeperAndClass(Vehicle vehicle)
    {
        if (vehicle.EnginePower == null) return 0;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Vehicles
            .Where(v => v.OwningPersonId == vehicle.OwningPersonId && v.EnginePower == null &&
                    vehicle.KeeperSignature.HasValue() && v.KeeperSignature == vehicle.KeeperSignature && vehicle.VehicleClass.HasValue() && v.VehicleClass == vehicle.VehicleClass)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.EnginePower, vehicle.EnginePower));
    }

    public async Task<int> AddKeeperCountryForSameKeeper(Vehicle vehicle)
    {
        if (vehicle.KeeperCountryId == null || vehicle.KeeperCountryId.Value == 0) return 0;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Vehicles
            .Where(v => v.OwningPersonId == vehicle.OwningPersonId && v.KeeperCountryId == null &&
                    vehicle.KeeperSignature.HasValue() && v.KeeperSignature == vehicle.KeeperSignature)
            .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.KeeperCountryId, vehicle.KeeperCountryId));
    }
}
