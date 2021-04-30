using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Security;

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
