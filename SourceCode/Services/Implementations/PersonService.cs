using ModulesRegistry.Resources;

namespace ModulesRegistry.Services.Implementations;

public sealed class PersonService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeprovider)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ITimeProvider TimeProvider = timeprovider;

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
                .Select(p => new ListboxItem(p.Id, p.NameWithCityAndCountry()))
                .OrderBy(li => li.Description)
                .ToList();
        }
        return Array.Empty<ListboxItem>();
    }

    /// <summary>
    /// Persons in a country whose name or city contains every word in <paramref name="text"/>, filtered in the database.
    /// Only countries the user is authorised in are searched, as in <see cref="ListboxItemsAsync"/>.
    /// </summary>
    /// <param name="anyCountry">The caller has checked that the user may search persons in any country, as an organiser registering participants for a meeting.</param>
    public async Task<IReadOnlyList<ListboxItem>> SearchAsync(ClaimsPrincipal? principal, int countryId, string? text, int excludeGroupId = 0, int maxResults = 10, bool anyCountry = false, CancellationToken cancellationToken = default)
    {
        var words = PersonSearchExtensions.SearchWords(text);
        if (words.Length == 0 || maxResults < 1 || !principal.IsAuthenticated() || (!anyCountry && !principal.IsAuthorisedInCountry(countryId))) return [];
        using var dbContext = await Factory.CreateDbContextAsync(cancellationToken);
        var persons = await dbContext.People.AsNoTracking()
            .Include(p => p.Country)
            .Where(p => (countryId == 0 || p.CountryId == countryId) && (excludeGroupId == 0 || !p.GroupMembers.Any(gm => gm.GroupId == excludeGroupId)))
            .WhereNameOrCityContainsAllWords(words)
            .OrderBy(p => p.FirstName).ThenBy(p => p.MiddleName).ThenBy(p => p.LastName).ThenBy(p => p.CityName)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
        return persons.Select(p => new ListboxItem(p.Id, p.NameWithCityAndCountry())).ToList();
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
                .Select(p => new ListboxItem(p.Id, p.NameWithCityAndCountry()))
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
                .Include(P => P.Country)
                .Include(p => p.User)
                .OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ThenBy(p => p.CityName)
                .ToListAsync();
        }
        else
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.People
                  .Where(p => p.CountryId == countryId && p.Id == principal.PersonId())  // Limits access to current user.
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
            // Corrected, not validated: names (capitals, ordinary text, a quoted nickname to its own field), email and loco addresses.
            entity.CorrectInput();
            using var dbContext = Factory.CreateDbContext();
            // Checked in the database (Migration step 3.5): the same person registered twice, the same address for two users.
            if (await dbContext.IsRegisteredAgainAsync(entity)) return Strings.PersonAlreadyExists.SaveResult(entity);
            if (await dbContext.AnotherUsersSignInAddressAsync(entity) is { } taken) return string.Format(Strings.EmailAddressIsAnotherUsersSignIn, taken).SaveResult(entity);
            if (entity.EmailAddresses.HasValue() && entity.User is not null)
            {
                entity.User.EmailAddress = entity.EmailAddresses.FirstItem(delimiter: ';');
            }
            if (entity.Id > 0)
            {
                var existing = await dbContext.People.FindAsync(entity.Id);
                if (existing is null) return principal.NothingToUpdate<Person>();
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                var isUserChanged = false;
                if (entity.User is not null)
                {
                    if (entity.User.Id > 0)
                    {
                        var existingUser = await dbContext.Users.FindAsync(entity.User.Id);
                        if (existingUser is null) return principal.NothingToUpdate<Person>();
                        dbContext.Entry(existingUser).CurrentValues.SetValues(entity.User);
                        isUserChanged = dbContext.Entry(existingUser).State != EntityState.Unchanged;
                    }
                    else
                    {
                        existing.User = entity.User;
                        isUserChanged = true;
                    }
                }
                if (!isUserChanged && dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(entity);
            }
            else
            {
                if (entity.User is { Id: > 0 } user)
                {
                    var existingUser = await dbContext.Users.FindAsync(user.Id);
                    if (existingUser is null) return principal.NothingToUpdate<Person>();
                    dbContext.Entry(existingUser).CurrentValues.SetValues(user);
                    entity.User = existingUser;
                }
                dbContext.People.Add(entity);
            }
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
            var person = await dbContext.People
                .Include(p => p.User)
                .SingleOrDefaultAsync(p => p.Id == id);
            if (person is null) return (0, Strings.NothingToDelete);
            // Add check rules before any delete here
            if (dbContext.MeetingParticipants.Any(mp => mp.PersonId == person.Id && mp.CancellationTime.HasValue == false && mp.Meeting.EndDate >= TimeProvider.LocalTime)) return (0, Strings.MayNotBeDeleted);
            if (dbContext.Meetings.Any(m => m.EndDate >= TimeProvider.LocalTime && m.OrganiserGroup.GroupMembers.Any(gm => gm.PersonId == id))) return (0, Strings.MayNotBeDeleted);


            if (principal.IsAuthorisedInCountry(person.CountryId))
            {
                person.DeletedTimestamp = TimeProvider.Now;
                if (person.User is not null) person.User.DeletedTimestamp = TimeProvider.Now;
                var count = await dbContext.SaveChangesAsync();
                return count.DeleteResult();
            }
        }
        return principal.NotAuthorized<Person>();
    }
}
