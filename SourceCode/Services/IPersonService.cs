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
        Task<IEnumerable<Person>> GetAllAsync(int inCountryId);
        Task<Person?> FindAsync(int id);
        Task<Person?> GetByAsync(int userId);
    }
}
