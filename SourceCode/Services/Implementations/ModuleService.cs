using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class ModuleService : IModuleService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public ModuleService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        public async Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal)
        {
            if (principal is null) return Array.Empty<Module>();
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Modules
                .Where(mo => mo.ModuleOwnerships
                .Any(mo => mo.PersonId == principal.PersonId()))
                .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
                .Include(m => m.Scale)
                .Include(m => m.Standard)
                .ToListAsync();
        }

        public async Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal is not null)
            {
                using var dbContext = Factory.CreateDbContext();
                var module = await dbContext.Modules.Include(m => m.ModuleOwnerships).SingleOrDefaultAsync(m => m.Id == id);
                if (module is null) return null;
                if (module.ModuleOwnerships.Any(mo => mo.PersonId == principal.PersonId())) return module;
            }
            return null;
        }

        public async Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity)
        {
            // TODO: Make it possible for group data administrator to create and edit other persons module.
            var ownerId = principal.PersonId();
            // TODO: Check if principal is data administrator in same group as owner.
            if (principal.MaySave(ownerId))
            {
                using var dbContext = Factory.CreateDbContext();
                dbContext.Modules.Attach(entity);
                dbContext.Entry(entity).State = entity.Id.GetState();
                var result = await dbContext.SaveChangesAsync();
                if (entity.ModuleOwnerships.Count == 0)
                {
                    dbContext.ModuleOwnerships.Add(new ModuleOwnership { ModuleId = entity.Id, PersonId = ownerId, OwnedShare = 1 });
                    result += await dbContext.SaveChangesAsync();
                }
                return result.SaveResult(entity);
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
    }
}
