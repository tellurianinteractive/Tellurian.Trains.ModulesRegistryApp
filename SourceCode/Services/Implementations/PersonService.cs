using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class PersonService : IPersonService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public PersonService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public async Task<IEnumerable<Person>> GetAllAsync(int inCountryId)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.People
                .Where(p => p.CountryId == inCountryId)
                .Include(p => p.User)
                .OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ThenBy(p => p.CityName)
                .ToListAsync();
        }

        public async Task<Person?> FindAsync(int id)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.People.FindAsync(id);
        }

        public async Task<Person?> GetByAsync(int userId)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.People.SingleOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
