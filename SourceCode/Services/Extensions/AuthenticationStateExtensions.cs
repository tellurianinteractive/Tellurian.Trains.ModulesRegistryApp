using Microsoft.AspNetCore.Components.Authorization;

namespace ModulesRegistry.Services.Extensions;

public static class AuthenticationStateExtensions
{
    public static async Task<bool> IsAuthenticatedAsync(this Task<AuthenticationState>?  authenticationStateTask)
    {
        var user = await authenticationStateTask.GetClaimsPrincipalAsync();
        return user?.Identity?.IsAuthenticated == true;
    }

    public static async Task<IEnumerable<Claim>> ClaimsAsync(this Task<AuthenticationState>? authenticationStateTask)
    {
        var user = await authenticationStateTask.GetClaimsPrincipalAsync();
        if (user is null) return Array.Empty<Claim>();
        return user.Claims;
    }

    public async static Task<ClaimsPrincipal?> GetClaimsPrincipalAsync(this Task<AuthenticationState>? authenticationStateTask) =>
        authenticationStateTask is null ? null : (await authenticationStateTask).User;
}
