using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Security;

internal static class LoginLogoutHandler
{
    public static async Task<IActionResult> LoginAsync(this PageModel model, UserService userService, string? username, string? password)
    {
        string? returnUrl = model.Url.Content("~/");
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return model.LocalRedirect(returnUrl);

        await SignOut();

        var user = await userService.FindByEmailAsync(username);
        if (user is not null && user.Person is not null && password.IsSamePasswordAs(user.HashedPassword))
        {
            var claims = new List<Claim>
                {
                    new Claim(AppClaimTypes.ObjectId, user.ObjectId.ToString()),
                    new Claim(ClaimTypes.Email, user.EmailAddress),
                };
            await TrySignIn(user, claims);
        }

        return model.LocalRedirect(returnUrl);

        async Task TrySignIn(User user, List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                RedirectUri = model.Request.Host.Value
            };
            if (claims.Any())
            {
                try
                {
                    await model.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    _ = await userService.UpdateLastSignInTime(user.Id, DateTimeOffset.Now);
                }
                catch
                {

                }
            }
        }

        async Task SignOut()
        {
            try
            {
                await model.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            catch { }
        }
    }

    public static async Task<IActionResult> LogoutAsync(this PageModel model)
    {
        await model.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Clears the authenication cookie.
        return model.LocalRedirect(model.Url.Content("~/"));
    }
}
