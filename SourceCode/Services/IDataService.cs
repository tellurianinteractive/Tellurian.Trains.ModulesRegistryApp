using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IDataService<T> where T : class
    {
        Task<T?> FindByIdAsync(ClaimsPrincipal? principal, int id);
        Task<(int Count, string Message, T? Entity)> SaveAsync(ClaimsPrincipal? principal, T entity);
        Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id);
    }
}
