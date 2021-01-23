using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IModuleService
    {
        Task<Module> GetModuleAsync(int id);
        IAsyncEnumerable<Module> GetModulesAsync();
    }
}
