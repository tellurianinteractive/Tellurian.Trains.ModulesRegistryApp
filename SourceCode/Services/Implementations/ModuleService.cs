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
    public sealed class ModuleService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        private readonly Random Random = new();
        public ModuleService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        public async Task<IEnumerable<ListboxItem>> ModuleItems(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef, int? stationId)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            if (principal is not null)
            {
                using var dbContext = Factory.CreateDbContext();
                List<Module>? modules;
                if (stationId.HasValue)
                {
                    modules = await dbContext.Modules.AsNoTracking()
                         .Where(m => (!m.StationId.HasValue || m.StationId.Value == stationId.Value) && m.ModuleOwnerships
                         .Any(mo => mo.PersonId == ownerRef.PersonId || mo.GroupId == ownerRef.GroupId))
                         .ToListAsync();
                }
                else
                {
                    modules = await dbContext.Modules.AsNoTracking()
                         .Where(m => m.ModuleOwnerships
                         .Any(mo => mo.PersonId == ownerRef.PersonId || mo.GroupId == ownerRef.GroupId))
                         .ToListAsync();

                }
                return modules.Select(m => new ListboxItem(m.Id, $"{m.FullName} {m.ConfigurationLabel}{m.PackageLabel} {m.FremoNumber}".Trim())).OrderBy(l => l.Description);
            }
            return Array.Empty<ListboxItem>();
        }

        public async Task<bool> HasAnyNonStationAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
        {
            if (principal is not null)
            {
                ownerRef = principal.UpdateFrom(ownerRef);
                using var dbContext = Factory.CreateDbContext();
                if (ownerRef.IsPerson || ownerRef.IsPersonInGroup)
                {
                    return await dbContext.Modules
                        .Where(m => m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId))
                        .AnyAsync(m => !m.StationId.HasValue);
                }
                else if (ownerRef.IsGroup)
                {
                    return await dbContext.Modules
                        .Where(m => m.ModuleOwnerships.Any(mo => mo.GroupId == ownerRef.GroupId))
                        .AnyAsync(m => !m.StationId.HasValue);

                }
            }
            return false;
        }

        public Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal) => GetAllAsync(principal, ModuleOwnershipRef.None);

        public async Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            using var dbContext = Factory.CreateDbContext();
            if (ownerRef.IsGroup)
            {
                bool isAdministrator = await IsGroupOrDataAdministrator(dbContext, principal, ownerRef);
                if (isAdministrator)
                {
                    return await dbContext.Modules.AsNoTracking()
                        .Where(m => m.ModuleOwnerships.Any(mo => mo.GroupId == ownerRef.GroupId))
                        .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                        .Include(m => m.Scale)
                        .Include(m => m.Standard)
                        .ToListAsync();
                }
            }
            else
            {
                var countryId = dbContext.People.Find(ownerRef.PersonId)?.CountryId;
                if (principal.IsAuthorisedInCountry(countryId, ownerRef.PersonId))
                {
                    return await dbContext.Modules.AsNoTracking()
                        .Where(m => m.ModuleOwnerships
                        .Any(mo => mo.PersonId == ownerRef.PersonId))
                        .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                        .Include(m => m.Scale)
                        .Include(m => m.Standard)
                        .ToListAsync();
                }
            }
            return Array.Empty<Module>();
        }

        private static async Task<bool> IsGroupOrDataAdministrator(ModulesDbContext dbContext, ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
        {
            if (principal.IsGlobalAdministrator()) return true;
            if (principal.IsCountryAdministrator())
            {
                if (await dbContext.Groups.AnyAsync(g => g.Id == ownerRef.GroupId && g.CountryId == principal.CountryId())) return true;
            }
            return await dbContext.GroupMembers.AsNoTracking()
                .AnyAsync(gm => gm.GroupId == ownerRef.GroupId && gm.PersonId == principal.PersonId() && (gm.IsDataAdministrator || gm.IsGroupAdministrator));
        }

        public Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id) =>
            FindByIdAsync(principal, id, ModuleOwnershipRef.None);

        public async Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            using var dbContext = Factory.CreateDbContext();
            if (principal is not null)
            {
                if (ownerRef.IsGroup)
                {
                    var isAdministrator = await IsGroupOrDataAdministrator(dbContext, principal, ownerRef);
                    if (isAdministrator)
                    {
                        return await dbContext.Modules.AsNoTracking()
                         .Where(m => m.Id == id && m.ModuleOwnerships.Any(mo => mo.GroupId == ownerRef.GroupId))
                         .Include(m => m.ModuleOwnerships)
                         .Include(m => m.ModuleExits)
                         .SingleOrDefaultAsync();
                    }
                }
                else
                {
                    return await dbContext.Modules.AsNoTracking()
                        .Where(m => m.Id == id && m.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId))
                        .Include(m => m.ModuleOwnerships)
                        .Include(m => m.ModuleExits)
                        .SingleOrDefaultAsync();
                }
            }
            return null;
        }
        public Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity) =>
            SaveAsync(principal, entity, ModuleOwnershipRef.None);

        public async Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity, ModuleOwnershipRef ownerRef)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            using var dbContext = Factory.CreateDbContext();
            if (principal.MaySave(ownerRef))
            {
                return await AddOrUpdate(dbContext, entity, ownerRef);
            }
            else if (ownerRef.IsGroup)
            {
                bool isPrincipalGroupsDataAdministrator = await IsPrincipalGroupsDataAdministrator(dbContext, principal, ownerRef);
                return isPrincipalGroupsDataAdministrator ? await AddOrUpdate(dbContext, entity, ownerRef) : principal.SaveNotAuthorised<Module>();
            }
            else if (ownerRef.IsPersonInGroup)
            {
                var isAdministrator = await IsPrincipalGroupsDataAdministrator(dbContext, principal, ownerRef);
                var isMember = await IsOwnerMemberOfGroup(ownerRef, dbContext);
                return isAdministrator && isMember ? await AddOrUpdate(dbContext, entity, ownerRef) : principal.SaveNotAuthorised<Module>();
            }
            return principal.SaveNotAuthorised<Module>();

            static async Task<(int Count, string Message, Module? Entity)> AddOrUpdate(ModulesDbContext dbContext, Module entity, ModuleOwnershipRef ownerRef)
            {
                var existing = await dbContext.Modules
                    .Include(m => m.ModuleExits)
                    .FirstOrDefaultAsync(m => m.Id == entity.Id);

                return (existing is null) ?
                    await AddNew(dbContext, entity, ownerRef) :
                    await UpdateExisting(dbContext, entity, existing);
            }

            static async Task<(int Count, string Message, Module? Entity)> AddNew(ModulesDbContext dbContext, Module entity, ModuleOwnershipRef ownerRef)
            {
                if (entity.ModuleOwnerships.Count == 0) entity.ModuleOwnerships.Add(CreateModuleOwnership(ownerRef));
                dbContext.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);

                static ModuleOwnership CreateModuleOwnership(ModuleOwnershipRef ownerRef)
                {
                    if (ownerRef.IsGroup) return new ModuleOwnership { GroupId = ownerRef.GroupId, OwnedShare = 1 };
                    return new ModuleOwnership { PersonId = ownerRef.PersonId, OwnedShare = 1 };
                }
            }

            static async Task<(int Count, string Message, Module? Entity)> UpdateExisting(ModulesDbContext dbContext, Module entity, Module existing)
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                AddOrRemoveExits(dbContext, entity, existing);
                if (IsUnchanged(dbContext, existing)) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing);

                static void AddOrRemoveExits(ModulesDbContext dbContext, Module entity, Module existing)
                {
                    foreach (var exit in entity.ModuleExits)
                    {
                        var existingExit = existing.ModuleExits.AsQueryable().FirstOrDefault(g => g.Id == exit.Id);
                        if (existingExit is null) existing.ModuleExits.Add(exit);
                        else dbContext.Entry(existingExit).CurrentValues.SetValues(exit);
                    }
                    foreach (var exit in existing.ModuleExits) if (!entity.ModuleExits.Any(mg => mg.Id == exit.Id)) dbContext.Remove(exit);
                }

                static bool IsUnchanged(ModulesDbContext dbContext, Module entity) =>
                    dbContext.Entry(entity).State == EntityState.Unchanged &&
                    entity.ModuleExits.All(mg => dbContext.Entry(mg).State == EntityState.Unchanged) &&
                    entity.ModuleOwnerships.All(mo => dbContext.Entry(mo).State == EntityState.Unchanged);

            }

            static async Task<bool> IsPrincipalGroupsDataAdministrator(ModulesDbContext dbContext, ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
            {
                return await dbContext.GroupMembers.AsNoTracking()
                    .AnyAsync(gm => gm.IsDataAdministrator && gm.GroupId == ownerRef.GroupId && gm.PersonId == principal.PersonId());
            }

            static async Task<bool> IsOwnerMemberOfGroup(ModuleOwnershipRef ownerRef, ModulesDbContext dbContext)
            {
                return await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.GroupId == ownerRef.GroupId && gm.PersonId == ownerRef.PersonId);
            }
        }

        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int moduleId)
        {
            var ownerRef = principal.AsModuleOwnershipRef();
            if (principal.MayDelete(ownerRef))
            {
                var entity = await FindByIdAsync(principal, moduleId);
                if (entity is not null && entity.ModuleOwnerships.Any(mo => mo.PersonId == ownerRef.PersonId))
                {
                    using var dbContext = Factory.CreateDbContext();

                    dbContext.Modules.Remove(entity);
                    var result = await dbContext.SaveChangesAsync();
                    return result.DeleteResult();
                }
            }
            return principal.DeleteNotAuthorized<Module>();
        }

        public async Task<(int Count, string Message)> CloneAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef)
        {
            ownerRef = principal.UpdateFrom(ownerRef);
            if (principal.MaySave(ownerRef))
            {
                var clone = await FindByIdAsync(principal, id, ownerRef);
                if (clone is null) return principal.NotFound();
                clone.FullName = CloneFullName(Random, clone);
                clone.Id = 0;
                clone.Station = null;
                clone.StationId = null;
                foreach (var gable in clone.ModuleExits) gable.Id = 0;
                foreach (var ownership in clone.ModuleOwnerships) ownership.Id = 0;
                using var dbContext = Factory.CreateDbContext();
                dbContext.Modules.Add(clone);
                var result = await dbContext.SaveChangesAsync();
                return result.CloneResult();
            }
            return (0, "Not authorized.");  //TODO: Fix extension with transplations.

            static string CloneFullName(Random random, Module module)
            {
                var appended = $"-{random.Next(1, 1000)}";
                var totalLength = module.FullName.Length + appended.Length;
                if (totalLength <= 50) return $"{module.FullName}{appended}";
                return $"{module.FullName.Substring(0, 50 - appended.Length)}{appended}";
            }
        }
    }
}
