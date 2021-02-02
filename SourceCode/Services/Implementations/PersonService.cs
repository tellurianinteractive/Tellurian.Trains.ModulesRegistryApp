using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Resources;
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

        public async Task<IEnumerable<Person>> GetAllInCountryAsync(int inCountryId)
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

        public async Task<(int, string, Person?)> SaveAsync(Person person)
        {
            using var dbContext = Factory.CreateDbContext();
            dbContext.People.Attach(person);
            dbContext.Entry(person).State = person.Id.GetState();
            var count = await dbContext.SaveChangesAsync();
            return count > 0 ? (count, Strings.Saved, person) : (0, Strings.SaveFailed, null);
        }

        public async Task<(int, string)> DeleteAsync(int id)
        {
            using var dbContext = Factory.CreateDbContext();
            var isUser = await dbContext.Users.AnyAsync(u => u.Person.Id == id);
            var hasModules = dbContext.ModuleOwnerships.Any(mo => mo.PersonId == id);
            if (isUser || hasModules) return (0, Strings.MayNotBeDeleted);

            var person = await dbContext.People.FindAsync(id);
            if (person is null) return (0, Strings.NothingToDelete);
            dbContext.Remove(person);
            var count =await dbContext.SaveChangesAsync();
            return (count, count > 0 ? Strings.DeletedSuccessfully : Strings.DeleteFailed);
        }
    }
}
