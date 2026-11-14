using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.AppTests;

/// <summary>
/// Signs in according to the <c>X-Test-User</c> header: <c>user</c> meets the User policy,
/// <c>no-terms</c> hasn't accepted the terms of use, <c>locked-out</c> is locked out. No header means not signed in.
/// </summary>
internal sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserHeader = "X-Test-User";
    public const int UsersCountryId = 1;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var user)) return Task.FromResult(AuthenticateResult.NoResult());
        List<Claim> claims = [new(AppClaimTypes.UserId, "10"), new(AppClaimTypes.CountryId, UsersCountryId.ToString())];
        if (user != "no-terms") claims.Add(new(AppClaimTypes.LastTermsOfUseAcceptTime, "2026-01-10"));
        if (user == "locked-out") claims.Add(new(AppClaimTypes.IsLockedOut, "True"));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}