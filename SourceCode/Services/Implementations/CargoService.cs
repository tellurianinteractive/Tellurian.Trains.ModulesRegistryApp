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
    public class CargoService 
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public CargoService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        public async Task<IEnumerable<ListboxItem>> GargoListboxItemsAsync(ClaimsPrincipal? principal)
        {
            if (principal.IsAuthenticated())
            {
                using var dbContext = Factory.CreateDbContext();
                var items = await dbContext.Cargos
                    .Select(c => new ListboxItem(c.Id, c.LocalizedName().Value)).ToListAsync();
                return items.OrderBy(l => l.Description).ToList();                  
            }
            return Array.Empty<ListboxItem>();
        }

        public async Task<IEnumerable<ListboxItem>> CargoDirectionsListboxItemsAsync(ClaimsPrincipal? principal)
        {
            if (principal.IsAuthenticated())
            {
                using var dbContext = Factory.CreateDbContext();
                var items = await dbContext.CargoDirections
                    .Select(cd => new ListboxItem(cd.Id, cd.FullName.Localized())).ToListAsync();;
                return items
                    .OrderBy(l => l.Description).ToList();               
            }
            return Array.Empty<ListboxItem>();
        }

        public async Task<IEnumerable<ListboxItem>> CargoQualtityListboxItemsAsync(ClaimsPrincipal? principal)
        {
            if (principal.IsAuthenticated())
            {
                using var dbContext = Factory.CreateDbContext();
                var items = await dbContext.CargoUnits
                    .Select(cu => new ListboxItem(cu.Id, cu.FullName.Localized())).ToListAsync();
                return items.OrderBy(l => l.Description).ToList();                 
            }
            return Array.Empty<ListboxItem>();
        }

        public async Task<IEnumerable<ListboxItem>> ReadyTimeListboxItemsAsync(ClaimsPrincipal? principal)
        {
            if (principal.IsAuthenticated())
            {
                using var dbContext = Factory.CreateDbContext();
                var items = await dbContext.CargoReadyTimes.Where(crt => crt.IsSpecifiedInLayoyt==true)
                    .Select(crt => new ListboxItem(crt.Id, crt.FullName.Localized())).ToListAsync();
                return items.OrderBy(l => l.Description).ToList();
            }
            return Array.Empty<ListboxItem>();
        }

        public async Task<IEnumerable<Cargo>> GetAll(ClaimsPrincipal? principal)
        {
            if (principal is not null && principal.IsAnyAdministrator())
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Cargos.ToListAsync();
            }
            return Array.Empty<Cargo>();
        }

        public async Task<Cargo?> FindByIdAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal is null) return null;
            using var dbContext = Factory.CreateDbContext();
            if (principal.IsAnyAdministrator()) return await dbContext.Cargos.FindAsync(id);
            return null;
        }

        public async Task<(int Count, string Message, Cargo? Entity)> SaveAsync(ClaimsPrincipal? principal, Cargo entity)
        {
            if (principal is null) return principal.SaveNotAuthorised<Cargo>();
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.Cargos.FindAsync(entity.Id);
            if (existing is null)
            {
                dbContext.Cargos.Add(entity);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(entity);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(existing);
            }
        }
        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal is null) return principal.DeleteNotAuthorized<Cargo>();
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.Cargos.FindAsync(id);
            if (existing is null) return principal.NotFound();
            dbContext.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }

        #region NHM

        public async Task<IEnumerable<ListboxItem>> GetNhmItems(ClaimsPrincipal? principal, int? subItemsToId = null)
        {
            if (principal is not null)
            {
                using var dbContext = Factory.CreateDbContext();
                if (subItemsToId.HasValue)
                {
                    if (subItemsToId.Value == 0) return await dbContext.NhmCodes.Where(c => c.LevelDigits <= 4).OrderBy(c => c.Id).Select(c => ListboxItem(c)).ToListAsync();
                    var min = subItemsToId + 1;
                    var max = min + 999999;
                    return await dbContext.NhmCodes.Where(c => c.Id >= min && c.Id <= max).OrderBy(c => c.Id).Select(c => ListboxItem(c)).ToListAsync();
                }
                else
                {
                    return await dbContext.NhmCodes.Where(c => c.LevelDigits == 2).OrderBy(c => c.Id).Select(c => ListboxItem(c)).ToListAsync();
                }
            }
            return Array.Empty<ListboxItem>();

        }

        private static ListboxItem ListboxItem(NHM nhm) => new (nhm.Id, $"{nhm.Code!.Substring(0, nhm.LevelDigits)} {nhm.LocalizedName()}");

        #endregion
    }
}
