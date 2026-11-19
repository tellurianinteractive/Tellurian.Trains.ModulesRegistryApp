namespace ModulesRegistry.Services.Implementations
{
    public sealed class LayoutParticipantService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
        private readonly ITimeProvider TimeProvider = timeProvider;

        public async Task<IEnumerable<LayoutParticipant>> GetAllForLayout(ClaimsPrincipal? principal, int layoutId)
        {
            if (principal.IsAuthenticated())
            {
                var dbContect = Factory.CreateDbContext();

                return await dbContect.LayoutParticipants.AsNoTracking()
                    .Include(x => x.Layout).ThenInclude(x => x.Meeting)
                    .Include(x => x.LayoutStations).ThenInclude(ls => ls.Station)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module).ThenInclude(x => x.ModuleOwnerships).ThenInclude(x=> x.Person)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module).ThenInclude(x => x.ModuleOwnerships).ThenInclude(x => x.Group)
                   .Include(x => x.LayoutModules).ThenInclude(x => x.Module).ThenInclude(x => x.Standard)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module).ThenInclude(x => x.Station).ThenInclude(x => x.StationTracks)
                    .Include(x => x.Person).ThenInclude(p => p.Country)
                    .Include(x => x.MeetingParticipant).ThenInclude(x => x.Meeting)
                    .Where(x => x.LayoutId == layoutId && x.MeetingParticipant.CancellationTime == null && x.MeetingParticipant.Person.DeletedTimestamp.HasValue == false)
                    .ToListAsync().ConfigureAwait(false);
            }
            return [];
        }

        public async Task<LayoutParticipant?> GetByIdAsync(ClaimsPrincipal? principal, int meetingPartictipantId, int layoutId)
        {
            if (principal.IsAuthenticated())
            {
                var dbContect = Factory.CreateDbContext();
                return await dbContect.LayoutParticipants.AsNoTracking()
                    .Where(x => x.LayoutId == layoutId && x.MeetingParticipantId == meetingPartictipantId && x.MeetingParticipant.Person.DeletedTimestamp.HasValue == false)
                    .Include(x => x.Layout).ThenInclude(x => x.Meeting)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module).ThenInclude(m => m.Standard)
                    .Include(x => x.LayoutStations).ThenInclude(ls => ls.Station)
                    .SingleOrDefaultAsync()
                    .ConfigureAwait(false);
            }
            return null;
        }

        /// <summary>
        /// Registers a meeting participant for a layout, or changes the registration: the participant while the layout is open,
        /// or an organiser of the meeting at any time (<see cref="MeetingPermissionExtensions"/>).
        /// </summary>
        public async Task<(int Count, string Message, LayoutParticipant? Entity)> SaveAsync(ClaimsPrincipal? principal, LayoutParticipant entity)
        {
            if (principal.IsAuthenticated())
            {
                if (!entity.IsValid()) return (0, "Incomplete data", entity);
                using var dbContext = Factory.CreateDbContext();
                var meetingParticipantPersonId = await dbContext.MeetingParticipants.AsNoTracking()
                    .Where(mp => mp.Id == entity.MeetingParticipantId)
                    .Select(mp => (int?)mp.PersonId)
                    .SingleOrDefaultAsync().ConfigureAwait(false);
                // The layout registration belongs to the meeting participant's person, and the layout to the participant's meeting.
                if (meetingParticipantPersonId != entity.PersonId ||
                    !await dbContext.MayManageParticipantAsync(principal, entity.MeetingParticipantId, TimeProvider.LocalTime, entity.LayoutId).ConfigureAwait(false))
                    return principal.SaveNotAuthorised<LayoutParticipant>();
                if (entity.Id > 0)
                {
                    var existing = await dbContext.LayoutParticipants.SingleOrDefaultAsync(lp => lp.Id == entity.Id).ConfigureAwait(false);
                    if (existing is not null)
                    {
                        if (existing.MeetingParticipantId != entity.MeetingParticipantId || existing.LayoutId != entity.LayoutId || existing.PersonId != entity.PersonId)
                            return principal.SaveNotAuthorised<LayoutParticipant>();
                        dbContext.Entry(existing).CurrentValues.SetValues(entity);
                        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(entity);
                        existing.LastModifiedDateTime = TimeProvider.Now;
                        var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
                        return result.SaveResult(existing);
                    }
                    return (0, "Inconsistent data", entity);
                }
                else
                {
                    entity.LastModifiedDateTime = TimeProvider.Now;
                    dbContext.LayoutParticipants.Add(entity);
                    var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
                    return result.SaveResult(entity);
                }
            }
            return principal.SaveNotAuthorised<LayoutParticipant>();
        }

        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int? layputParticipantId)
        {
            if (principal.IsAuthenticated() && layputParticipantId.HasValue)
            {
                using var dbContext = Factory.CreateDbContext();
                var existing = await dbContext.LayoutParticipants.Include(lp => lp.LayoutModules).SingleOrDefaultAsync(lp => lp.Id == layputParticipantId).ConfigureAwait(false);
                if (existing is not null)
                {
                    if (!await dbContext.MayManageParticipantAsync(principal, existing.MeetingParticipantId, TimeProvider.LocalTime, existing.LayoutId).ConfigureAwait(false))
                        return principal.NotAuthorized<LayoutParticipant>();
                    if (existing.LayoutModules.Count > 0) { return (-1, "RemoveRegisteredModulesFirst"); }
                    dbContext.LayoutParticipants.Remove(existing);
                    var result = await dbContext.SaveChangesAsync();
                    return result.DeleteResult();
                }
                return principal.NotFound();
            }
            return principal.NotAuthorized<LayoutParticipant>();
        }
    }
}
