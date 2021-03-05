using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class StationService : IStationService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public StationService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        public Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id) =>
            FindByIdAsync(principal, id, 0);

        public async Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id, int personalOwnerId)
        {
            if (principal is not null)
            {
                var ownerPersonId = personalOwnerId > 0 ? personalOwnerId : principal.PersonId();
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Stations.AsNoTracking()
                    .Where(s => s.Id == id && s.Modules.Any(m => m.ModuleOwnerships.Any(mo => mo.PersonId == ownerPersonId)))
                    .Include(m => m.StationTracks)
                    .Include(m => m.Modules)
                    .SingleOrDefaultAsync();
            }
            return null;
        }

        public async Task<IEnumerable<Station>> GetAllAsync(ClaimsPrincipal? principal)
        {
            if (principal is not null)
            {
                var ownerId = principal.PersonId();
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Stations.AsNoTracking()
                    .Where(s => s.Modules.Any(m => m.ModuleOwnerships.Any(mo => mo.PersonId == ownerId)))
                    .Include(s => s.StationTracks)
                    .Include(m => m.Modules)
                    .ToListAsync();
            }
            return Array.Empty<Station>();
        }
 
        public async Task<(int Count, string Message, Station? Entity)> SaveAsync(ClaimsPrincipal? principal, Station entity, int owningPersonId, int moduleId)
        {
            var ownerId = owningPersonId > 0 ? owningPersonId : principal.PersonId();
            if (principal.MaySave(ownerId))
            {
                using var dbContext = Factory.CreateDbContext();
                var existing = await dbContext.Stations.Include(s => s.StationTracks).FirstOrDefaultAsync(s => s.Id == entity.Id);
                if (existing is null)
                {
                    dbContext.Add(entity);
                    var module = await dbContext.Modules.FindAsync(moduleId);
                    if (module is null) return principal.SaveNotAuthorised<Station>();
                    entity.Modules.Add(module);
                }
                else
                {
                    dbContext.Entry(existing).CurrentValues.SetValues(entity);
                    foreach (var track in entity.StationTracks)
                    {
                        var existingTrack = existing.StationTracks.AsQueryable().FirstOrDefault(t => t.Id == track.Id);
                        if (existingTrack is null)
                        {
                            existing.StationTracks.Add(track);
                        }
                        else
                        {
                            dbContext.Entry(existingTrack).CurrentValues.SetValues(track);
                        }
                    }
                    foreach (var track in existing.StationTracks)
                    {
                        if (!entity.StationTracks.Any(st => st.Id == track.Id)) dbContext.Remove(track);
                    }
                    var existingModule = await dbContext.Modules.FindAsync(moduleId);
                    if (existingModule is null)
                    {
                        existing.Modules.Add(new Module { Id = moduleId, StationId = existing.Id });
                    }
                }
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing ?? entity);
            }
            return principal.SaveNotAuthorised<Station>();
        }


        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal.MayDelete(principal.PersonId()))
            {
                var entity = await FindByIdAsync(principal, id);
                using var dbContext = Factory.CreateDbContext();

                dbContext.Stations.Remove(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.DeleteResult();

            }
            return principal.DeleteNotAuthorized<Station>();
        }
    }
}
