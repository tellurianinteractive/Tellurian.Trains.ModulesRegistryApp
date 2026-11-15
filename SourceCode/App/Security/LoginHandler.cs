using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Diagnostics;
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
        if (user is null)
        {
            logger.SignInFailedUnknownUser();
            return;
        }
        if (user.IsLockedOut())
        {
            logger.SignInFailed(user.Id, SignInFailure.AlreadyLockedOut);
            return;
        }

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
            if (await TrySignInAsync(context, user, claims)) logger.SignedIn(user.Id);
        }
        else
        {
            logger.SignInFailed(user.Id, user.Person is null ? SignInFailure.NoPerson : SignInFailure.WrongPassword);
            var updated = await userService.UpdateFailedLoginAttempts(user.Id);
            if (updated is not null && updated.IsLockedOut())
            {
                logger.LockedOut(updated.Id, updated.FailedLoginAttempts);
                await LoginLogoutHandler.SendSecurityNotification(updated, "AccountLockedMail", "AccountLockedMailSubject", mailSender, contentService, logger);
            }
        }
    }

    /// <returns>True if the user was signed in.</returns>
    private async Task<bool> TrySignInAsync(HttpContext context, User user, List<Claim> claims)
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
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Signing in user {UserId} failed", user.Id);
            return false;
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