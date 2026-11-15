using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.Circuits;
using ModulesRegistry.Data.ChangeTracking;
using ModulesRegistry.Data.Extensions;
using System.Security.Claims;

namespace ModulesRegistry.Security;

/// <summary>
/// The signed-in user of the scope, for the change log (Migration step 2C.5c).
/// In a Blazor circuit the user comes from the <see cref="AuthenticationStateProvider"/>,
/// since <see cref="IHttpContextAccessor.HttpContext"/> isn't reliable there; in an HTTP request
/// (endpoints, server rendering) from the request, since the provider isn't set there.
/// </summary>
public sealed class CurrentUser(AuthenticationStateProvider authenticationStateProvider, IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    /// <summary>
    /// Set by <see cref="CurrentUserCircuitHandler"/> when the scope is a circuit.
    /// </summary>
    internal bool IsInCircuit { get; set; }

    // BL0013 is about keeping the state between calls; it is read on each call here, so it can't be stale.
#pragma warning disable BL0013
    public async Task<int?> GetUserIdAsync()
    {
        var principal = IsInCircuit ?
            (await authenticationStateProvider.GetAuthenticationStateAsync()).User :
            httpContextAccessor.HttpContext?.User;
        return principal.UserIdOrNull();
    }
#pragma warning restore BL0013
}

/// <summary>
/// Marks the scope's <see cref="CurrentUser"/> as a circuit before any component runs.
/// </summary>
public sealed class CurrentUserCircuitHandler(CurrentUser currentUser) : CircuitHandler
{
    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        currentUser.IsInCircuit = true;
        return Task.CompletedTask;
    }
}

internal static class CurrentUserExtensions
{
    extension(ClaimsPrincipal? principal)
    {
        public int? UserIdOrNull() => principal.UserId() is > 0 and var id ? id : null;
    }
}