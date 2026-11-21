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

    /// <summary>Who may invite the person to become a user (<see cref="PersonPermissionExtensions.MayInvitePersonAsync"/>).</summary>
    public async Task<bool> MayInviteAsync(ClaimsPrincipal? principal, Person person)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayInvitePersonAsync(principal, person);
    }

    /// <summary>
    /// Whether the user may edit the stored <paramref name="person"/> (or add a new one): a global administrator, a country
    /// administrator in the person's country, the person themselves, or, with <paramref name="groupId"/>, one who administers that
    /// group's members (<see cref="GroupPermissionExtensions.MayAdministerGroupMembersAsync"/>) when the person is new or a member of
    /// the group. Never a read-only user.
    /// </summary>
    /// <remarks>
    /// Before release 1.15.28 saving needed only a country administrator in any country, the person themselves, or a group
    /// administrator flag the page computed from the group in the address, without checking that the person was in that group.
    /// </remarks>
    public async Task<bool> MayEditAsync(ClaimsPrincipal? principal, Person person, int groupId = 0)
    {
        using var dbContext = await Factory.CreateDbContextAsync();
        return await dbContext.MayEditPersonAsync(principal, person, groupId);
    }

    /// <summary>
    /// Saves a person, and the user's rights when the person is a user. Who may save: <see cref="MayEditAsync"/>. The country may be
    /// set by a global administrator, a country administrator to their own country, and a group's member administrator (members may
    /// come from other countries); the person themselves keeps it. Rights the signed-in user may not change keep their stored values
    /// (<see cref="PersonPermissionExtensions.KeepRightsNotChangeableBy"/>).
    /// </summary>
    public async Task<(int Count, string Message, Person? Entity)> SaveAsync(ClaimsPrincipal? principal, Person entity, int groupId = 0)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = entity.Id > 0 ? await dbContext.People.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == entity.Id) : null;
        if (entity.Id > 0 && existing is null) return principal.NothingToUpdate<Person>();
        if (!await dbContext.MayEditPersonAsync(principal, existing ?? entity, groupId)) return principal.SaveNotAuthorised<Person>();
        if (!principal.MaySetPersonCountry(entity.CountryId, existing?.CountryId, groupId > 0)) return principal.SaveNotAuthorised<Person>();
        // Corrected, not validated: names (capitals, ordinary text, a quoted nickname to its own field), email and loco addresses.
        entity.CorrectInput();
        // Checked in the database (Migration step 3.5): the same person registered twice, the same address for two users.
        if (await dbContext.IsRegisteredAgainAsync(entity)) return Strings.PersonAlreadyExists.SaveResult(entity);
        if (await dbContext.AnotherUsersSignInAddressAsync(entity) is { } taken) return string.Format(Strings.EmailAddressIsAnotherUsersSignIn, taken).SaveResult(entity);
        if (entity.EmailAddresses.HasValue && entity.User is not null)
        {
            entity.User.EmailAddress = entity.EmailAddresses.FirstItem(separator: ';');
        }
        if (existing is not null)
        {
            var isUserChanged = false;
            if (entity.User is not null)
            {
                // Only the person's own user, and only the rights the signed-in user may change.
                if (existing.User is null || entity.User.Id != existing.User.Id) return principal.SaveNotAuthorised<Person>();
                entity.User.KeepRightsNotChangeableBy(principal, existing.User, existing.CountryId);
                existing.User.SetRightsAndSignInAddressFrom(entity.User);
                isUserChanged = dbContext.Entry(existing.User).State != EntityState.Unchanged;
            }
            entity.UserId = existing.UserId; // The user is linked by invitation, never by the form.
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (!isUserChanged && dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(entity);
        }
        else
        {
            entity.User = null;
            entity.UserId = null;
            dbContext.People.Add(entity);
        }
        var count = await dbContext.SaveChangesAsync();
        return count.SaveResult(existing ?? entity);
    }

    /// <summary>
    /// What deleting the person removes and changes (<see cref="PersonDeletionPlan"/>), or null when there is no such person or
    /// the user may not delete it (<see cref="PersonPermissionExtensions.MayDeletePerson"/>).
    /// </summary>
    public async Task<PersonDeletion?> GetDeletionAsync(ClaimsPrincipal? principal, int id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await Factory.CreateDbContextAsync(cancellationToken);
        if (!await MayDeleteAsync(dbContext, principal, id, cancellationToken)) return null;
        var plan = await PersonDeletionPlan.LoadAsync(dbContext, id, principal.PersonId(), TimeProvider.LocalTime, cancellationToken);
        return plan?.AsPersonDeletion();
    }

    /// <summary>
    /// Deletes the person and everything that belongs to the person (<see cref="PersonDeletionPlan"/>), in one transaction.
    /// </summary>
    /// <remarks>
    /// Before release 1.15.29 a person was only marked as deleted (<see cref="Person.DeletedTimestamp"/>), with its user, and
    /// everything that referred to it stayed; such persons are deleted here too.
    /// </remarks>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await Factory.CreateDbContextAsync(cancellationToken);
        if (!await MayDeleteAsync(dbContext, principal, id, cancellationToken)) return principal.NotAuthorized<Person>();
        var plan = await PersonDeletionPlan.LoadAsync(dbContext, id, principal.PersonId(), TimeProvider.LocalTime, cancellationToken);
        if (plan is null) return Strings.NothingToDelete.DeleteResult();
        if (plan.Obstacles.Count > 0) return string.Join(" ", plan.Obstacles).DeleteResult();
        var count = await plan.ApplyAsync(dbContext, cancellationToken);
        return count.DeleteResult();
    }

    /// <summary>
    /// Deletes one module the person owns alone, from the person's delete page, with its station unless another module refers to
    /// it (<see cref="PersonDeletionPlan.LoadForModuleAsync"/>). Not when it, or its station, is registered for a meeting that has
    /// not ended.
    /// </summary>
    public async Task<(int Count, string Message)> DeleteOwnedModuleAsync(ClaimsPrincipal? principal, int id, int moduleId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await Factory.CreateDbContextAsync(cancellationToken);
        if (!await MayDeleteAsync(dbContext, principal, id, cancellationToken)) return principal.NotAuthorized<Person>();
        var plan = await PersonDeletionPlan.LoadForModuleAsync(dbContext, id, moduleId, TimeProvider.LocalTime, cancellationToken);
        if (plan is null) return Strings.NothingToDelete.DeleteResult();
        if (plan.Obstacles.Count > 0) return string.Join(" ", plan.Obstacles).DeleteResult();
        var count = await plan.ApplyAsync(dbContext, cancellationToken);
        return count.DeleteResult();
    }

    /// <summary>
    /// The persons in the country that were only marked as deleted before release 1.15.29, for the country administrator to
    /// delete for real. Remove this when there are none left in production, together with the query filter on persons.
    /// </summary>
    public async Task<IReadOnlyList<Person>> GetMarkedAsDeletedInCountryAsync(ClaimsPrincipal? principal, int countryId, CancellationToken cancellationToken = default)
    {
        if (!principal.MayDeletePerson(countryId)) return [];
        await using var dbContext = await Factory.CreateDbContextAsync(cancellationToken);
        return await dbContext.People.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.CountryId == countryId && p.DeletedTimestamp != null)
            .OrderBy(p => p.FirstName).ThenBy(p => p.LastName).ThenBy(p => p.CityName)
            .ToListAsync(cancellationToken);
    }

    private static async Task<bool> MayDeleteAsync(ModulesDbContext dbContext, ClaimsPrincipal? principal, int id, CancellationToken cancellationToken)
    {
        var countryId = await dbContext.People.IgnoreQueryFilters().AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => (int?)p.CountryId)
            .SingleOrDefaultAsync(cancellationToken);
        return countryId.HasValue && principal.MayDeletePerson(countryId.Value);
    }
}