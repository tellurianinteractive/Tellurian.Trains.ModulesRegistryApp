namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// A station's tracks, saved one by one from the track form (Migration step 3.6). Only a user who may edit the station
/// (<c>MayEditStationAsync</c>) changes its tracks; before step 3.6 any signed-in user could change or delete any track.
/// </summary>
public class StationTrackService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    /// <summary>A track of a station the user may edit, or null.</summary>
    public async Task<StationTrack?> FindAsync(ClaimsPrincipal? principal, int stationId, int trackId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayEditStationAsync(principal, stationId)) return null;
        return await dbContext.StationTracks.AsNoTracking().SingleOrDefaultAsync(st => st.Id == trackId && st.StationId == stationId);
    }

    public async Task<DataServiceResult<StationTrack>> SaveOrUpdateAsync(ClaimsPrincipal? principal, StationTrack entity)
    {
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        using var dbContext = Factory.CreateDbContext();
        var existing = entity.Id > 0 ? await dbContext.StationTracks.FindAsync(entity.Id) : null;
        if (entity.Id > 0 && existing is null) return entity.NonExisting();
        if (existing is not null) entity.StationId = existing.StationId; // A track never moves to another station.
        if (!await dbContext.MayEditStationAsync(principal, entity.StationId)) return entity.NotAuthorised();
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

    public async Task<DataServiceResult<StationTrack>> DeleteAsync(ClaimsPrincipal? principal, int trackId)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.StationTracks.FindAsync(trackId);
        if (existing is null) return new StationTrack { Id = trackId }.NonExisting();
        if (!await dbContext.MayEditStationAsync(principal, existing.StationId)) return existing.NotAuthorised();
        dbContext.StationTracks.Remove(existing);
        var count = await dbContext.SaveChangesAsync();
        return existing.SuccessOrFailure(count);
    }
}