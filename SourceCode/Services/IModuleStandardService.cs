using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IModuleStandardService : IDataService<ModuleStandard>
    {
        Task<IEnumerable<ModuleStandard>> All(ClaimsPrincipal? principal);
        Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal);
    }
}
