using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IPersonService
    {
        Task<IEnumerable<Person>> GetAllInCountryAsync(ClaimsPrincipal? principal, int countryId);
        Task<Person?> FindByIdAsync(ClaimsPrincipal? principal, int id);
        Task<Person?> FindByUserIdAsync(ClaimsPrincipal? principal, int userId);
        Task<(int Count, string Message, Person? Person)> SaveAsync(ClaimsPrincipal? principal, Person person);
        Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id);
        Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal, int countryId);
    }
}
