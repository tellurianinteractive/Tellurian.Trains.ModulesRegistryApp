using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface ICountryService
    {
        Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal);
        Task<Country?> FindById(int id);
    }
}
