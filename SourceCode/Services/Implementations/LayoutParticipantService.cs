namespace ModulesRegistry.Services.Implementations
{
    public sealed class LayoutParticipantService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public LayoutParticipantService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public async Task<IEnumerable<LayoutParticipant>> GetAllForLayout(ClaimsPrincipal principal, int layoutId)
        {
            if (principal.IsAuthenticated())
            {
                var dbContect = Factory.CreateDbContext();
                return await dbContect.LayoutParticipants.AsNoTracking()
                    .Include(x => x.Layout).ThenInclude(x => x.Meeting)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module)
                    .Where(x => x.LayoutId == layoutId)
                    .ToListAsync().ConfigureAwait(false);
            }
            return Array.Empty<LayoutParticipant>();
        }

        public async Task<LayoutParticipant?> GetLayoutParticipantAsync(ClaimsPrincipal principal, int layoutParticipantId)
        {
            if (principal.IsAuthenticated())
            {
                var dbContect = Factory.CreateDbContext();
                return await dbContect.LayoutParticipants.AsNoTracking()
                    .Include(x => x.Layout).ThenInclude(x => x.Meeting)
                    .Include(x => x.LayoutModules).ThenInclude(x => x.Module)
                    .Where(x => x.Id == layoutParticipantId)
                    .SingleOrDefaultAsync().ConfigureAwait(false);
            }
            return null;
        }
    }
}
