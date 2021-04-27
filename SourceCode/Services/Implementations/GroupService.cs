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
    public sealed class GroupService
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;
        public GroupService(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int? countryId)
        {
            using var dbContext = Factory.CreateDbContext();
            var id = countryId ?? principal.CountryId();
            return await dbContext.Groups.AsNoTracking().Where(g => principal.IsGlobalAdministrator() || g.CountryId == id).Select(g => new ListboxItem(g.Id, g.FullName)).ToListAsync();
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
            if (principal.IsCountryAdministratorInCountry(countryId))
            {
                using var dbContext = Factory.CreateDbContext();
                var items = await dbContext.Groups.AsNoTracking()
                    .Include(g => g.GroupDomain)
                    .Where(g =>  (g.CountryId == countryId) || (g.GroupDomainId > 0 && principal.GroupDomainIds().Contains(g.GroupDomainId.Value)))
                    .OrderBy(g => g.FullName)
                    .ToListAsync();
                return items.Select(i => (i, true));
            }
            else
            {
                using var dbContext = Factory.CreateDbContext();
                var items = await dbContext.Groups.AsNoTracking()
                    .Include(g => g.GroupDomain)
                    .Where(g => (g.GroupDomainId > 0 && principal.GroupDomainIds().Contains(g.GroupDomainId.Value)) || g.GroupMembers.Any(gm => gm.PersonId == principal.PersonId()))
                    .OrderBy(g => g.FullName)
                    .ToListAsync();
                return items.Select(i => (i, i.GroupMembers.Any(gm => (gm.IsDataAdministrator || gm.IsGroupAdministrator) && gm.PersonId == principal.PersonId())));
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
            if (await IsGroupDataAdministratorAsync(principal, groupMember.GroupId, groupMember.Group.CountryId)) return groupMember;
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
        public async Task<bool> IsGroupMemberAdministratorAsync(ClaimsPrincipal? pricipal, int groupId, int? countryId = null)
        {
            if (pricipal.IsCountryAdministratorInCountry(countryId)) return true;
            else
            {
                using var dbContext = Factory.CreateDbContext();
                return await dbContext.GroupMembers.AsNoTracking()
                    .AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == pricipal.PersonId() && gm.IsGroupAdministrator);
            }
        }

        public async Task<bool> IsDataAdministratorInSameGroupAsMember(ClaimsPrincipal? pricipal, int memberPersonId)
        {
            using var dbContext = Factory.CreateDbContext();
            var adminGroups = await dbContext.Groups.AsNoTracking()
                .Include(g => g.GroupMembers)
                .Where(g => g.GroupMembers
                .Any(gm => gm.IsDataAdministrator && gm.PersonId == pricipal.PersonId()))
                .ToListAsync();
            if (adminGroups is null || adminGroups.Count == 0) return false;
            return adminGroups.Any(ag => ag.GroupMembers.Any(gm => gm.PersonId == memberPersonId));
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
                    .AsEnumerable()
                    .GroupBy(gm => gm.Group.GroupDomainId)
                    .ToList();
                return memberships.Any(m => m.Count(gm => gm.PersonId == ownershipRef.PersonId || gm.PersonId == principal.PersonId()) > 1);
            }
            return false;
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

        public async Task<(int Count, string Message, GroupMember? Entity)> SaveMemberAsync(ClaimsPrincipal? principal, GroupMember entity)
        {
            using var dbContext = Factory.CreateDbContext();
            var countryId = (await dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == entity.GroupId))?.CountryId;
            if (await IsGroupDataAdministratorAsync(principal, entity.GroupId, countryId))
            {
                dbContext.GroupMembers.Attach(entity);
                dbContext.Entry(entity).State = entity.Id.GetState();
                var count = await dbContext.SaveChangesAsync();
                return count.SaveResult(entity);
            }
            return principal.SaveNotAuthorised<GroupMember>();
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

        public async Task<(int Count, string Message)> RemoveMemberAsync(ClaimsPrincipal? principal, int membershipId)
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.GroupMembers.Include(gm => gm.Group).SingleOrDefaultAsync(gm => gm.Id == membershipId);
            if (existing is null) return existing.NotFound();
            if (await IsGroupDataAdministratorAsync(principal, existing.GroupId, existing.Group.CountryId))
            {
                dbContext.GroupMembers.Remove(existing);
                var result = await dbContext.SaveChangesAsync();
                return result.DeleteResult();
            }
            return principal.DeleteNotAuthorized<GroupMember>();
        }
    }
}
