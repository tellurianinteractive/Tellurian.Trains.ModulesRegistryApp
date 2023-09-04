using ModulesRegistry.Data;
using ModulesRegistry.Services.Resources;

namespace ModulesRegistry.Services.Implementations;

public sealed class PersonService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public PersonService(IDbContextFactory<ModulesDbContext> factory)
    {
        Factory = factory;
    }

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int countryId, int excludeGroupId = 0)
    {
        if (principal.IsAuthorisedInCountry(countryId))
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.People.AsNoTracking()
                .Include(p => p.Country)
                .Where(p => (countryId == 0 || p.CountryId == countryId) && (excludeGroupId == 0 || !p.GroupMembers.Any(gm => gm.GroupId == excludeGroupId && gm.PersonId == p.Id)))
                .ToListAsync();
            return items
                .Select(p => new ListboxItem(p.Id, p.NameCityAndCountry()))
                .OrderBy(li => li.Description)
                .ToList();
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<ListboxItem>> FremoMembersListboxItemsAsync(ClaimsPrincipal? principal, int countryId = 0, int personId = 0)
    {
        personId = principal.IsGlobalAdministrator() || principal.MayManageWiFreds() ? personId : principal.PersonId();
        countryId = principal.IsGlobalAdministrator() || principal.MayManageWiFreds() ? countryId : principal.CountryId();
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.People.AsNoTracking()
                .Include(p => p.Country)
                .Where(p => (personId == 0 || p.Id == personId) && (countryId == 0 || p.CountryId == countryId))
                .ToReadOnlyListAsync();
            return items
                .Select(p => new ListboxItem(p.Id, p.NameCityAndCountry()))
                .OrderBy(li => li.Description)
                .ToList();
        }
        return Array.Empty<ListboxItem>();
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
        if (principal.MayRead(id))
        {
            using var dbContext = Factory.CreateDbContext();
            var person = await dbContext.People.Include(p => p.User).Include(p => p.Country).SingleOrDefaultAsync(p => p.Id == id);
            if (person is null) return null;
            if (principal.MayRead(person.Id))
            {
                return person;
            }
        }
        return null;
    }

    public async Task<Person?> FindByUserIdAsync(ClaimsPrincipal? principal, int userId)
    {
        using var dbContext = Factory.CreateDbContext();
        var person = await dbContext.People.SingleOrDefaultAsync(p => p.UserId == userId);
        if (person is not null && principal.MayRead(person.Id))
        {
            return person;
        }
        return null;
    }

    public async Task<(int Count, string Message, Person? Entity)> SaveAsync(ClaimsPrincipal? principal, Person entity, bool isGroupAdministrator = false)
    {
        var ownerRef = ModuleOwnershipRef.Person(entity.Id);
        if (principal.MaySave(ownerRef, isGroupAdministrator))
        {
            using var dbContext = Factory.CreateDbContext();
            dbContext.People.Attach(entity);
            if (entity.User is not null) dbContext.Users.Attach(entity.User);
            if (entity.EmailAddresses.HasValue() && entity.User is not null)
            {
                entity.User!.EmailAddress = entity.EmailAddresses.FirstItem(delimiter: ';');
            }
            dbContext.Entry(entity).State = entity.Id.GetState();
            if (entity.User is not null)  dbContext.Entry(entity.User).State = entity.User.Id.GetState();
            var count = await dbContext.SaveChangesAsync();
            return count.SaveResult(entity);
        }
        return principal.SaveNotAuthorised<Person>();
    }

    

    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.MayDelete(principal.OwnerRef()))
        {
            using var dbContext = Factory.CreateDbContext();
            var isUser = await dbContext.Users.AnyAsync(u => u.Person.Id == id);
            var hasModules = dbContext.ModuleOwnerships.Any(mo => mo.PersonId == id);
            if (isUser || hasModules) return (0, Strings.MayNotBeDeleted);

            var person = await dbContext.People.Include(p => p.GroupMembers).SingleOrDefaultAsync(p => p.Id == id);
            if (person is null) return (0, Strings.NothingToDelete);
            if (principal.IsAuthorisedInCountry(person.CountryId))
            {
                foreach (var membership in person.GroupMembers)
                {
                    dbContext.Remove(membership);
                }
                //await dbContext.SaveChangesAsync();
                dbContext.Remove(person);
                var count = await dbContext.SaveChangesAsync();
                return count.DeleteResult();
            }
        }
        return principal.NotAuthorized<Person>();
    }
}
