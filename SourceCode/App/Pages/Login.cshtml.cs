using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Pages;

public class LoginModel(UserService userService, IMailSender mailSender, ContentService contentService) : PageModel
{
    private readonly UserService UserService = userService;
    private readonly IMailSender MailSender = mailSender;
    private readonly ContentService ContentService = contentService;
    public string? ReturnUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string? username, string? password, string? returnUrl) =>
        await this.LoginAsync(UserService, MailSender, ContentService, username, password, returnUrl);
}
