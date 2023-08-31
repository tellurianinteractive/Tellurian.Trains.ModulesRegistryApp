namespace ModulesRegistry.Services.Implementations;

public class StationService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    private readonly ITimeProvider TimeProvider;
    public StationService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
    {
        Factory = factory;
        TimeProvider = timeProvider;
    }

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
                 .Include( m => m.Region)
                 .SingleOrDefaultAsync()
                 .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated())
        {
            if (!ownershipRef.IsNone) ownershipRef = principal.UpdateFrom(ownershipRef);
            using var dbContext = Factory.CreateDbContext();
            var isMemberInGroupsInSameDomain = GroupService.IsMemberInGroupsInSameDomain(dbContext, principal, ownershipRef);
            return await dbContext.Stations.AsNoTracking()
                 .Where(s => s.Id == id && s.Modules.Any(m => (m.ModuleOwnerships.Any(mo => mo.PersonId == ownershipRef.PersonId || mo.GroupId == ownershipRef.GroupId))))
                 .Include(m => m.StationTracks)
                 .Include(m => m.Modules)
                 .Include(m => m.Region)
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
            var isMemberInGroupsInSameDomain = GroupService.IsMemberInGroupsInSameDomain(dbContext, principal, ownershipRef);
            var stations = await dbContext.Stations.AsNoTracking()
                .Where(s => s.Modules.Any(m => m.ModuleOwnerships.Any(mo => mo.PersonId == ownershipRef.PersonId || mo.GroupId == ownershipRef.GroupId)))
                .Include(s => s.Modules).ThenInclude(m => m.ModuleOwnerships)
                .Include(s => s.StationTracks)
                .Include(m => m.PrimaryModule).ThenInclude(pm => pm.ModuleOwnerships)
                .ToListAsync()
                .ConfigureAwait(false);
            if (ownershipRef.IsPersonInGroup)
            {
                var group = await dbContext.Groups.FindAsync(ownershipRef.GroupId).ConfigureAwait(false);
                if (group is not null)
                {
                    return stations.Where(s => s.Modules.Any(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownershipRef, isMemberInGroupsInSameDomain) && principal.IsMemberOfGroupSpecificGroupDomainOrNone(group.GroupDomainId)));
                }
            }
            else
            {
                return stations.Where(s => s.Modules.Any(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownershipRef, isMemberInGroupsInSameDomain)));
            }
        }
        return Array.Empty<Station>();
    }

    public async Task<(int Count, string Message, Station? Entity)> SaveAsync(ClaimsPrincipal? principal, Station entity, ModuleOwnershipRef ownershipRef, int moduleId)
    {
        if (principal.IsAuthenticated())
        {
            ownershipRef = principal.UpdateFrom(ownershipRef);
            if (ownershipRef.IsGroup || ownershipRef.IsPersonInGroup)
            {
                using var dbContext = Factory.CreateDbContext();
                var isDataAdministrator = await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.IsDataAdministrator && gm.GroupId == ownershipRef.GroupId && gm.PersonId == principal.PersonId()).ConfigureAwait(false);
                if (isDataAdministrator || principal.IsCountryOrGlobalAdministrator())
                {
                    return await AddOrUpdate(dbContext, principal, entity, moduleId).ConfigureAwait(false);
                }
            }
            else if (principal.MaySave(ownershipRef))
            {
                using var dbContext = Factory.CreateDbContext();
                return await AddOrUpdate(dbContext, principal, entity, moduleId).ConfigureAwait(false);
            }
        }
        return principal.SaveNotAuthorised<Station>();

        static async Task<(int Count, string Message, Station? Entity)> AddOrUpdate(ModulesDbContext dbContext, ClaimsPrincipal? principal, Station entity, int moduleId)
        {
            var existing = await dbContext.Stations.Include(s => s.StationTracks).Include(s => s.Modules).FirstOrDefaultAsync(s => s.Id == entity.Id).ConfigureAwait(false);
            return (existing is null) ?
                await AddNew(dbContext, principal, entity, moduleId).ConfigureAwait(false) :
                await UpdateExisting(dbContext, entity, existing).ConfigureAwait(false);
        }

        static async Task<(int Count, string Message, Station? Entity)> AddNew(ModulesDbContext dbContext, ClaimsPrincipal? principal, Station entity, int moduleId)
        {
            entity.PrimaryModuleId = moduleId;
            dbContext.Add(entity);
            var module = await dbContext.Modules.FindAsync(moduleId).ConfigureAwait(false);
            if (module is null) return principal.SaveNotAuthorised<Station>();

            entity.Modules.Add(module);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(entity);
        }

        static async Task<(int Count, string Message, Station? Entity)> UpdateExisting(ModulesDbContext dbContext, Station entity, Station existing)
        {
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            AddOrRemoveTracks(dbContext, entity, existing);

            if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing);

            static void AddOrRemoveTracks(ModulesDbContext dbContext, Station entity, Station existing)
            {
                foreach (var track in entity.StationTracks)
                {
                    var existingTrack = existing.StationTracks.AsQueryable().FirstOrDefault(t => t.Id == track.Id);
                    if (existingTrack is null) existing.StationTracks.Add(track);
                    else dbContext.Entry(existingTrack).CurrentValues.SetValues(track);
                }
                foreach (var track in existing.StationTracks) if (!entity.StationTracks.Any(st => st.Id == track.Id)) dbContext.Remove(track);
            }

            static bool IsUnchanged(ModulesDbContext dbContext, Station station) =>
                dbContext.Entry(station).State == EntityState.Unchanged &&
                station.StationTracks.All(st => dbContext.Entry(st).State == EntityState.Unchanged);
        }
    }


    public async Task<bool> IsSubmittedToUpcomingMeeting(Station? station)
    {
        if (station is null) return false;
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.LayoutStations.AnyAsync(ls => ls.StationId == station.Id && ls.LayoutParticipant.Layout.Meeting.EndDate > TimeProvider.Now);

    }
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.MayDelete(principal.OwnerRef()))
        {
            var entity = await FindByIdAsync(principal, id).ConfigureAwait(false);
            using var dbContext = Factory.CreateDbContext();
            if (entity is null) return principal.NotAuthorized<Station>();
            dbContext.Stations.Remove(entity);
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.DeleteResult();
        }
        return principal.NotAuthorized<Station>();
    }
}
