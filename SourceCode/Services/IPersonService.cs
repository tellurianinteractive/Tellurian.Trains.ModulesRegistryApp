using ModulesRegistry.Data;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IPersonService : IDataService<Person>
    {
        Task<IEnumerable<Person>> GetAllInCountryAsync(ClaimsPrincipal? principal, int countryId);
        Task<Person?> FindByUserIdAsync(ClaimsPrincipal? principal, int userId);

        Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int countryId);
    }
}
