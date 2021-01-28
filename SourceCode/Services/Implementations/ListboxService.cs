using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class ListboxService : IListboxService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public ListboxService(IDbContextFactory<ModulesDbContext>  factory)
        {
            Factory = factory;
        }

        public async Task<IEnumerable<ListboxItem>> Countries()
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Countries
                .Select(c => new ListboxItem(c.Id, c.EnglishName))
                .OrderBy(i => i.Description)
                .ToListAsync();
        }
    }
}
