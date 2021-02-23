using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IScaleService
    {
        Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal);
    }
}
