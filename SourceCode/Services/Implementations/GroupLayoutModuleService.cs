namespace ModulesRegistry.Services.Implementations;

public class GroupLayoutModuleService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<GroupLayoutModule>> GetGroupMembersLayoutModulesAsync(ClaimsPrincipal? principal, int groupMemberId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.GroupLayoutModules
                .Include(glm => glm.Module)
                .Where(glm => glm.GroupMemberId == groupMemberId)
                .ToReadOnlyListAsync();
        }
        return [];
    }


    public async Task<IEnumerable<Module>> GetGroupMemberModulesAsync(ClaimsPrincipal? principal, int groupMemberId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var person = await dbContext.GroupMembers.Where(gm => gm.Id == groupMemberId).Select(gm => gm.Person).SingleOrDefaultAsync();
            if (person is not null)
            {
                return await dbContext.Modules
                    .Where(m => m.ModuleOwnerships.Any(mo => mo.PersonId == person.Id) && !m.GroupLayoutModules.Any(glm => glm.GroupMemberId == groupMemberId))
                    .ToReadOnlyListAsync();

            }
        }
        return [];
    }

    /// <summary>
    /// Adds one of a member's own modules to the group's layout modules, if the user administers the group's members
    /// (Migration step 3.6; before, any signed-in user could). The member must belong to the group and own the module.
    /// </summary>
    public async Task<int> AddModule(ClaimsPrincipal? principal, int groupId, int moduleId, int? groupMemberId = null)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayAdministerGroupMembersAsync(principal, groupId)) return 0;
        if (groupMemberId.HasValue)
        {
            var personId = await dbContext.GroupMembers.Where(gm => gm.Id == groupMemberId && gm.GroupId == groupId).Select(gm => (int?)gm.PersonId).SingleOrDefaultAsync();
            if (personId is null || !await dbContext.ModuleOwnerships.AnyAsync(mo => mo.ModuleId == moduleId && mo.PersonId == personId)) return 0;
        }
        if (await dbContext.GroupLayoutModules.AnyAsync(x => x.GroupId == groupId && x.ModuleId == moduleId)) return 0;
        dbContext.GroupLayoutModules.Add(new GroupLayoutModule { GroupId = groupId, GroupMemberId = groupMemberId, ModuleId = moduleId });
        return await dbContext.SaveChangesAsync();
    }

    /// <summary>Removes a module from the group's layout modules, if the user administers the group's members.</summary>
    public async Task<int> RemoveModule(ClaimsPrincipal? principal, int groupId, int moduleId)
    {
        using var dbContext = Factory.CreateDbContext();
        if (!await dbContext.MayAdministerGroupMembersAsync(principal, groupId)) return 0;
        var existing = await dbContext.GroupLayoutModules.SingleOrDefaultAsync(x => x.GroupId == groupId && x.ModuleId == moduleId);
        if (existing is null) return 0;
        dbContext.GroupLayoutModules.Remove(existing);
        return await dbContext.SaveChangesAsync();
    }
}
