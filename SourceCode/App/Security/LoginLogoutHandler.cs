using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Security;

internal static class LoginLogoutHandler
{
    public static async Task<IActionResult> LoginAsync(this PageModel model, UserService userService, string? username, string? password, string? returnUrl = null)
    {
        string rootUrl = model.Url.Content("~/");
        if (returnUrl is null) returnUrl = rootUrl;
        else if (!returnUrl.StartsWith(rootUrl)) { returnUrl = rootUrl + returnUrl; }
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return model.LocalRedirect(rootUrl);

        await SignOut();

        var user = await userService.FindByEmailAsync(username);
        if (user is not null)
        {

            if (user.Person is not null && password.IsSamePasswordAs(user.HashedPassword))
            {
                var claims = new List<Claim>
                {
                    new(AppClaimTypes.ObjectId, user.ObjectId.ToString()),
                    new(ClaimTypes.Email, user.EmailAddress),
                };
                await TrySignIn(user, claims);
            }
            else
            {
                _ = await userService.UpdateFailedLoginAttempts(user.Id);
            }
        }
        return model.LocalRedirect(returnUrl);


        async Task TrySignIn(User user, List<Claim> claims)
        {
            if (claims.Any())
            {
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    RedirectUri = model.Request.Path
                };

                try
                {
                    await model.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    _ = await userService.UpdateLastSignIn(user.Id, DateTimeOffset.Now);
                }
                catch
                {

                }
            }
            else
            {
                _ = await userService.UpdateFailedLoginAttempts(user.Id);
            }
        }

        async Task SignOut()
        {
            try
            {
                await LogoutAsync(model);
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
