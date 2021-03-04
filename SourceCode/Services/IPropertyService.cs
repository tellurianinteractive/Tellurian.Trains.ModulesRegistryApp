using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IPropertyService
    {
        Task<IEnumerable<ListboxItem>> GetGableTypeListboxItemsAsync();
    }
}
