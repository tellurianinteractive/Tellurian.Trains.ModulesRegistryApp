﻿namespace ModulesRegistry.Services.Implementations
{
    public sealed class LayoutParticipantService(IDbContextFactory<ModulesDbContext> factory)
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

        public async Task<IEnumerable<LayoutParticipant>> GetAllForLayout(ClaimsPrincipal? principal, int layoutId)
        {
            if (principal.IsAuthenticated())
            {
                var dbContect = Factory.CreateDbContext();

                return await dbContect.LayoutParticipants.AsNoTracking()
                    .Include(x => x.Layout).ThenInclude(x => x.Meeting)
                    .Include(x => x.LayoutStations).ThenInclude(ls => ls.Station)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module).ThenInclude(x => x.ModuleOwnerships)
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
        /// Saves <see cref="=LayoutParticipant"/> including the modules and stations the participant brings. 
        /// </summary>
        /// <param name="principal"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<(int Count, string Message, LayoutParticipant? Entity)> SaveAsync(ClaimsPrincipal? principal, LayoutParticipant entity)
        {
            if (principal.IsAuthenticated())
            {
                if (!entity.IsValid()) return (0, "Incomplete data", entity);
                var dbContext = Factory.CreateDbContext();
                if (entity.Id > 0)
                {
                    var existing = await dbContext.LayoutParticipants.SingleOrDefaultAsync(lp => lp.Id == entity.Id).ConfigureAwait(false);
                    if (existing is not null)
                    {
                        dbContext.Entry(existing).CurrentValues.SetValues(entity);
                        existing.LayoutModules = entity.LayoutModules;
                        existing.LayoutStations = entity.LayoutStations;
                        var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
                        return result.SaveResult(existing);
                    }
                    return (0, "Inconsistent data", entity);
                }
                else
                {
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
                var dbContext = Factory.CreateDbContext();
                var existing = await dbContext.LayoutParticipants.FindAsync(layputParticipantId).ConfigureAwait(false);
                if (existing is not null)
                {
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
