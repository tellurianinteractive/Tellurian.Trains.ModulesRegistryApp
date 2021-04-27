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
    public class StationService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public StationService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        public async Task<IEnumerable<ListboxItem>> StationItemsAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
        {
            if (principal.IsAuthenticated())
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Stations.AsNoTracking()
                    .Where(s => s.Modules.Any(m => m.ModuleOwnerships.Any(mo => mo.GroupId == ownerRef.GroupId || mo.PersonId == ownerRef.PersonId)))
                    .Select(s => new ListboxItem(s.Id, s.FullName))
                    .ToListAsync();
            }
            return Array.Empty<ListboxItem>();
        }

        public Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id) =>
            FindByIdAsync(principal, id, ModuleOwnershipRef.None);

        public async Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef)
        {
            if (principal.IsAuthenticated())
            {
                ownerRef = principal.UpdateFrom(ownerRef);
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Stations.AsNoTracking()
                     .Where(s => s.Id == id && s.Modules.Any(m => m.ObjectVisibilityId >= principal.MinimumObjectVisibility(ownerRef) && m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId || mo.GroupId == ownerRef.GroupId)))
                     .Include(m => m.StationTracks)
                     .Include(m => m.Modules)
                     .SingleOrDefaultAsync();
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
                    .Include(m => m.Modules)
                    .ToListAsync();
                if (ownershipRef.IsPersonInGroup)
                {
                    var group = await dbContext.Groups.FindAsync(ownershipRef.GroupId);
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

        public async Task<(int Count, string Message, Station? Entity)> SaveAsync(ClaimsPrincipal? principal, Station entity, ModuleOwnershipRef ownerRef, int moduleId)
        {
            if (principal.IsAuthenticated())
            {
                ownerRef = principal.UpdateFrom(ownerRef);
                if (ownerRef.IsGroup)
                {
                    using var dbContext = Factory.CreateDbContext();
                    var isDataAdministrator = await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.IsDataAdministrator && gm.GroupId == ownerRef.GroupId && gm.PersonId == principal.PersonId());
                    if (isDataAdministrator)
                    {
                        return await AddOrUpdate(dbContext, principal, entity, moduleId);
                    }
                }
                else if (principal.MaySave(ownerRef))
                {
                    using var dbContext = Factory.CreateDbContext();
                    return await AddOrUpdate(dbContext, principal, entity, moduleId);
                }
            }
            return principal.SaveNotAuthorised<Station>();

            static async Task<(int Count, string Message, Station? Entity)> AddOrUpdate(ModulesDbContext dbContext, ClaimsPrincipal? principal, Station entity, int moduleId)
            {
                var existing = await dbContext.Stations
                    .Include(s => s.StationTracks)
                    .Include(s => s.Modules)
                    .FirstOrDefaultAsync(s => s.Id == entity.Id);
                return (existing is null) ?
                    await AddNew(dbContext, principal, entity, moduleId) :
                    await UpdateExisting(dbContext, entity, existing, moduleId);
            }

            static async Task<(int Count, string Message, Station? Entity)> AddNew(ModulesDbContext dbContext, ClaimsPrincipal? principal, Station entity, int moduleId)
            {
                dbContext.Add(entity);
                var module = await dbContext.Modules.FindAsync(moduleId);
                if (module is null) return principal.SaveNotAuthorised<Station>();
                entity.Modules.Add(module);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }

            static async Task<(int Count, string Message, Station? Entity)> UpdateExisting(ModulesDbContext dbContext, Station entity, Station existing, int moduleId)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                AddOrRemoveTracks(dbContext, entity, existing);

                if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
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

        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal.MayDelete(principal.OwnerRef()))
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
