using System.Net;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModulesRegistry.Security;

namespace ModulesRegistry.AppTests.Security;

/// <summary>
/// Runs the real login endpoint with antiforgery validation in a test server, with a fake sign-in.
/// The credentials must only be accepted from a posted form, never from the URL.
/// </summary>
[TestClass]
public sealed class LoginEndpointTests
{
    private const string TokenUrl = "/test/antiforgery";

    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private FakeLoginHandler _handler = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _handler = new FakeLoginHandler();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAntiforgery();
        builder.Services.AddSingleton<ILoginHandler>(_handler);
        _app = builder.Build();
        _app.UseAntiforgery();
        _app.MapLogin();
        // Stands in for _Host.cshtml, which creates the token and the antiforgery cookie.
        _app.MapGet(TokenUrl, (HttpContext context, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return $"{tokens.FormFieldName}={tokens.RequestToken}";
        });
        await _app.StartAsync();
        _client = _app.GetTestClient();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [TestMethod]
    public async Task PostedFormSignsInAndRedirectsToReturnUrl()
    {
        var response = await PostLoginAsync(new() { ["username"] = "anna@example.com", ["password"] = "secret", ["returnUrl"] = "Meetings/5" });

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/Meetings/5", response.Headers.Location?.OriginalString);
        Assert.AreEqual(("anna@example.com", "secret"), _handler.SignIn);
    }

    [TestMethod]
    public async Task PostWithoutAntiforgeryTokenIsRejected()
    {
        var response = await _client.PostAsync(LoginEndpoint.Route, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["username"] = "anna@example.com",
            ["password"] = "secret",
        }));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.IsNull(_handler.SignIn);
    }

    [TestMethod]
    public async Task CredentialsInTheUrlDontSignIn()
    {
        var response = await _client.GetAsync($"{LoginEndpoint.Route}?username=anna%40example.com&password=secret&returnUrl=Meetings");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/", response.Headers.Location?.OriginalString);
        Assert.IsNull(_handler.SignIn);
    }

    [TestMethod]
    [DataRow("", "secret")]
    [DataRow("anna@example.com", "")]
    [DataRow("  ", "  ")]
    public async Task MissingCredentialsRedirectToStartPageWithoutSignIn(string username, string password)
    {
        var response = await PostLoginAsync(new() { ["username"] = username, ["password"] = password, ["returnUrl"] = "Meetings" });

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/", response.Headers.Location?.OriginalString);
        Assert.IsNull(_handler.SignIn);
    }

    [TestMethod]
    [DataRow(null, "/")]
    [DataRow("", "/")]
    [DataRow("Meetings/5", "/Meetings/5")]
    [DataRow("/Meetings/5", "/Meetings/5")]
    [DataRow("Meetings?countryId=1", "/Meetings?countryId=1")]
    [DataRow("/", "/")]
    [DataRow("//evil.example.com", "/")]
    [DataRow("/\\evil.example.com", "/")]
    [DataRow("\\evil.example.com", "/")]
    [DataRow("https://evil.example.com", "/https://evil.example.com")]
    public void ReturnUrlIsAlwaysLocal(string? returnUrl, string expected)
    {
        Assert.AreEqual(expected, LoginEndpoint.ReturnUrlOrRoot(PathString.Empty, returnUrl));
    }

    [TestMethod]
    public void ReturnUrlKeepsThePathBase()
    {
        Assert.AreEqual("/registry/Meetings", LoginEndpoint.ReturnUrlOrRoot(new PathString("/registry"), "Meetings"));
        Assert.AreEqual("/registry/Meetings", LoginEndpoint.ReturnUrlOrRoot(new PathString("/registry"), "/registry/Meetings"));
    }

    private async Task<HttpResponseMessage> PostLoginAsync(Dictionary<string, string> fields)
    {
        var tokenResponse = await _client.GetAsync(TokenUrl);
        var token = (await tokenResponse.Content.ReadAsStringAsync()).Split('=', 2);
        var cookies = tokenResponse.Headers.GetValues("Set-Cookie").Select(c => c.Split(';')[0]);

        fields[token[0]] = token[1];
        var request = new HttpRequestMessage(HttpMethod.Post, LoginEndpoint.Route) { Content = new FormUrlEncodedContent(fields) };
        request.Headers.Add("Cookie", string.Join("; ", cookies));
        return await _client.SendAsync(request);
    }

    private sealed class FakeLoginHandler : ILoginHandler
    {
        public (string Username, string Password)? SignIn { get; private set; }

        public Task SignInAsync(HttpContext context, string username, string password)
        {
            SignIn = (username, password);
            return Task.CompletedTask;
        }
    }
}