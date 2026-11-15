using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Security;

namespace ModulesRegistry.AppTests.Security;

/// <summary>
/// The change log's user (Migration step 2C.5c): from the authentication state in a circuit, from the request otherwise.
/// </summary>
[TestClass]
public sealed class CurrentUserTests
{
    [TestMethod]
    public async Task InRequestTheUserComesFromTheRequest()
    {
        var target = new CurrentUser(new ThrowingAuthenticationStateProvider(), Accessor(User(5)));

        Assert.AreEqual(5, await target.GetUserIdAsync());
    }

    [TestMethod]
    public async Task InCircuitTheUserComesFromTheAuthenticationState()
    {
        var target = new CurrentUser(new FixedAuthenticationStateProvider(User(7)), Accessor(User(5)));
        await new CurrentUserCircuitHandler(target).OnCircuitOpenedAsync(null!, CancellationToken.None);

        Assert.AreEqual(7, await target.GetUserIdAsync());
    }

    [TestMethod]
    public async Task NotSignedInGivesNull()
    {
        var target = new CurrentUser(new ThrowingAuthenticationStateProvider(), Accessor(new ClaimsPrincipal(new ClaimsIdentity())));

        Assert.IsNull(await target.GetUserIdAsync());
    }

    [TestMethod]
    public async Task OutsideRequestGivesNull()
    {
        var target = new CurrentUser(new ThrowingAuthenticationStateProvider(), new HttpContextAccessor());

        Assert.IsNull(await target.GetUserIdAsync());
    }

    private static ClaimsPrincipal User(int userId) =>
        new(new ClaimsIdentity([new Claim(AppClaimTypes.UserId, userId.ToString())], "Test"));

    private static HttpContextAccessor Accessor(ClaimsPrincipal user) =>
        new() { HttpContext = new DefaultHttpContext { User = user } };

    private sealed class FixedAuthenticationStateProvider(ClaimsPrincipal user) : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(user));
    }

    /// <summary>
    /// As the server's provider outside a circuit, where it isn't set.
    /// </summary>
    private sealed class ThrowingAuthenticationStateProvider : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
            throw new InvalidOperationException("GetAuthenticationStateAsync was called before SetAuthenticationState.");
    }
}