using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IGroupService : IDataService<Group>
    {
        Task<IEnumerable<Group>> GetAllInCountryAsync(ClaimsPrincipal? principal, int countryId);
        Task<(int Count, string Message, GroupMember? Member)> AddMemberAsync(ClaimsPrincipal? principal, GroupMember groupMember);
        Task<(int Count, string Message)> RemoveMemberAsync(ClaimsPrincipal? principal, int groupId, int personId);
    }
}
