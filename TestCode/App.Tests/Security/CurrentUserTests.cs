using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Security;

namespace ModulesRegistry.AppTests.Security;

/// <summary>
/// The change log's user (Migration step 2C.5c): from the request (since step 3.7 there are no circuits).
/// </summary>
[TestClass]
public sealed class CurrentUserTests
{
    [TestMethod]
    public async Task TheUserComesFromTheRequest() =>
        Assert.AreEqual(5, await new CurrentUser(Accessor(User(5))).GetUserIdAsync());

    [TestMethod]
    public async Task NotSignedInGivesNull() =>
        Assert.IsNull(await new CurrentUser(Accessor(new ClaimsPrincipal(new ClaimsIdentity()))).GetUserIdAsync());

    [TestMethod]
    public async Task OutsideRequestGivesNull() =>
        Assert.IsNull(await new CurrentUser(new HttpContextAccessor()).GetUserIdAsync());

    private static ClaimsPrincipal User(int userId) =>
        new(new ClaimsIdentity([new Claim(AppClaimTypes.UserId, userId.ToString())], "Test"));

    private static HttpContextAccessor Accessor(ClaimsPrincipal user) =>
        new() { HttpContext = new DefaultHttpContext { User = user } };
}