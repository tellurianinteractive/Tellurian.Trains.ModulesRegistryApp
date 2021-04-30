using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Security;

namespace ModulesRegistry.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync() => await this.LogoutAsync();
    }
}
