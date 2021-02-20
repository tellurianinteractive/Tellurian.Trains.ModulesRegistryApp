using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
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

        public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int countryId)
        {
            if (!principal.IsAuthorisedInCountry(countryId)) return Array.Empty<ListboxItem>();
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.People
                .Where(p => p.CountryId == countryId)
                .ToListAsync();
            return items
                .Select(p => new ListboxItem(p.Id, p.FirstName + " " + p.LastName))
                .OrderBy(li => li.Description)
                .ToList();
        }

        public async Task<IEnumerable<Person>> GetAllInCountryAsync(ClaimsPrincipal? principal, int countryId)
        {
            if (principal.IsAuthorisedInCountry(countryId))
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.People
                    .Where(p => p.CountryId == countryId)
                    .Include(p => p.User)
                    .OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ThenBy(p => p.CityName)
                    .ToListAsync();
            }
            else
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.People
                      .Where(p => p.CountryId == countryId && p.Id == principal.PersonId())
                      .Include(p => p.User)
                      .OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ThenBy(p => p.CityName)
                      .ToListAsync();
            }
        }

        public async Task<Person?> FindByIdAsync(ClaimsPrincipal? principal, int id)
        {
            using var dbContext = Factory.CreateDbContext();
            var person = await dbContext.People.FindAsync(id);
            if (principal.IsAuthorisedInCountry(person.CountryId) || person.Id == principal.PersonId())
            {
                return person;
            }
            return null;
        }

        public async Task<Person?> FindByUserIdAsync(ClaimsPrincipal? principal, int userId)
        {
            using var dbContext = Factory.CreateDbContext();
            var person = await dbContext.People.SingleOrDefaultAsync(p => p.UserId == userId);
            if (principal.IsAuthorisedInCountry(person.CountryId) || person.Id == principal.PersonId())
            {
                return person;
            }
            return null;
        }

        public async Task<(int, string, Person?)> SaveAsync(ClaimsPrincipal? principal, Person person)
        {
            if (principal.IsAuthorisedInCountry(person.CountryId) || person.Id == principal.PersonId())
            {
                using var dbContext = Factory.CreateDbContext();
                dbContext.People.Attach(person);
                dbContext.Entry(person).State = person.Id.GetState();
                var count = await dbContext.SaveChangesAsync();
                return count > 0 ? (count, Strings.Saved, person) : (0, Strings.SaveFailed, null);
            }
            return (0, Strings.NotAuthorized, null);
        }

        public async Task<(int, string)> DeleteAsync(ClaimsPrincipal? principal, int id)
        {
            using var dbContext = Factory.CreateDbContext();
            var isUser = await dbContext.Users.AnyAsync(u => u.Person.Id == id);
            var hasModules = dbContext.ModuleOwnerships.Any(mo => mo.PersonId == id);
            if (isUser || hasModules) return (0, Strings.MayNotBeDeleted);

            var person = await dbContext.People.FindAsync(id);
            if (principal.IsAuthorisedInCountry(person.CountryId))
            {
                if (person is null) return (0, Strings.NothingToDelete);
                dbContext.Remove(person);
                var count = await dbContext.SaveChangesAsync();
                return (count, count > 0 ? Strings.DeletedSuccessfully : Strings.DeleteFailed);
            }
            return (0, Strings.NotAuthorized);
        }
    }
}
