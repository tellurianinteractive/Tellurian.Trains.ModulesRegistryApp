namespace ModulesRegistry.Services.Implementations;

public class StationService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ITimeProvider TimeProvider = timeProvider;

    public async Task<IEnumerable<ListboxItem>> StationItemsAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.Stations.AsNoTracking()
                .Where(s => s.Modules.Any(m => m.ModuleOwnerships.Any(mo => mo.GroupId == ownershipRef.GroupId || mo.PersonId == ownershipRef.PersonId)))
                .Select(s => new ListboxItem(s.Id, $"{s.FullName} {s.PrimaryModule.ConfigurationLabel} ".TrimEnd()))
                .ToListAsync()
                .ConfigureAwait(false);
            return items.OrderBy(s => s.Description).ToList();
        }
        return Array.Empty<ListboxItem>();
    }

    public Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id) =>
        FindByIdWithVisibilityAsync(principal, id, ModuleOwnershipRef.None);

    public async Task<Station?> FindByIdWithVisibilityAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated())
        {
            if (!ownershipRef.IsNone) ownershipRef = principal.UpdateFrom(ownershipRef);
            using var dbContext = Factory.CreateDbContext();
            var isMemberInGroupsInSameDomain = GroupService.IsMemberInGroupsInSameDomain(dbContext, principal, ownershipRef);
            return await dbContext.Stations.AsNoTracking()
                 .Where(s => s.Id == id && s.Modules.Any(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownershipRef, isMemberInGroupsInSameDomain) && (ownershipRef.IsNone || m.ModuleOwnerships.Any(mo => mo.PersonId == ownershipRef.PersonId || mo.GroupId == ownershipRef.GroupId))))
                 .Include(m => m.StationTracks)
                 .Include(m => m.Modules)
                 .Include(m => m.Region)
                 .SingleOrDefaultAsync()
                 .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int stationId, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated())
        {
            if (!ownershipRef.IsNone) ownershipRef = principal.UpdateFrom(ownershipRef);
            using var dbContext = Factory.CreateDbContext();
            var isMemberInGroupsInSameDomain = GroupService.IsMemberInGroupsInSameDomain(dbContext, principal, ownershipRef);
            return await dbContext.Stations.AsNoTracking()
                 .Where(s => s.Id == stationId && s.Modules.Any(m => (m.ModuleOwnerships.Any(mo => mo.PersonId == ownershipRef.PersonId || mo.GroupId == ownershipRef.GroupId))))
                 .Include(m => m.StationTracks)
                 .Include(m => m.Modules)
                 .Include(m => m.Region)
                 .SingleOrDefaultAsync()
                 .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<Station?> FindFullInfoAsync(ClaimsPrincipal? principal, int stationId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Stations.AsNoTracking()
                .Where(s => s.Id == stationId)
                .Include(s => s.PrimaryModule).ThenInclude(pm => pm.ModuleOwnerships.Where(mo => mo.OwnedShare > 0)).ThenInclude(mo => mo.Person)
                .Include(s => s.PrimaryModule).ThenInclude(m => m.Standard)
                .Include(s => s.PrimaryModule).ThenInclude(m => m.Scale)
                .Include(s => s.StationTracks)
                .Include(s => s.StationCustomers).ThenInclude(sc => sc.Cargos).ThenInclude(c => c.Cargo)
                .Include(s => s.StationCustomers).ThenInclude(sc => sc.Cargos).ThenInclude(c => c.Direction)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }
        return null;
    }

    public Task<IEnumerable<Station>> GetAllAsync(ClaimsPrincipal? principal) => GetAllAsync(principal, ModuleOwnershipRef.None);

    public async Task<IEnumerable<Station>> GetAllAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated())
        {
            ownershipRef = principal.UpdateFrom(ownershipRef);
            using var dbContext = Factory.CreateDbContext();
            if (ownershipRef.IsPerson || ownershipRef.IsPersonInGroup)
            {
                return await dbContext.Stations.AsNoTracking()
                    .Include(s => s.Modules).ThenInclude(m => m.ModuleOwnerships)
                    .Include(s => s.StationTracks)
                    .Include(m => m.PrimaryModule).ThenInclude(pm => pm.ModuleOwnerships)
                    .Where(s => s.PrimaryModule.ModuleOwnerships.Any(mo => mo.PersonId == ownershipRef.PersonId))
                    .ToListAsync()
                    .ConfigureAwait(false);

            }
            else if (ownershipRef.IsGroup)
            {
                var isMemberInGroupsInSameDomain = GroupService.IsMemberInGroupsInSameDomain(dbContext, principal, ownershipRef);
                return await dbContext.Stations.AsNoTracking()
                    .Where(s => s.Modules.Any(m => m.ModuleOwnerships.Any(mo => mo.GroupId == ownershipRef.GroupId)))
                    .Include(s => s.Modules).ThenInclude(m => m.ModuleOwnerships)
                    .Include(s => s.StationTracks)
                    .Include(m => m.PrimaryModule).ThenInclude(pm => pm.ModuleOwnerships)
                    .ToListAsync()
                    .ConfigureAwait(false);

            }
        }
        return [];
    }

    /// <summary>
    /// Saves a station's own fields; its tracks are saved one by one (<see cref="StationTrackService"/>). A new station is added to
    /// the module <paramref name="moduleId"/>, which must not have a station, by a user who may edit the module
    /// (<c>MayEditModuleAsync</c>); an existing station by a user who may edit it (<c>MayEditStationAsync</c>). The primary module and
    /// the instruction document are never changed here.
    /// </summary>
    /// <remarks>
    /// Before Migration step 3.6 the rights came from the page's ownership reference and any station id could be saved with it.
    /// </remarks>
    public async Task<(int Count, string Message, Station? Entity)> SaveAsync(ClaimsPrincipal? principal, Station entity, int moduleId = 0)
    {
        if (!principal.IsAuthenticated()) return principal.SaveNotAuthorised<Station>();
        using var dbContext = Factory.CreateDbContext();
        // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5); the signature by the country's language.
        entity.CorrectInput(await dbContext.CountryCasingsAsync(entity.RegionId, principal));
        if (entity.Id > 0)
        {
            var existing = await dbContext.Stations.FirstOrDefaultAsync(s => s.Id == entity.Id).ConfigureAwait(false);
            if (existing is null || !await dbContext.MayEditStationAsync(principal, existing.Id)) return principal.SaveNotAuthorised<Station>();
            entity.PrimaryModuleId = existing.PrimaryModuleId;
            entity.PdfInstructionId = existing.PdfInstructionId;
            entity.KeepKeyRequiredOf(existing);
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            existing.RemoveRedundantKeyRequired(await dbContext.StationTracks.AnyAsync(t => t.StationId == existing.Id && t.IsLockable));
            if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(existing);
        }
        else
        {
            var module = await dbContext.Modules.FindAsync(moduleId).ConfigureAwait(false);
            if (module is null || module.StationId.HasValue || !await dbContext.MayEditModuleAsync(principal, moduleId)) return principal.SaveNotAuthorised<Station>();
            entity.PrimaryModuleId = moduleId;
            entity.PdfInstructionId = null;
            entity.StationTracks.Clear();
            dbContext.Add(entity);
            entity.Modules.Add(module);
            return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(entity);
        }
    }

    public async Task<bool> MayEditAsync(ClaimsPrincipal? principal, int stationId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayEditStationAsync(principal, stationId);
    }

    public async Task<bool> MayAddToModuleAsync(ClaimsPrincipal? principal, int moduleId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayEditModuleAsync(principal, moduleId);
    }

    public async Task<bool> IsSubmittedToUpcomingMeeting(Station? station)
    {
        if (station is null) return false;
        using var dbContext = Factory.CreateDbContext();
        var today = TimeProvider.Today.ToDateTime(TimeOnly.MinValue); // A meeting has not ended until its last day is over, as in PersonDeletionPlan.
        return await dbContext.LayoutStations.AnyAsync(ls => ls.StationId == station.Id && ls.LayoutParticipant.Layout.Meeting.EndDate >= today);

    }
    /// <summary>
    /// Deletes a station with its tracks and instruction document, by a user who may edit it, when it is not registered for a
    /// meeting that has not ended. Before Migration step 3.6 any user who could see the station could delete it.
    /// </summary>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationAsync(principal, id)) return principal.NotAuthorized<Station>();
        var entity = await dbContext.Stations.Include(s => s.StationTracks).Include(s => s.Modules).SingleOrDefaultAsync(s => s.Id == id).ConfigureAwait(false);
        if (entity is null) return principal.NotAuthorized<Station>();
        if (await IsSubmittedToUpcomingMeeting(entity)) return 0.DeleteResult();
        if (entity.PdfInstructionId.HasValue) await DocumentService.RemoveDocumentsAsync(dbContext, [entity.PdfInstructionId.Value]);
        foreach (var module in entity.Modules) module.StationId = null;
        dbContext.Stations.Remove(entity);
        var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
        return result.DeleteResult();
    }
}