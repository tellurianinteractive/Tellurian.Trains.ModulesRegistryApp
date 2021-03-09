using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IGroupService : IDataService<Group>
    {
        Task<IEnumerable<Group>> GetAllAsync(ClaimsPrincipal? principal, int countryId);
        Task<(int Count, string Message, GroupMember? Member)> AddMemberAsync(ClaimsPrincipal? principal, GroupMember groupMember);
        Task<(int Count, string Message)> RemoveMemberAsync(ClaimsPrincipal? principal, int groupId, int personId);
        Task<GroupMember?> FindMemberByIdAsync(ClaimsPrincipal? principal, int memberId);
        Task<(int Count, string Message, GroupMember? Entity)> SaveMemberAsync(ClaimsPrincipal? principal, GroupMember entity);
        Task<bool> IsGroupDataAdministratorAsync(ClaimsPrincipal? pricipal, int groupId, int? countryId = null);
        Task<bool> IsDataAdministratorInSameGroupAsMember(ClaimsPrincipal? pricipal, int memberPersonId);
    }
}
