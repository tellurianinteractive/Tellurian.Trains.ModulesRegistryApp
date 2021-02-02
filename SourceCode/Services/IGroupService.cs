using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IGroupService
    {
        Task<IEnumerable<Group>> GetAllInCountryAsync(int countryId);
        Task<Group?> FindAsync(int id);
        Task<(int Count, Group? Group, string Message)> SaveAsync(Group group);
        Task<(int Count, string Message)> DeleteAsync(int id);
        Task<(int Count, GroupMember? GroupMember, string Message)> AddMemberAsync(GroupMember groupMember);
        Task<(int Count, GroupMember? GroupMember, string Message)> RemoveMemberAsync(int groupId, int personId);
    }
}
