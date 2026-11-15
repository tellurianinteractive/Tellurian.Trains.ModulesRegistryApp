using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Security;

/// <summary>
/// Checks the credentials and signs the user in with the authentication cookie.
/// Separated from <see cref="LoginEndpoint"/> so the endpoint can be tested without a database.
/// </summary>
public interface ILoginHandler
{
    Task SignInAsync(HttpContext context, string username, string password);
}

internal sealed class LoginHandler(UserService userService, IMailSender mailSender, ContentService contentService, ILogger<LoginHandler> logger) : ILoginHandler
{
    public async Task SignInAsync(HttpContext context, string username, string password)
    {
        await SignOutAsync(context);

        var user = await userService.FindByEmailAsync(username);
        if (user is null || user.IsLockedOut()) return;

        var verification = user.Person is not null
            ? password.VerifyPassword(user.HashedPassword)
            : default;

        if (verification.Matched)
        {
            if (verification.NeedsRehash)
            {
                try { _ = await userService.UpdateHashedPasswordAsync(user.Id, password.AsHashedPassword()); }
                catch (Exception ex)
                {
                    // Best-effort: the user stays on the legacy hash until the next sign-in.
                    logger.LogError(ex, "Rehashing the password of user {UserId} failed", user.Id);
                }
            }
            var claims = new List<Claim>
            {
                new(AppClaimTypes.ObjectId, user.ObjectId.ToString()),
                new(ClaimTypes.Email, user.EmailAddress),
            };
            await TrySignInAsync(context, user, claims);
        }
        else
        {
            var updated = await userService.UpdateFailedLoginAttempts(user.Id);
            if (updated is not null && updated.IsLockedOut())
            {
                await LoginLogoutHandler.SendSecurityNotification(updated, "AccountLockedMail", "AccountLockedMailSubject", mailSender, contentService, logger);
            }
        }
    }

    private async Task TrySignInAsync(HttpContext context, User user, List<Claim> claims)
    {
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            RedirectUri = context.Request.Path
        };
        try
        {
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
            _ = await userService.UpdateLastSignIn(user.Id, DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Signing in user {UserId} failed", user.Id);
        }
    }

    private async Task SignOutAsync(HttpContext context)
    {
        try
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Signing out before signing in failed");
        }
    }
}