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

    public async Task<int> AddModule(ClaimsPrincipal? principal, int groupId, int moduleId, int? groupMemberId = null)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.GroupLayoutModules.SingleOrDefaultAsync(x => x.GroupId == groupId && x.ModuleId == moduleId);
            if (existing is null)
            {
                var entity = new GroupLayoutModule { GroupId = groupId, GroupMemberId = groupMemberId, ModuleId = moduleId };
                dbContext.GroupLayoutModules.Add(entity);
                return await dbContext.SaveChangesAsync();
            }
        }
        return 0;
    }
    public async Task<int> RemoveModule(ClaimsPrincipal? principal, int groupId, int moduleId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.GroupLayoutModules.SingleOrDefaultAsync(x => x.GroupId == groupId && x.ModuleId == moduleId);
            if (existing is not null)
            {
                dbContext.GroupLayoutModules.Remove(existing);
                return await dbContext.SaveChangesAsync();
            }
        }
        return 0;
    }

}
