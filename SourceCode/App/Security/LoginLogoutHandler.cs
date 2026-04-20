using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;
using System.Security.Claims;

namespace ModulesRegistry.Security;

internal static class LoginLogoutHandler
{
    public static async Task<IActionResult> LoginAsync(this PageModel model, UserService userService, IMailSender mailSender, ContentService contentService, string? username, string? password, string? returnUrl = null)
    {
        string rootUrl = model.Url.Content("~/");
        if (returnUrl is null) returnUrl = rootUrl;
        else if (!returnUrl.StartsWith(rootUrl)) { returnUrl = rootUrl + returnUrl; }
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return model.LocalRedirect(rootUrl);

        await SignOut();

        var user = await userService.FindByEmailAsync(username);
        if (user is not null && !user.IsLockedOut())
        {
            var verification = user.Person is not null
                ? password.VerifyPassword(user.HashedPassword)
                : default;

            if (verification.Matched)
            {
                if (verification.NeedsRehash)
                {
                    try { _ = await userService.UpdateHashedPasswordAsync(user.Id, password.AsHashedPassword()); }
                    catch { /* rehash is best-effort; user stays on legacy hash until next login */ }
                }
                var claims = new List<Claim>
                {
                    new(AppClaimTypes.ObjectId, user.ObjectId.ToString()),
                    new(ClaimTypes.Email, user.EmailAddress),
                };
                await TrySignIn(user, claims);
            }
            else
            {
                var updated = await userService.UpdateFailedLoginAttempts(user.Id);
                if (updated is not null && updated.IsLockedOut())
                {
                    await SendSecurityNotification(updated, "AccountLockedMail", "AccountLockedMailSubject", mailSender, contentService);
                }
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

    internal static async Task SendSecurityNotification(User recipient, string contentName, string subjectKey, IMailSender mailSender, ContentService contentService)
    {
        try
        {
            var language = recipient.PreferredLanguage();
            var markdown = await contentService.GetTextContent(contentName, language);
            var subject = LanguageExtensions.GetLocalizedString(subjectKey, language);
            var notification = new SecurityNotification(recipient, subject, markdown);
            var message = notification.MailMessage;
            if (message is not null) _ = await mailSender.SendMailMessageAsync(message);
        }
        catch { /* security notifications are best-effort; never block the primary flow */ }
    }

    public static async Task<IActionResult> LogoutAsync(this PageModel model)
    {
        await model.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); // Clears the authenication cookie.
        return model.LocalRedirect(model.Url.Content("~/"));
    }
}
