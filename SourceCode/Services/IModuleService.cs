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
        Task<IEnumerable<Module>> GetAllAsync(ClaimsPrincipal? principal);
    }
}
