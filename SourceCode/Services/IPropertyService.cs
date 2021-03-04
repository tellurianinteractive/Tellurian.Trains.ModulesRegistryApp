using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IPropertyService
    {
        Task<IEnumerable<ListboxItem>> AddProperty(string name, string value);
        Task<IEnumerable<ListboxItem>> GetGableTypeListboxItemsAsync();
        Task<IEnumerable<ListboxItem>> GetListboxItemsAsync(string name);
    }
}
