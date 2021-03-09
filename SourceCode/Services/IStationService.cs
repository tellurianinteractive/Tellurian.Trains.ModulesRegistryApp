using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IStationService 
    {
        Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef);
        Task<IEnumerable<Station>> GetAllAsync(ClaimsPrincipal? principal);
        Task<IEnumerable<Station>> GetAllAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef);
        Task<(int Count, string Message, Station? Entity)> SaveAsync(ClaimsPrincipal? principal, Station entity, ModuleOwnershipRef ownerRef, int moduleId);
    }
}
