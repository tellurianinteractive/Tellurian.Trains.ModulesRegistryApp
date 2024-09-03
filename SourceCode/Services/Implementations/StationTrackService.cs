namespace ModulesRegistry.Services.Implementations;

// NOTE: This service implements the new SaveResult and DeleteResult types.
public class StationTrackService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;


    public async Task<IEnumerable<StationTrack>> GetStationTracksAsync(ClaimsPrincipal? principal, Station? station)
    {
        if (station is null || station.Id == 0) return [];
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.StationTracks
                .Where(st => st.StationId == station.Id)
                .ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<DataServiceResult<StationTrack>> SaveOrUpdateAsync(ClaimsPrincipal? principal, StationTrack entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.StationTracks.FindAsync(entity.Id);
            if (existing is not null)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return existing.Unchanged();
                var count = await dbContext.SaveChangesAsync();
                return existing.SuccessOrFailure(count);
            }
            else
            {
                dbContext.StationTracks.Add(entity);
                var count = await dbContext.SaveChangesAsync();
                return entity.SuccessOrFailure(count);
            }
        }
        return entity.NotAuthorised();
    }

    public async Task<DataServiceResult<StationTrack>> DeleteAsync(ClaimsPrincipal? principal, StationTrack entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.StationTracks.FindAsync(entity.Id);
            if (existing is not null)
            {
                dbContext.StationTracks.Remove(existing);
                var count = await dbContext.SaveChangesAsync();
                return entity.SuccessOrFailure(count);
            }
            return entity.NonExisting();
        }
        return entity.NotAuthorised();

    }
}
