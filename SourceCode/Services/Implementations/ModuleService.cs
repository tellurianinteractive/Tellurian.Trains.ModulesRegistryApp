using Microsoft.EntityFrameworkCore;
using ModulesInventory.Services;
using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class ModuleService : IModuleService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public ModuleService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }
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
    }


}
