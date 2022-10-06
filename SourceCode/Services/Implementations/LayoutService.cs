namespace ModulesRegistry.Services.Implementations;

public sealed class LayoutService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    private readonly ITimeProvider TimeProvider;
    public LayoutService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
    {
        Factory = factory;
        TimeProvider = timeProvider;
    }

    public async Task<int> ModulesRegisteredCountAsync(ClaimsPrincipal? principal, int meetingId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutModules.AsNoTracking()
                .Where(ls => ls.LayoutParticipant.Layout.MeetingId == meetingId)
                .CountAsync();
        }
        return -1;
    }

    public async Task<IEnumerable<LayoutStation>> GetStationsAsync(ClaimsPrincipal? principal, int layoutId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutStations.AsNoTracking()
                .Where(ls => ls.LayoutParticipantId == layoutId)
                .ToListAsync();
        }
        return Array.Empty<LayoutStation>();
    }

    public async Task<(int Count, string Message, Layout? Entity)> SaveAsync(ClaimsPrincipal? principal, Layout entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            if (!entity.ModuleRegistrationClosingDate.HasValue) entity.ModuleRegistrationClosingDate = entity.RegistrationClosingDate;
            var existing = await dbContext.Layouts.SingleOrDefaultAsync(l => l.Id == entity.Id);
            if (existing is null)
            {
                dbContext.Layouts.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(entity).State == EntityState.Unchanged) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }

        }
        return (0, Resources.Strings.NotAuthorized, entity);
    }

    public async Task<IEnumerable<Module>> GetAvailableModules(ClaimsPrincipal? principal, LayoutParticipant participant, Layout layout)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();

            var groupsId = await dbContext.GroupMembers.AsNoTracking()
                .Where(gm => (gm.IsDataAdministrator || gm.IsGroupAdministrator || gm.MayBorrowGroupsModules) && gm.PersonId == participant.PersonId)
                .Select(gm => gm.GroupId)
                .ToListAsync()
                .ConfigureAwait(false);

            var modules = await dbContext.Modules.AsNoTracking()
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
                .Include(m => m.Standard)
                .Where(m => m.ScaleId == layout.PrimaryModuleStandard.ScaleId)
                .ToListAsync()
                .ConfigureAwait(false);

            var personsIdYouCanBorrowModulesFrom = await dbContext.GroupMembers.AsNoTracking()
                .Where(gm => gm.MemberMayBorrowMyModules && gm.Group.GroupMembers.Any(gm => gm.PersonId == principal.PersonId()))
                .Select(gm => gm.PersonId).ToListAsync().ConfigureAwait(false);
            ;

            if (modules is not null)
            {
                return modules
                   .Where(m => m.ModuleOwnerships.Any(
                        mo => mo.PersonId == participant.PersonId || personsIdYouCanBorrowModulesFrom.Any(id => id == mo.PersonId) || groupsId.Any(id => id == mo.GroupId)));
            }
        }
        return Array.Empty<Module>();

    }

    /// <summary>
    /// Adds <see cref="LayoutModule">modules</see> and <see cref="LayoutService">stations</see> in a <see cref="ModulePackage"/> to a <see cref="Layout"/>
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="participantId"></param>
    /// <param name="layoutId"></param>
    /// <param name="package"></param>
    /// <returns></returns>
    public async Task<(int Count, string Message)> AddPackageModulesAsync(ClaimsPrincipal? principal, int layoutParticipantId, ModulePackage package)
    {
        if (principal.IsAuthenticated())
        {
            var result = package.Modules.Count();
            using var dbContext = Factory.CreateDbContext();
            var participant = await dbContext.LayoutParticipants
                .SingleOrDefaultAsync(lp => lp.Id == layoutParticipantId);
            if (participant is null) return (-1, Resources.Strings.NotFound);
            foreach (var module in package.Modules)
            {
                var existing = await dbContext.LayoutModules.SingleOrDefaultAsync(lm => lm.ModuleId == module.Id && lm.LayoutParticipantId == layoutParticipantId);
                if (existing is null)
                {
                    var layoutModule = new LayoutModule { ModuleId = module.Id, LayoutParticipantId = participant.Id, RegisteredTime = TimeProvider.Now };
                    if (module.StationId.HasValue)
                    {
                        var layoutStation = new LayoutStation { StationId = module.StationId.Value, LayoutParticipantId = layoutParticipantId };
                        layoutModule.LayoutStation = layoutStation;
                    }
                    dbContext.LayoutModules.Add(layoutModule);
                    await dbContext.SaveChangesAsync();
                }
                else
                {
                    result--;
                }
            }
            return result == 0 ? (-1, Resources.Strings.NoModification) : (1, Resources.Strings.Saved);
        }
        return (0, Resources.Strings.NotAuthorized);
    }

    public async Task<IEnumerable<LayoutModule>> GetRegisteredModulesAsync(ClaimsPrincipal? principal, int layoutParticipantId = 0)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.LayoutModules
                .Include(lm => lm.LayoutStation)
                .Include(lm => lm.Module).ThenInclude(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                .Include(lm => lm.LayoutParticipant).ThenInclude(p => p.MeetingParticipant).ThenInclude(mp => mp.Person)
                .Where(lm => layoutParticipantId == 0 || lm.LayoutParticipantId == layoutParticipantId)
                .ToListAsync();
        }
        return Array.Empty<LayoutModule>();
    }

    public async Task<(int Count, string Message)> RemoveModuleAsync(ClaimsPrincipal? principal, int layoutModuleId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.LayoutModules
                .Include(lm => lm.LayoutStation).ThenInclude(ls => ls.LayoutModules)
                .SingleOrDefaultAsync(lm => lm.Id == layoutModuleId);

            if (existing is null) return (-1, Resources.Strings.NoModification);

            var stationIsReferredFromOtherLayoutModule = existing.LayoutStation is not null && existing.LayoutStation.LayoutModules.Any(lm => lm.Id != existing.Id);
            var layoutStationIsNotUsedInLayout = existing.LayoutStation is null || await dbContext.LayoutStations.Where(ls => ls.Id == existing.LayoutStationId).AnyAsync(ls => !ls.StartingLines.Any() && !ls.EndingLines.Any());
            if (layoutStationIsNotUsedInLayout)
            {
                if (!stationIsReferredFromOtherLayoutModule && existing.LayoutStation is not null)
                {
                    dbContext.LayoutStations.Remove(existing.LayoutStation);
                }
                dbContext.LayoutModules.Remove(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.DeleteResult();
            }
            Data.Resources.Strings.NotAuthorised.DeleteResult();
        }
        return principal.DeleteNotAuthorized<LayoutModule>();
    }

}
