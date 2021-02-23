using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class CountryService : ICountryService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        private readonly ResourceManager ResourceManager;
        public CountryService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
            ResourceManager = Resources.Strings.ResourceManager;
        }

        public async Task<Country?> FindById(int id)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Countries.FindAsync(id);
        }

        public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal)
        {
            if (principal is null) return Array.Empty<ListboxItem>();
            using var dbContext = Factory.CreateDbContext();
            var countries = await dbContext.Countries.ToListAsync();
            return countries.AsEnumerable().Where(c => principal.IsAuthorisedInCountry(c.Id)).Select(c => new ListboxItem(c.Id, LocalizedName(c))).ToList();
        }

        private string LocalizedName(Country country) =>
            ResourceManager.GetString(country.EnglishName) ?? country.EnglishName;
    }
}
