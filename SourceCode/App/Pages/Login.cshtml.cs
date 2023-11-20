using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Security;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Pages;

public class LoginModel(UserService userService) : PageModel
{
    private readonly UserService UserService = userService;
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string? username, string? password, string? returnUrl) =>
        await this.LoginAsync(UserService, username, password, returnUrl);
}
