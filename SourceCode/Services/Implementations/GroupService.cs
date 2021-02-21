using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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

        public async Task<IEnumerable<Group>> GetAllInCountryAsync(ClaimsPrincipal? principal, int countryId)
        {
            if (principal.IsAuthorisedInCountry(countryId))
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Groups
                    .Where(g => g.CountryId == countryId)
                    .OrderBy(g => g.FullName)
                    .ToListAsync();
            }
            else
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.Groups
                    .Where(g => g.GroupMembers
                    .Any(gm => (gm.IsGroupAdministrator || gm.IsDataAdministrator) && gm.PersonId == principal.PersonId()))
                    .OrderBy(g => g.FullName)
                    .ToListAsync();
            }
        }

        public async Task<Group?> FindByIdAsync(ClaimsPrincipal? principal, int id)
        {
            using var dbContext = Factory.CreateDbContext();
            var group = await dbContext.Groups
                .Include(g => g.GroupMembers)
                .ThenInclude(gm => gm.Person)
                .SingleOrDefaultAsync(g => g.Id == id);
            if (principal.IsAuthorisedInCountry(group.CountryId)) return group;
            return null;
        }

        public async Task<(int Count, string Message, Group? Entity)> SaveAsync(ClaimsPrincipal? principal, Group group)
        {
            if (principal.MaySave())
            {
                using var dbContext = Factory.CreateDbContext();
                dbContext.Groups.Attach(group);
                dbContext.Entry(group).State = group.Id.GetState();
                var count = await dbContext.SaveChangesAsync();
                return count.SaveResult(group);
            }
            return principal.SaveNotAuthorised<Group>();
        }

        public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
        {
            if (principal.MayDelete())
            {
                using var dbContext = Factory.CreateDbContext();
                var group = await FindByIdAsync(principal, id);
                if (group is null) return (0, Strings.NothingToDelete);
                if (group.GroupMembers.Any() || group.ModuleOwnerships.Any()) return (0, Strings.MayNotBeDeleted);
                dbContext.Remove(group);
                var count = await dbContext.SaveChangesAsync();
                return count.DeleteResult();
            }
            return principal.DeleteNotAuthorized<Group>();
       }

        public async Task<(int Count, string Message, GroupMember? Member)> AddMemberAsync(ClaimsPrincipal? principal, GroupMember groupMember)
        {
            using var dbContext = Factory.CreateDbContext();
            if (principal.IsGroupMemberAdministrator(dbContext.GroupMembers.Where(gm => gm.GroupId == groupMember.GroupId)))
            {
                var existing = dbContext.GroupMembers.SingleOrDefault(gm => gm.PersonId == groupMember.PersonId && gm.GroupId == groupMember.GroupId);
                if (existing is not null) return existing.AlreadyExists();
                dbContext.GroupMembers.Add(groupMember);
                var result = await dbContext.SaveChangesAsync();
                return result.SaveResult(groupMember);
            }
            return principal.SaveNotAuthorised<GroupMember>();
        }

        public async Task<(int Count, string Message)> RemoveMemberAsync(ClaimsPrincipal? principal, int groupId, int personId)
        {
            using var dbContext = Factory.CreateDbContext();
            if (principal.IsGroupMemberAdministrator(dbContext.GroupMembers.Where(gm => gm.GroupId == groupId)))
            {
                var existing = dbContext.GroupMembers.SingleOrDefault(gm => gm.PersonId == personId && gm.GroupId == groupId);
                if (existing is null) return existing.NotFound();
                dbContext.GroupMembers.Remove(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.DeleteResult();
            }
            return principal.DeleteNotAuthorized<GroupMember>();
        }
    }
}
