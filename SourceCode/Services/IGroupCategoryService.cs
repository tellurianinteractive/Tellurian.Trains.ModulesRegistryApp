using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IGroupCategoryService
    {
        Task<IEnumerable<(string Value, string Description)>> AllAsync();
    }
}
