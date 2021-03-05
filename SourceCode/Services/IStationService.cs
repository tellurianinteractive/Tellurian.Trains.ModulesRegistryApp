using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IStationService 
    {
        Task<Station?> FindByIdAsync(ClaimsPrincipal? principal, int id, int personalOwnerId);
        Task<IEnumerable<Station>> GetAllAsync(ClaimsPrincipal? principal);
        Task<(int Count, string Message, Station? Entity)> SaveAsync(ClaimsPrincipal? principal, Station entity, int owningPersonId, int moduleId);
    }
}
