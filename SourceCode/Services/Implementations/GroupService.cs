using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class GroupService : IGroupService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public GroupService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public async Task<IEnumerable<Group>> GetAllInCountryAsync(int countryId)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Groups
                .Where(g => g.CountryId == countryId)
                .OrderBy( g => g.FullName)
                .ToListAsync();
        }

        public async Task<Group?> FindAsync(int id)
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.Groups
                .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.Person)
                .SingleOrDefaultAsync(g => g.Id == id);
        }

        public async Task<(int, Group?, string)> SaveAsync(Group group)
        {
            using var dbContext = Factory.CreateDbContext();
            dbContext.Groups.Attach(group);
            dbContext.Entry(group).State = group.Id.GetState();
            var count = await dbContext.SaveChangesAsync();
            return count > 0 ? (count, group, "Saved") : (count, null, "SaveFailed");
        }

        public async Task<(int, string)> DeleteAsync(int id)
        {
            using var dbContext = Factory.CreateDbContext();
            var group = await FindAsync(id);
            if (group is null) return (0, Strings.NothingToDelete);
            if (group.GroupMembers.Any() || group.ModuleOwnerships.Any()) return (0, Strings.MayNotBeDeleted);
            dbContext.Remove(group);
            var count = await dbContext.SaveChangesAsync();
            return (count, count > 0 ? Strings.DeletedSuccessfully : Strings.DeleteFailed);
        }

        public async Task<(int, GroupMember?, string)> AddMemberAsync(GroupMember groupMember)
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = dbContext.GroupMembers.SingleOrDefault(gm => gm.PersonId == groupMember.PersonId && gm.GroupId == groupMember.GroupId);
            if (existing is not null) return (0, existing, "AlreadyExists");
            dbContext.GroupMembers.Add(groupMember);
            var result = await dbContext.SaveChangesAsync();
            return result > 0 ? (result, groupMember, "Saved") : (result, null, "SaveFailed");
        }

        public async Task<(int, GroupMember?, string)> RemoveMemberAsync(int groupId, int personId)
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = dbContext.GroupMembers.SingleOrDefault(gm => gm.PersonId == personId && gm.GroupId == groupId);
            if (existing is null) return (0, null, "NotMember");
            dbContext.GroupMembers.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result > 0 ? (result, existing, "Removed") : (result, null, "RemoveFailed");
        }

    }
}
