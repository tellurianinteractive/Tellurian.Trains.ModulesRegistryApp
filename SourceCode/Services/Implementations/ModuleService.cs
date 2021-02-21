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

        public IAsyncEnumerable<Module> GetModulesAsync()
        {
            using var dbContext = Factory.CreateDbContext();
            return dbContext.Modules
                .Include(t => t.ModuleOwnerships).ThenInclude(mo => mo.Person).AsAsyncEnumerable();
        }

        public async Task<Module> GetModuleAsync(int id)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Modules.Include(m => m.ModuleOwnerships).SingleOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<ListboxItem>> GetModulesListboxItems()
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Modules.Select(m => new ListboxItem(m.Id, $"{m.FullName}")).ToListAsync();
        }

        public async Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal)
        {
            if (principal is null) return Array.Empty<Module>();
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Modules.Where(mo => mo.ModuleOwnerships.Any(mo => mo.PersonId == principal.PersonId())).Include(m => m.ModuleOwnerships).ToListAsync();
        }

        public async Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal is not null)
            {
                using var dbContext = Factory.CreateDbContext();
                var module = await dbContext.Modules.SingleOrDefaultAsync(m => m.Id == id);
                if (module is null) return null;
                if (module.ModuleOwnerships.Any(mo => mo.PersonId == principal.PersonId())) return module;
            }
            return null;
        }

        public async Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity)
        {
            if (principal.MaySave(principal.PersonId()))
            {
                if (entity.ModuleOwnerships.Count == 0)
                {
                    entity.ModuleOwnerships.Add(new ModuleOwnership { Module = entity, PersonId = principal.PersonId(), OwnedShare = 1 });
                }
                if (entity.ModuleOwnerships.Any(mo => mo.PersonId == principal.PersonId()))
                {
                    using var dbContext = Factory.CreateDbContext();
                    dbContext.Modules.Attach(entity);
                    var result = await dbContext.SaveChangesAsync();
                    return result.SaveResult(entity);
                }
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
