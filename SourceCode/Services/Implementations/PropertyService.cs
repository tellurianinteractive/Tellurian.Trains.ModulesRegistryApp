using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class PropertyService : IPropertyService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public PropertyService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

        public Task<IEnumerable<ListboxItem>> GetGableTypeListboxItemsAsync() =>
            GetListboxItemsAsync("GableType");

        private async Task<IEnumerable<ListboxItem>> GetListboxItemsAsync(string type)
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.Properties.AsNoTracking().Where(p => p.Name.Equals(type)).Select(p => new ListboxItem(p.Id, p.Value)).ToListAsync();
            return items.OrderBy(i => i.Description);
        }

    }
}
