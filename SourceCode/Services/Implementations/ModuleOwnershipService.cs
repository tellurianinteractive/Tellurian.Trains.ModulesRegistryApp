using ModulesRegistry.Resources;
using Rationals;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// Transfers module ownership and adds or removes assistants (rules decided 2026-11-21):
/// <list type="bullet">
/// <item>A module is owned either by persons, in shares, or by one group alone, 1/1; never by both.</item>
/// <item>A person owner transfers all or part of their share to another person.</item>
/// <item>A data administrator of a group transfers the group's module to one of its members, or a module owned only by members of
/// the group to the group.</item>
/// <item>A country administrator does all of that for owners and groups in their country.</item>
/// <item>Only a global administrator transfers to an owner in another country than the current owners.</item>
/// </list>
/// The stored ownerships decide, never values posted by the page.
/// </summary>
/// <remarks>Before release 1.15.29 any signed-in user could transfer any ownership and add or remove assistants.</remarks>
public class ModuleOwnershipService(IDbContextFactory<ModulesDbContext> factory)
{
    private const double SmallestShare = 0.01;
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    /// <param name="originId">The ownership to transfer from.</param>
    /// <param name="newOwnership">The new owner, a person or a group, and for a person the share of the whole module to transfer.
    /// A group always gets the whole module.</param>
    public async Task<(int Count, string Message, Module? Entity)> TransferOwnershipAsync(ClaimsPrincipal? principal, int originId, ModuleOwnership newOwnership)
    {
        if (!principal.IsAuthenticated() || principal.IsReadOnly()) return principal.SaveNotAuthorised<Module>();
        if (newOwnership.PersonId > 0 == newOwnership.GroupId > 0) return (0, Strings.OwnerMustBeEitherAPersonOrAGroup, null);
        await using var dbContext = await Factory.CreateDbContextAsync();
        var origin = await dbContext.ModuleOwnerships
            .Include(mo => mo.Module).ThenInclude(m => m.ModuleOwnerships)
            .SingleOrDefaultAsync(mo => mo.Id == originId);
        if (origin is null || origin.OwnedShare <= 0) return principal.NothingToUpdate<Module>();

        var refusal =
            newOwnership.GroupId > 0 ? await TransferToGroupAsync(dbContext, principal, origin, newOwnership.GroupId.Value) :
            origin.GroupId.HasValue ? await TransferFromGroupAsync(dbContext, principal, origin, newOwnership.PersonId!.Value) :
            await TransferBetweenPersonsAsync(dbContext, principal, origin, newOwnership.PersonId!.Value, newOwnership.OwnedShare);
        if (refusal is not null) return (0, refusal, null);

        var count = await dbContext.SaveChangesAsync();
        return count.SaveResult(await GetModule(origin.ModuleId));
    }

    public async Task<(int Count, string Message, Module? Entity)> AddAssistantAsync(ClaimsPrincipal? principal, ModuleOwnership ownership)
    {
        if (principal.IsAuthenticated() && ownership.IsAssistantOnly())
        {
            await using var dbContext = await Factory.CreateDbContextAsync();
            if (!await MayManageOwnershipsAsync(dbContext, principal, ownership.ModuleId)) return principal.SaveNotAuthorised<Module>();
            var existing = dbContext.ModuleOwnerships
                .AsNoTracking()
                .SingleOrDefault(mo => mo.ModuleId == ownership.ModuleId && (ownership.PersonId.HasValue && mo.PersonId == ownership.PersonId || ownership.GroupId.HasValue && mo.GroupId == ownership.GroupId));
            if (existing is not null) return principal.SaveNotAuthorised<Module>();

            var result = await SaveAssistant(dbContext, ownership);
            var module = await GetModule(ownership.ModuleId);
            return result.SaveResult(module);
        }
        return principal.SaveNotAuthorised<Module>();
    }

    public async Task<(int Count, string Message)> RemoveAssistantAsync(ClaimsPrincipal? principal, ModuleOwnership ownership)
    {
        if (principal.IsAuthenticated() && ownership.IsAssistantOnly())
        {
            await using var dbContext = await Factory.CreateDbContextAsync();
            var existing = await dbContext.ModuleOwnerships.SingleOrDefaultAsync(mo => mo.Id == ownership.Id);
            if (existing is null || !existing.IsAssistantOnly()) return principal.NotFound();
            if (!await MayManageOwnershipsAsync(dbContext, principal, existing.ModuleId)) return principal.NotAuthorized<ModuleOwnership>();
            dbContext.ModuleOwnerships.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.NotAuthorized<ModuleOwnership>();
    }

    /// <summary>A module owned only by members of the group becomes the group's, 1/1; assistants stay.</summary>
    private static async Task<string?> TransferToGroupAsync(ModulesDbContext dbContext, ClaimsPrincipal principal, ModuleOwnership origin, int groupId)
    {
        if (origin.GroupId.HasValue) return Strings.TransferBetweenGroupsIsNotSupported;
        var group = await dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId);
        if (group is null) return Strings.NotFound;
        var owners = Owners(origin.Module);
        if (owners.Any(o => o.GroupId.HasValue)) return Strings.GroupOwnsWholeModuleAlone;
        var ownerIds = owners.Select(o => o.PersonId!.Value).ToList();
        var memberCount = await dbContext.GroupMembers.CountAsync(gm => gm.GroupId == groupId && ownerIds.Contains(gm.PersonId));
        if (memberCount < ownerIds.Distinct().Count()) return Strings.AllOwnersMustBeGroupMembers;
        var ownerCountryIds = await PersonCountryIdsAsync(dbContext, ownerIds);

        var refusal = Refusal(principal,
            hasRole: await IsDataAdministratorAsync(dbContext, principal, groupId) || principal.IsCountryAdministratorInCountry(group.CountryId),
            isBetweenCountries: ownerCountryIds.Any(id => id != group.CountryId));
        if (refusal is not null) return refusal;

        dbContext.ModuleOwnerships.RemoveRange(owners);
        dbContext.ModuleOwnerships.Add(new ModuleOwnership { ModuleId = origin.ModuleId, GroupId = groupId, OwnedShare = 1 });
        return null;
    }

    /// <summary>The group's module, 1/1, becomes one of its members'.</summary>
    private static async Task<string?> TransferFromGroupAsync(ModulesDbContext dbContext, ClaimsPrincipal principal, ModuleOwnership origin, int personId)
    {
        var groupId = origin.GroupId!.Value;
        var groupCountryId = await dbContext.Groups.Where(g => g.Id == groupId).Select(g => g.CountryId).SingleAsync();
        if (!await dbContext.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == personId)) return Strings.NewOwnerMustBeGroupMember;
        var personCountryId = await dbContext.People.Where(p => p.Id == personId).Select(p => (int?)p.CountryId).SingleOrDefaultAsync();
        if (personCountryId is null) return Strings.NotFound;

        var refusal = Refusal(principal,
            hasRole: await IsDataAdministratorAsync(dbContext, principal, groupId) || principal.IsCountryAdministratorInCountry(groupCountryId),
            isBetweenCountries: personCountryId != groupCountryId);
        if (refusal is not null) return refusal;

        dbContext.ModuleOwnerships.Remove(origin);
        GiveShare(dbContext, origin.Module, personId, Rational.One);
        return null;
    }

    /// <summary>A person owner's share, or part of it, goes to another person.</summary>
    private static async Task<string?> TransferBetweenPersonsAsync(ModulesDbContext dbContext, ClaimsPrincipal principal, ModuleOwnership origin, int personId, double share)
    {
        if (personId == origin.PersonId) return Strings.NotFound;
        if (Owners(origin.Module).Any(o => o.GroupId.HasValue)) return Strings.GroupOwnsWholeModuleAlone;
        var transferred = share > 0 ? Rational.Approximate(share, 0.001) : Rational.Zero;
        var remaining = origin.SubtractShare(transferred);
        if (transferred <= Rational.Zero || !remaining.Ok) return Strings.UpdateFailed;
        var originCountryId = (await PersonCountryIdsAsync(dbContext, [origin.PersonId!.Value])).SingleOrDefault();
        var personCountryId = await dbContext.People.Where(p => p.Id == personId).Select(p => (int?)p.CountryId).SingleOrDefaultAsync();
        if (personCountryId is null) return Strings.NotFound;

        var refusal = Refusal(principal,
            hasRole: origin.PersonId == principal.PersonId() || principal.IsCountryAdministratorInCountry(originCountryId),
            isBetweenCountries: personCountryId != originCountryId);
        if (refusal is not null) return refusal;

        if (remaining.Percentage < SmallestShare) dbContext.ModuleOwnerships.Remove(origin);
        else origin.OwnedShare = remaining.Percentage;
        GiveShare(dbContext, origin.Module, personId, transferred);
        return null;
    }

    /// <summary>A global administrator may do any transfer; others need a role, and the new owner in the same country.</summary>
    private static string? Refusal(ClaimsPrincipal principal, bool hasRole, bool isBetweenCountries) =>
        principal.IsGlobalAdministrator() ? null :
        !hasRole ? Strings.NotAuthorized :
        isBetweenCountries ? Strings.OnlyGlobalAdministratorTransfersBetweenCountries :
        null;

    /// <summary>
    /// Who adds and removes assistants: a global administrator, an owner, a data administrator of the owning group, or a country
    /// administrator in an owner's country.
    /// </summary>
    private static async Task<bool> MayManageOwnershipsAsync(ModulesDbContext dbContext, ClaimsPrincipal principal, int moduleId)
    {
        if (principal.IsReadOnly()) return false;
        if (principal.IsGlobalAdministrator()) return true;
        var owners = await dbContext.ModuleOwnerships.AsNoTracking().Where(mo => mo.ModuleId == moduleId && mo.OwnedShare > 0).ToListAsync();
        if (owners.Any(o => o.PersonId == principal.PersonId())) return true;
        var groupId = owners.FirstOrDefault(o => o.GroupId.HasValue)?.GroupId;
        if (groupId.HasValue && await IsDataAdministratorAsync(dbContext, principal, groupId.Value)) return true;
        if (!principal.IsCountryAdministrator()) return false;
        var countryIds = await PersonCountryIdsAsync(dbContext, [.. owners.Where(o => o.PersonId.HasValue).Select(o => o.PersonId!.Value)]);
        if (groupId.HasValue) countryIds.Add(await dbContext.Groups.Where(g => g.Id == groupId).Select(g => g.CountryId).SingleAsync());
        return countryIds.Contains(principal.CountryId());
    }

    private static Task<bool> IsDataAdministratorAsync(ModulesDbContext dbContext, ClaimsPrincipal principal, int groupId) =>
        dbContext.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == principal.PersonId() && gm.IsDataAdministrator);

    private static async Task<List<int>> PersonCountryIdsAsync(ModulesDbContext dbContext, IEnumerable<int> personIds)
    {
        var ids = personIds.ToList();
        return await dbContext.People.Where(p => ids.Contains(p.Id)).Select(p => p.CountryId).ToListAsync();
    }

    private static List<ModuleOwnership> Owners(Module module) => [.. module.ModuleOwnerships.Where(mo => mo.OwnedShare > 0)];

    /// <summary>Adds the share to the person's ownership; an assistant becomes an owner.</summary>
    private static void GiveShare(ModulesDbContext dbContext, Module module, int personId, Rational share)
    {
        var existing = module.ModuleOwnerships.FirstOrDefault(mo => mo.PersonId == personId && dbContext.Entry(mo).State != EntityState.Deleted);
        if (existing is null)
        {
            dbContext.ModuleOwnerships.Add(new ModuleOwnership { ModuleId = module.Id, PersonId = personId, OwnedShare = (double)share });
        }
        else
        {
            existing.OwnedShare = Math.Min(1, existing.AddShare(share).Percentage);
        }
    }

    private async Task<Module?> GetModule(int moduleId)
    {
        await using var dbContext = await Factory.CreateDbContextAsync();
        return await dbContext.Modules
           .AsNoTracking()
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
           .SingleOrDefaultAsync(m => m.Id == moduleId);
    }

    /// <summary>
    /// A new entity with only the ids, so that navigations on <paramref name="ownership"/> aren't added too.
    /// (Was raw SQL, from before <c>BlankTriggerAddingConvention</c> made inserts into tables with triggers work.)
    /// </summary>
    private static async Task<int> SaveAssistant(ModulesDbContext dbContext, ModuleOwnership ownership)
    {
        dbContext.ModuleOwnerships.Add(new ModuleOwnership
        {
            PersonId = ownership.PersonId,
            GroupId = ownership.GroupId,
            ModuleId = ownership.ModuleId,
            OwnedShare = 0,
        });
        return await dbContext.SaveChangesAsync();
    }
}