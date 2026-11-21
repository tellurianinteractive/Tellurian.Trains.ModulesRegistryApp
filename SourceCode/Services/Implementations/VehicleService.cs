using System.Linq.Expressions;

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
                .Where(v => v.Id == vehicleId && v.OwningPerson.CountryId == countryId) // Only vehicles of owners in the administrator's country.
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

    /// <summary>
    /// Saves a vehicle. The owner saves their own vehicles, and a country administrator those of owners in the administrator's country;
    /// a new vehicle is always the user's own. The owner is never changed here. A coupling or wheels feature that is not for the
    /// vehicle's scale (or not of its kind) is removed, since the lists depend on the scale (Migration step 3.6).
    /// </summary>
    public async Task<(int Count, string Message, Vehicle? Entity)> SaveAsync(ClaimsPrincipal? principal, Vehicle entity)
    {
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        if (!principal.IsAuthenticated()) return principal.SaveNotAuthorised<Vehicle>();

        using var dbContext = Factory.CreateDbContext();
        var existing = entity.Id > 0 ? await dbContext.Vehicles.Include(v => v.OwningPerson).SingleOrDefaultAsync(v => v.Id == entity.Id).ConfigureAwait(false) : null;
        if (entity.Id > 0 && existing is null) return principal.SaveNotAuthorised<Vehicle>();
        if (existing is null ? entity.OwningPersonId != principal.PersonId() : !MaySave(principal, existing)) return principal.SaveNotAuthorised<Vehicle>();
        if (existing is not null) entity.OwningPersonId = existing.OwningPersonId;
        entity.CouplingFeatureId = await FeatureForScaleOrNullAsync(dbContext, "COUPLINGS", entity.CouplingFeatureId, entity.ScaleId);
        entity.WheelsFeatureId = await FeatureForScaleOrNullAsync(dbContext, "WHEELS", entity.WheelsFeatureId, entity.ScaleId);
        entity.TractionFeatureId = await FeatureForScaleOrNullAsync(dbContext, "TRACTION", entity.TractionFeatureId, entity.ScaleId);

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

    private static bool MaySave(ClaimsPrincipal principal, Vehicle existing) =>
        existing.OwningPersonId == principal.PersonId() || principal.IsCountryAdministratorInCountry(existing.OwningPerson.CountryId);

    private static async Task<int?> FeatureForScaleOrNullAsync(ModulesDbContext dbContext, string category, int? featureId, int scaleId) =>
        featureId is null ? null :
        await dbContext.VehicleFeatures.AnyAsync(vf => vf.Id == featureId && vf.Category == category && (vf.OnlyScaleId == null || vf.OnlyScaleId == scaleId)) ? featureId : null;

    public async Task<int> AddPrototypeLengthForSameKeeperAndClass(Vehicle vehicle)
    {
        // The captured vehicle is checked before the query: extension properties can't be in a query (CS9296).
        if (vehicle.PrototypeLength is null or 0 || vehicle.KeeperSignature.IsEmpty || vehicle.VehicleClass.IsEmpty) return 0;
        return await FillInForSameKeeperAsync(
            v => v.OwningPersonId == vehicle.OwningPersonId && v.PrototypeLength == null &&
                v.KeeperSignature == vehicle.KeeperSignature && v.VehicleClass == vehicle.VehicleClass,
            v => v.PrototypeLength = vehicle.PrototypeLength);
    }

    public async Task<int> AddPrototypeWeightForSameKeeperAndClass(Vehicle vehicle)
    {
        if (vehicle.PrototypeWeight == null || vehicle.KeeperSignature.IsEmpty || vehicle.VehicleClass.IsEmpty) return 0;
        return await FillInForSameKeeperAsync(
            v => v.OwningPersonId == vehicle.OwningPersonId && v.PrototypeWeight == null &&
                v.KeeperSignature == vehicle.KeeperSignature && v.VehicleClass == vehicle.VehicleClass,
            v => v.PrototypeWeight = vehicle.PrototypeWeight);
    }

    public async Task<int> AddEnginePowerForSameKeeperAndClass(Vehicle vehicle)
    {
        if (vehicle.EnginePower == null || vehicle.KeeperSignature.IsEmpty || vehicle.VehicleClass.IsEmpty) return 0;
        return await FillInForSameKeeperAsync(
            v => v.OwningPersonId == vehicle.OwningPersonId && v.EnginePower == null &&
                v.KeeperSignature == vehicle.KeeperSignature && v.VehicleClass == vehicle.VehicleClass,
            v => v.EnginePower = vehicle.EnginePower);
    }

    public async Task<int> AddKeeperCountryForSameKeeper(Vehicle vehicle)
    {
        if (vehicle.KeeperCountryId is null or 0 || vehicle.KeeperSignature.IsEmpty) return 0;
        return await FillInForSameKeeperAsync(
            v => v.OwningPersonId == vehicle.OwningPersonId && v.KeeperCountryId == null &&
                v.KeeperSignature == vehicle.KeeperSignature,
            v => v.KeeperCountryId = vehicle.KeeperCountryId);
    }

    /// <summary>
    /// Loads the matching vehicles and changes them through the change tracker (not <c>ExecuteUpdate</c>),
    /// so each changed vehicle gets a change log row. Only one owner's vehicles, so few rows.
    /// </summary>
    private async Task<int> FillInForSameKeeperAsync(Expression<Func<Vehicle, bool>> predicate, Action<Vehicle> fillIn)
    {
        using var dbContext = Factory.CreateDbContext();
        var vehicles = await dbContext.Vehicles.Where(predicate).ToListAsync();
        if (vehicles.Count == 0) return 0;
        foreach (var vehicle in vehicles) fillIn(vehicle);
        return await dbContext.SaveChangesAsync();
    }
}