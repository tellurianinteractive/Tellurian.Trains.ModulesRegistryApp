using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Security;
using ModulesRegistry.Services.Diagnostics;

namespace ModulesRegistry.AppTests.Security;

/// <summary>
/// The logout endpoint logs a signed-in user's sign-out as Warning with the user id (Migration step 2C.4).
/// </summary>
[TestClass]
public sealed class LogoutLoggingTests
{
    private const string SignedInHeader = "X-Test-Signed-In";

    [TestMethod]
    public async Task SignOutOfASignedInUserIsLoggedWithTheUserId()
    {
        var logs = new FakeLogCollector();
        await using var app = await StartAsync(logs);
        using var client = app.GetTestClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, LoginEndpoint.LogoutRoute);
        request.Headers.Add(SignedInHeader, "true");

        var response = await client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        var record = logs.GetSnapshot().Single(r => r.Id.Id == SecurityLog.SignedOutEventId);
        Assert.AreEqual(LogLevel.Warning, record.Level);
        Assert.AreEqual("User 10 signed out", record.Message);
    }

    [TestMethod]
    public async Task LogoutWithoutASignedInUserIsNotLogged()
    {
        var logs = new FakeLogCollector();
        await using var app = await StartAsync(logs);
        using var client = app.GetTestClient();

        var response = await client.GetAsync(LoginEndpoint.LogoutRoute);

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.IsFalse(logs.GetSnapshot().Any(r => r.Id.Id == SecurityLog.SignedOutEventId));
    }

    private static async Task<WebApplication> StartAsync(FakeLogCollector logs)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders().AddProvider(new FakeLoggerProvider(logs));
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        builder.Services.AddSingleton<ILoginHandler, NoLoginHandler>();
        var app = builder.Build();
        // Stands in for the cookie and the claims transformation: a signed-in user has an object id and a user id.
        app.Use((context, next) =>
        {
            if (context.Request.Headers.ContainsKey(SignedInHeader))
                context.User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new(AppClaimTypes.ObjectId, Guid.NewGuid().ToString()), new(AppClaimTypes.UserId, "10")], "Test"));
            return next(context);
        });
        app.MapLogin();
        await app.StartAsync();
        return app;
    }

    private sealed class NoLoginHandler : ILoginHandler
    {
        public Task SignInAsync(HttpContext context, string username, string password) => Task.CompletedTask;
    }
}