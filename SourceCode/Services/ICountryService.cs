using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface ICountryService
    {
        Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal);
    }
}
