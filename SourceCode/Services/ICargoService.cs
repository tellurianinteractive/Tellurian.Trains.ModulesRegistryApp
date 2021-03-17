using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface ICargoService : IDataService<Cargo>
    {
        Task<IEnumerable<Cargo>> GetAll(ClaimsPrincipal? principal);
        Task<IEnumerable<ListboxItem>> GetNhmItems(ClaimsPrincipal? principal, int? subItemsToId = null);
    }
}
