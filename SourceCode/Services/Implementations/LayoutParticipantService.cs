namespace ModulesRegistry.Services.Implementations
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
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module)
                    .Include(x => x.Person)
                    .Include(x => x.LayoutStations).ThenInclude(ls => ls.Station)
                    .Where(x => x.LayoutId == layoutId && x.MeetingParticipant.CancellationTime == null)
                    .ToListAsync().ConfigureAwait(false);
            }
            return Array.Empty<LayoutParticipant>();
        }

        public async Task<LayoutParticipant?> GetByIdAsync(ClaimsPrincipal? principal,int meetingPartictipantId, int layoutId)
        {
            if (principal.IsAuthenticated())
            {
                var dbContect = Factory.CreateDbContext();
                return await dbContect.LayoutParticipants.AsNoTracking()
                    .Where(x => x.LayoutId == layoutId && x.MeetingParticipantId == meetingPartictipantId)
                    .Include(x => x.Layout).ThenInclude(x => x.Meeting)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module)
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
    }
}
