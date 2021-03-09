using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IModuleService : IDataService<Module>
    {
        Task<(int Count, string Message)> CloneAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef);
        Task<Module?> FindByIdAsync(ClaimsPrincipal? principal, int id, ModuleOwnershipRef ownerRef);
        Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal);
        Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef);
        Task<IEnumerable<ListboxItem>> ModuleItems(ClaimsPrincipal? principal);
        Task<(int Count, string Message, Module? Entity)> SaveAsync(ClaimsPrincipal? principal, Module entity, ModuleOwnershipRef ownerRef);
    }
}
