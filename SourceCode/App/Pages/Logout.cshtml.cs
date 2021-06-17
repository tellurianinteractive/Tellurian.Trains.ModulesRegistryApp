using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Security;
using System.Threading.Tasks;

namespace ModulesRegistry.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync() => await this.LogoutAsync();
    }
}
