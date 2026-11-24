using ModulesRegistry.Resources;

namespace ModulesRegistry.Services.Implementations;

public sealed class GroupService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int? countryId, int? onlyGroupId = null)
    {
        using var dbContext = Factory.CreateDbContext();
        var id = principal.IsGlobalOrCountryAdministrator() ? 0 : countryId ?? principal.CountryId();
        return await dbContext.Groups
            .Where(g =>  (g.CountryId == id || id == 0) && (!onlyGroupId.HasValue || onlyGroupId==0 || onlyGroupId > 0 && g.Id == onlyGroupId.Value))
            .OrderBy(g => g.FullName)
            .Select(g => new ListboxItem(g.Id, g.FullName))
            .ToReadOnlyListAsync();
    }

    /// <summary>The groups in the country, for everyone signed in, also administrators of other countries.</summary>
    public async Task<IEnumerable<ListboxItem>> CountryListboxItemsAsync(ClaimsPrincipal? principal, int countryId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Groups
                .Where(g => g.CountryId == countryId)
                .OrderBy(g => g.FullName)
                .Select(g => new ListboxItem(g.Id, g.FullName))
                .ToReadOnlyListAsync();
        }
        return [];
    }

    public async Task<IEnumerable<ListboxItem>> MemberListboxItemsAsync(ClaimsPrincipal? principal, int? groupId)
    {
        if (principal.IsAuthenticated() && groupId > 0 == true)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.GroupMembers
                .Where(gm => gm.GroupId == groupId.Value)
                .Include(gm => gm.Person)
                .Select(gd => new ListboxItem(gd.PersonId, gd.Person.Name()))
                .ToReadOnlyListAsync();
        }
        return Array.Empty<ListboxItem>();

    }
    public async Task<IEnumerable<ListboxItem>> GroupDomainListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.GroupDomains.AsNoTracking().Select(gd => new ListboxItem(gd.Id, gd.Name)).ToListAsync();
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<(Group Group, bool MayEdit)>> GetAllAsync(ClaimsPrincipal? principal, int countryId)
    {
        if (principal.IsGlobalAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.Groups.AsNoTracking()
                .Where(g => g.CountryId == countryId)
                .Include(g => g.GroupDomain)
                .Include(g => g.Country)
                .Include(g => g.GroupMembers)
                .OrderBy(g => g.FullName)
                .ToListAsync();
            return items.Select(i => (i, true));
        }
        else
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.Groups.AsNoTracking()
                .Where(g => (g.GroupDomainId > 0 && principal.GroupDomainIds().Contains(g.GroupDomainId.Value)) || g.GroupMembers.Any(gm => gm.PersonId == principal.PersonId()))
                .Include(g => g.GroupDomain)
                .Include(g => g.Country)
                .Include(g => g.GroupMembers)
                .OrderBy(g => g.FullName)
                .ToListAsync();
            return items.Select(i => (i, principal.IsCountryAdministratorInCountry(i.CountryId) || i.GroupMembers.Any(gm => (gm.IsDataAdministrator || gm.IsGroupAdministrator) && gm.PersonId == principal.PersonId())));
        }
    }

    public async Task<Group?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var group = await dbContext.Groups.AsNoTracking()
                 .Include(g => g.GroupMembers).ThenInclude(gm => gm.Person).ThenInclude(p => p.User)
                 .Where(g => g.Id == id)
                 .ToListAsync();
            return group.SingleOrDefault(g => principal.IsCountryAdministratorInCountry(g.CountryId) || principal.IsMemberOfGroupSpecificGroupDomainOrNone(g.GroupDomainId) || g.GroupMembers.Any(gm => gm.PersonId == principal.PersonId()));
        }
        return null;
    }

    public async Task<GroupMember?> FindMemberByIdAsync(ClaimsPrincipal? principal, int memberId)
    {
        using var dbContext = Factory.CreateDbContext();
        var groupMember = await dbContext.GroupMembers.AsNoTracking()
            .Where(gm => gm.Id == memberId)
            .Include(gm => gm.Group)
            .Include(gm => gm.Person)
            .SingleOrDefaultAsync();
        if (groupMember is null) return null;
        if (await IsGroupMemberAdministratorAsync(principal, groupMember.GroupId, groupMember.Group.CountryId)) return groupMember;
        return null;
    }

    public async ValueTask<bool> IsGroupDataAdministratorAsync(ClaimsPrincipal? pricipal, int groupId, int? countryId = null)
    {
        if (pricipal.IsCountryAdministratorInCountry(countryId)) return true;
        else
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.GroupMembers.AsNoTracking()
                .AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == pricipal.PersonId() && gm.IsDataAdministrator);
        }
    }
    /// <summary>
    /// Whether the user administers the group's members: a global administrator, a country administrator in the group's country
    /// (<paramref name="countryId"/>, looked up when not given), or one of the group's administrators. Never a read-only user.
    /// </summary>
    public async ValueTask<bool> IsGroupMemberAdministratorAsync(ClaimsPrincipal? pricipal, int groupId, int? countryId = null)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.MayAdministerGroupMembersAsync(pricipal, groupId, countryId);
    }

    public async ValueTask<bool> IsDataAdministratorInSameGroupAsMember(ClaimsPrincipal? pricipal, int memberPersonId)
    {
        if (pricipal.IsGlobalAdministrator()) return true;
        else if (pricipal.IsCountryAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            var countryIds = await dbContext.Groups.AsNoTracking()
                .Where(g => g.GroupMembers.Any(gm => gm.PersonId == memberPersonId))
                .Select(g => g.CountryId)
                .ToListAsync();
            return countryIds.Any(c => c == pricipal.CountryId());
        }
        else
        {
            using var dbContext = Factory.CreateDbContext();
            var adminGroups = await dbContext.Groups.AsNoTracking()
                .Include(g => g.GroupMembers)
                .Where(g => g.GroupMembers.Any(gm => gm.IsDataAdministrator && gm.PersonId == pricipal.PersonId()))
                .ToListAsync();
            if (adminGroups is null || adminGroups.Count == 0) return false;
            return adminGroups.Any(ag => pricipal.IsCountryAdministratorInCountry(ag.CountryId) || ag.GroupMembers.Any(gm => gm.PersonId == memberPersonId));
        }
    }

    public bool IsMemberInGroupsInSameDomain(ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef)
    {
        using var dbContext = Factory.CreateDbContext();
        return IsMemberInGroupsInSameDomain(dbContext, principal, ownershipRef);
    }

    internal static bool IsMemberInGroupsInSameDomain(ModulesDbContext dbContext, ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef)
    {
        if (principal.IsAuthenticated() && ownershipRef.IsPerson)
        {
            if (principal.PersonId() == ownershipRef.PersonId) return false;

            var memberships = dbContext.GroupMembers.AsNoTracking().Include(gm => gm.Group)
                .Where(gm => gm.Group.GroupDomainId > 0 && (gm.PersonId == ownershipRef.PersonId || gm.PersonId == principal.PersonId()))
                .ToList()
                .GroupBy(gm => gm.Group.GroupDomainId)
                .ToList();
            return memberships.Any(m => m.Count(gm => gm.PersonId == ownershipRef.PersonId || gm.PersonId == principal.PersonId()) > 1);
        }
        return false;
    }

    /// <summary>
    /// Whether the user may edit the group's own fields: a global administrator, a country administrator in the group's country,
    /// or one of the group's group or data administrators (the same rule as Edit in the groups list). Never a read-only user.
    /// </summary>
    public static bool MayEdit(ClaimsPrincipal? principal, Group group) =>
        principal is not null && !principal.IsReadOnly() &&
        (principal.IsCountryAdministratorInCountry(group.CountryId) || principal.IsAnyGroupAdministrator(group));

    /// <summary>
    /// Adds or updates a group (Migration step 3.6; before, any user who wasn't read-only could save any group). A new group is
    /// added by a global administrator, or a country administrator in its country. An existing group is saved by those who
    /// <see cref="MayEdit"/> it; only a global administrator moves it to another country, and only global and country
    /// administrators change its domain (which decides whose modules its members see).
    /// </summary>
    public async Task<(int Count, string Message, Group? Entity)> SaveAsync(ClaimsPrincipal? principal, Group group)
    {
        if (principal is null || principal.IsReadOnly()) return principal.SaveNotAuthorised<Group>();
        group.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        using var dbContext = Factory.CreateDbContext();
        if (group.Id == 0)
        {
            if (!principal.IsCountryAdministratorInCountry(group.CountryId)) return principal.SaveNotAuthorised<Group>();
            group.Country = null!; group.GroupDomain = null!; // Only the ids are saved.
            dbContext.Groups.Add(group);
            return (await dbContext.SaveChangesAsync()).SaveResult(group);
        }
        var existing = await dbContext.Groups.Include(g => g.GroupMembers).SingleOrDefaultAsync(g => g.Id == group.Id);
        if (existing is null) return principal.NothingToUpdate<Group>();
        if (!MayEdit(principal, existing)) return principal.SaveNotAuthorised<Group>();
        if (!principal.IsGlobalAdministrator()) group.CountryId = existing.CountryId;
        if (!principal.IsCountryAdministratorInCountry(existing.CountryId)) group.GroupDomainId = existing.GroupDomainId;
        dbContext.Entry(existing).CurrentValues.SetValues(group);
        if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
        return (await dbContext.SaveChangesAsync()).SaveResult(existing);
    }

    public async Task<(int Count, string Message, GroupMember? Entity)> SaveMemberAsync(ClaimsPrincipal? principal, GroupMember entity)
    {
        using var dbContext = Factory.CreateDbContext();
        var countryId = (await dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == entity.GroupId))?.CountryId;
        if (await IsGroupMemberAdministratorAsync(principal, entity.GroupId, countryId))
        {
            if (entity.Id > 0)
            {
                // Only a member of the group the user administers; its group and person stay (Migration step 3.6).
                var existing = await dbContext.GroupMembers.SingleOrDefaultAsync(gm => gm.Id == entity.Id && gm.GroupId == entity.GroupId);
                if (existing is null) return principal.NothingToUpdate<GroupMember>();
                entity.PersonId = existing.PersonId;
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(entity);
            }
            else
            {
                dbContext.GroupMembers.Attach(entity);
                dbContext.Entry(entity).State = EntityState.Added;
            }
            var count = await dbContext.SaveChangesAsync();
            return count.SaveResult(entity);
        }
        return principal.SaveNotAuthorised<GroupMember>();
    }

    /// <summary>
    /// The groups among <paramref name="groupIds"/> that <paramref name="principal"/> may delete: a global administrator,
    /// or a country administrator in the group's country, and only a group that has no members, no modules and no meetings
    /// (nor layouts at meetings, which also refer to the group), and that no locality is used by.
    /// </summary>
    public async Task<IReadOnlySet<int>> DeletableIdsAsync(ClaimsPrincipal? principal, IEnumerable<int> groupIds)
    {
        if (principal is null || principal.IsReadOnly()) return new HashSet<int>();
        var ids = groupIds.ToList();
        if (ids.Count == 0) return new HashSet<int>();
        using var dbContext = Factory.CreateDbContext();
        var groups = await Unreferenced(dbContext)
            .Where(g => ids.Contains(g.Id))
            .Select(g => new { g.Id, g.CountryId })
            .ToListAsync();
        return groups.Where(g => principal.IsGlobalOrCountryAdministrator(g.CountryId)).Select(g => g.Id).ToHashSet();
    }

    /// <summary>
    /// Deletes a group, under the same conditions as <see cref="DeletableIdsAsync"/>.
    /// </summary>
    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal is null || principal.IsReadOnly()) return principal.NotAuthorized<Group>();
        using var dbContext = Factory.CreateDbContext();
        var group = await dbContext.Groups.SingleOrDefaultAsync(g => g.Id == id);
        if (group is null) return (0, Strings.NothingToDelete);
        if (!principal.IsGlobalOrCountryAdministrator(group.CountryId)) return principal.NotAuthorized<Group>();
        if (!await Unreferenced(dbContext).AnyAsync(g => g.Id == id)) return (0, Strings.MayNotBeDeleted);
        dbContext.Groups.Remove(group);
        var count = await dbContext.SaveChangesAsync();
        return count.DeleteResult();
    }

    // Groups that nothing refers to, so they can be deleted without breaking a foreign key.
    private static IQueryable<Group> Unreferenced(ModulesDbContext dbContext) =>
        dbContext.Groups.Where(g =>
            !g.GroupMembers.Any() &&
            !g.ModuleOwnerships.Any() &&
            !dbContext.Meetings.Any(m => m.OrganiserGroupId == g.Id) &&
            !dbContext.Layouts.Any(l => l.OrganisingGroupId == g.Id) &&
            !dbContext.GroupLayoutModules.Any(glm => glm.GroupId == g.Id) &&
            !dbContext.Localities.Any(l => l.UsedByGroupId == g.Id));

    public async Task<(int Count, string Message, GroupMember? Member)> AddMemberAsync(ClaimsPrincipal? principal, GroupMember groupMember)
    {
        using var dbContext = Factory.CreateDbContext();
        // Only the group's member administrators (before, any user could: the extension's country check was always true).
        if (groupMember.PersonId > 0 && await IsGroupMemberAdministratorAsync(principal, groupMember.GroupId))
        {
            groupMember.Group = null!; groupMember.Person = null!; // Only the ids are saved.
            var existing = dbContext.GroupMembers.SingleOrDefault(gm => gm.PersonId == groupMember.PersonId && gm.GroupId == groupMember.GroupId);
            if (existing is not null) return existing.AlreadyExists();
            dbContext.GroupMembers.Add(groupMember);
            var result = await dbContext.SaveChangesAsync();
            return result.SaveResult(groupMember);
        }
        return principal.SaveNotAuthorised<GroupMember>();
    }

    public async Task<(int Count, string Message)> RemoveMemberAsync(ClaimsPrincipal? principal, int membershipId)
    {
        using var dbContext = Factory.CreateDbContext();
        var existing = await dbContext.GroupMembers.Include(gm => gm.Group).SingleOrDefaultAsync(gm => gm.Id == membershipId);
        if (existing is null) return existing.NotFound();
        // The group's member administrators, who see Remove (before, data administrators; Migration step 3.6).
        if (await IsGroupMemberAdministratorAsync(principal, existing.GroupId, existing.Group.CountryId))
        {
            dbContext.GroupMembers.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.NotAuthorized<GroupMember>();
    }
}
