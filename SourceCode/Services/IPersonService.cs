using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IPersonService
    {
        Task<IEnumerable<Person>> GetAllInCountryAsync(int inCountryId);
        Task<Person?> FindAsync(int id);
        Task<Person?> GetByAsync(int userId);
        Task<(int Count, string Message, Person? Person)> SaveAsync(Person person);
        Task<(int Count, string Message)> DeleteAsync(int id);
        Task<IEnumerable<ListboxItem>> ListboxItemsAsync(int countryId);
    }
}
