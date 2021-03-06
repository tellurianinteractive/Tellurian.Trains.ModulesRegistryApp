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
    public sealed class ModuleService : IModuleService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        private readonly Random Random = new();
        public ModuleService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        public async Task<IEnumerable<ListboxItem>> ModuleItems(ClaimsPrincipal? principal)
        {
            if (principal is not null)
            {
                using var dbContext = Factory.CreateDbContext();
                var modules = await dbContext.Modules.AsNoTracking()
                    .Where(mo => mo.ModuleOwnerships
                    .Any(mo => mo.PersonId == principal.PersonId()))
                    .ToListAsync();
                return modules.Select(m => new ListboxItem(m.Id, $"{m.FullName} {m.ConfigurationLabel}{m.PackageLabel} {m.FremoNumber}".Trim())).OrderBy(l => l.Description);
            }
            return Array.Empty<ListboxItem>();
        }

        public async Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal)
        {
            if (principal is null) return Array.Empty<Module>();
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Modules.AsNoTracking()
                .Where(mo => mo.ModuleOwnerships
                .Any(mo => mo.PersonId == principal.PersonId()))
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                .Include(m => m.Scale)
                .Include(m => m.Standard)
                .ToListAsync();
        }

        public async Task<IEnumerable<Module>> GetForOwningPerson(ClaimsPrincipal? principal, int personId)
        {
            var ownerId = personId == 0 ? principal.PersonId() : personId;
            using var dbContext = Factory.CreateDbContext();
            var countryId = dbContext.People.Find(ownerId)?.CountryId;
            if (principal.IsAuthorisedInCountry(countryId, ownerId))
            {
                return await dbContext.Modules.AsNoTracking()
                    .Where(m => m.ModuleOwnerships
                    .Any(mo => mo.PersonId == ownerId))
                    .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                    .Include(m => m.Scale)
                    .Include(m => m.Standard)
                    .ToListAsync();
            }
            return Array.Empty<Module>();
        }
        public Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id) =>
            FindByIdAsync(principal, id, 0);

        public async Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id, int personalOwnerId)
        {
            if (principal is not null)
            {
                var ownerPersonId = personalOwnerId > 0 ? personalOwnerId : principal.PersonId();
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Modules.AsNoTracking()
                    .Where(m => m.Id == id && m.ModuleOwnerships.Any(mo => mo.PersonId == ownerPersonId))
                    .Include(m => m.ModuleOwnerships)
                    .Include(m => m.ModuleGables)
                    .SingleOrDefaultAsync();
            }
            return null;
        }
        public Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity) =>
            SaveAsync(principal, entity, 0);

        public async Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity, int owningPersonId)
        {
            // TODO: Make it possible for group data administrator to create and edit other persons module.
            // TODO: Check if principal is data administrator in same group as owner.
            var ownerId = owningPersonId > 0 ? owningPersonId : principal.PersonId();
            if (principal.MaySave(ownerId))
            {
                using var dbContext = Factory.CreateDbContext();
                var existing = await dbContext.Modules.Include(m => m.ModuleGables).FirstOrDefaultAsync(m => m.Id == entity.Id);
                if (existing is null)
                {
                    dbContext.Add(entity);
                }
                else
                {
                    dbContext.Entry(existing).CurrentValues.SetValues(entity);
                    foreach (var gable in entity.ModuleGables)
                    {
                        var existingGable = existing.ModuleGables.AsQueryable().FirstOrDefault(g => g.Id == gable.Id);
                        if (existingGable is null)
                        {
                            existing.ModuleGables.Add(gable);
                        }
                        else
                        {
                            dbContext.Entry(existingGable).CurrentValues.SetValues(gable);
                        }
                    }
                    foreach (var gable in existing.ModuleGables)
                    {
                        if (!entity.ModuleGables.Any(mg => mg.Id == gable.Id)) dbContext.Remove(gable);
                    }
                }

                var result = await dbContext.SaveChangesAsync();
                if (entity.ModuleOwnerships.Count == 0)
                {
                    dbContext.ModuleOwnerships.Add(new ModuleOwnership { ModuleId = entity.Id, PersonId = ownerId, OwnedShare = 1 });
                    result += await dbContext.SaveChangesAsync();
                }

                return result.SaveResult(existing ?? entity);
            }
            return principal.SaveNotAuthorised<Module>();
        }

        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal.MayDelete(principal.PersonId()))
            {
                var entity = await FindByIdAsync(principal, id);
                if (entity is not null && entity.ModuleOwnerships.Any(mo => mo.PersonId == principal.PersonId()))
                {
                    using var dbContext = Factory.CreateDbContext();

                    dbContext.Modules.Remove(entity);
                    var result = await dbContext.SaveChangesAsync();
                    return result.DeleteResult();
                }
            }
            return principal.DeleteNotAuthorized<Module>();
        }

        public async Task<(int Count, string Message)> CloneAsync(ClaimsPrincipal? principal, int id, int owningPersonId)
        {
            var ownerId = owningPersonId > 0 ? owningPersonId : principal.PersonId();

            if (principal.MaySave(ownerId))
            {
                var clone = await FindByIdAsync(principal, id, ownerId);
                if (clone is null) return principal.NotFound();
                clone.FullName = CloneFullName(Random, clone);
                clone.Id = 0;
                clone.Station = null;
                clone.StationId = null;
                foreach (var gable in clone.ModuleGables) gable.Id = 0;
                foreach (var ownership in clone.ModuleOwnerships) ownership.Id = 0;
                using var dbContext = Factory.CreateDbContext();
                dbContext.Modules.Add(clone);
                var result = await dbContext.SaveChangesAsync();
                return result.CloneResult();
            }
            return (0, "Not authorized.");  //TODO: Fix extension with transplations.

            static string CloneFullName(Random random, Module module)
            {
                var appended = $"-{random.Next(1,1000)}";
                var totalLength = module.FullName.Length + appended.Length;
                if (totalLength <= 50) return $"{module.FullName}{appended}";
                return $"{module.FullName.Substring(0, 50 - appended.Length)}{appended}";
            }
        }
    }
}
