using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Security;
using ModulesRegistry.Services.Implementations;
using System.Threading.Tasks;

namespace ModulesRegistry.Pages
{
    public class LoginModel : PageModel
    {
        public LoginModel(UserService userService) { UserService = userService; }
        private readonly UserService UserService;
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string? username, string? password) =>
            await this.LoginAsync(UserService, username, password);
    }
}
