using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class ModuleGableTypeService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public ModuleGableTypeService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(int? scaleId)
        {
            var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleGableTypes.AsNoTracking()
                .Where(mgt => !scaleId.HasValue || mgt.ScaleId == scaleId)
                .Select(mgt => new ListboxItem(mgt.Id, mgt.Designation))
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<ModuleGableType>> GetAllAsync()
        {
            var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleGableTypes.AsNoTracking()
                .Include(mgt => mgt.Scale)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<ModuleGableType?> FindByIdAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal.IsAnyAdministrator())
            {
                var dbContext = Factory.CreateDbContext();
                return await dbContext.ModuleGableTypes.FindAsync(id).ConfigureAwait(false);
            }
            return null;
        }

        public async Task<(int Count, string Message, ModuleGableType? Entity)> SaveAsync(ClaimsPrincipal? principal, ModuleGableType entity)
        {
            if (principal.IsAnyAdministrator())
            {
                var dbContext = Factory.CreateDbContext();
                var existing = await dbContext.ModuleGableTypes.FindAsync(entity.Id).ConfigureAwait(false);
                if (existing is null)
                {
                    dbContext.ModuleGableTypes.Add(entity);
                }
                else
                {
                    dbContext.Entry(existing).CurrentValues.SetValues(entity);
                    if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
                }
                var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
                return result.SaveResult(existing ?? entity);
            }
            return principal.SaveNotAuthorised<ModuleGableType>();
        }
    }
}
