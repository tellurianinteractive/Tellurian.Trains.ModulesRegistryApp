using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Data;

namespace ModulesRegistry.Pages
{
    public class LoginModel : PageModel
    {
        public LoginModel(IUserService userService) => UserService = userService;
        private readonly IUserService UserService;
        public string? ReturnUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string? username, string? password)
        {
            string? returnUrl = Url.Content("~/");
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return LocalRedirect(returnUrl);

            await SignOut();

            var user = await UserService.FindByEmailAsync(username);
            if (user is not null && user.Person is not null && password.IsSamePasswordAs(user.HashedPassword))
            {
                var claims = new List<Claim>
                {
                    new Claim(AppClaimTypes.ObjectId, user.ObjectId.ToString()),
                    new Claim(ClaimTypes.Email, user.EmailAddress),
                };
                await TrySignIn(user, claims);
            }

            return LocalRedirect(returnUrl);

            async Task TrySignIn(User user, List<Claim> claims)
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = Request.Host.Value
                };
                if (claims.Any())
                {
                    try
                    {
                        await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    }
                    catch (Exception ex)
                    {
                        string error = ex.Message;
                    }
                    _ = await UserService.UpdateLastSignInTime(user.Id, DateTimeOffset.Now);
                }
            }

            async Task SignOut()
            {
                try
                {
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
                catch { }
            }
        }
    }
}
