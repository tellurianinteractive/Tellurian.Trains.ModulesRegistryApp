using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModulesRegistry.Endpoints;
using ModulesRegistry.Security;

namespace ModulesRegistry.AppTests.Endpoints;

/// <summary>
/// Runs the real document download endpoint and authorization policies in a test server, with a test sign-in and fake documents.
/// Only signed-in users who meet the User policy may download; an API key gives no access.
/// Exception: a public link with a key signed for that document id.
/// </summary>
[TestClass]
public sealed class DocumentEndpointTests
{
    private WebApplication _app = null!;
    private HttpClient _client = null!;
    private FakeDocumentSource _source = null!;
    private PublicDocumentKeys _publicKeys = null!;

    [TestInitialize]
    public async Task InitializeAsync()
    {
        _source = new FakeDocumentSource();
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(TestAuthenticationHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.SchemeName, null);
        builder.Services.AddAuthorizationPolicies();
        builder.Services.AddSingleton<IDocumentSource>(_source);
        builder.Services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
        builder.Services.AddSingleton<PublicDocumentKeys>();
        _app = builder.Build();
        _app.UseAuthentication();
        _app.UseAuthorization();
        _app.MapDocumentDownload();
        await _app.StartAsync();
        _publicKeys = _app.Services.GetRequiredService<PublicDocumentKeys>();
        _client = _app.GetTestClient();
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        _client.Dispose();
        await _app.DisposeAsync();
    }

    [TestMethod]
    public async Task NotSignedInGetsNoDocument()
    {
        var response = await GetAsync("/Documents/5", user: null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    public async Task ApiKeyGivesNoAccess()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/Documents/5");
        request.Headers.Add("X-API-Key", "00000000-0000-0000-0000-000000000001");

        var response = await _client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    [DataRow("no-terms")]
    [DataRow("locked-out")]
    public async Task UserNotMeetingUserPolicyIsForbidden(string user)
    {
        var response = await GetAsync("/Documents/5", user);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    public async Task SignedInUserDownloadsTheFile()
    {
        var response = await GetAsync("/Documents/5", "user");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(5, _source.Id);
        Assert.AreEqual("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual("Station A.pdf", response.Content.Headers.ContentDisposition?.FileNameStar ?? response.Content.Headers.ContentDisposition?.FileName);
        CollectionAssert.AreEqual(_source.Content, await response.Content.ReadAsByteArrayAsync());
        Assert.IsTrue(response.Headers.CacheControl?.NoStore, "Documents must not be cached.");
    }

    [TestMethod]
    public async Task PublicLinkWorksWithoutSigningIn()
    {
        var response = await GetAsync("/" + DocumentEndpoint.PublicUrl(5, _publicKeys.Create(5)), user: null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(5, _source.Id);
    }

    [TestMethod]
    public async Task PublicKeyOnlyWorksForItsOwnDocument()
    {
        var response = await GetAsync("/" + DocumentEndpoint.PublicUrl(6, _publicKeys.Create(5)), user: null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("not-a-key")]
    [DataRow("5")]
    public async Task InvalidPublicKeyGivesNoAccess(string key)
    {
        var response = await GetAsync($"/Documents/5?key={Uri.EscapeDataString(key)}", user: null);

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.IsFalse(_source.WasCalled);
    }

    [TestMethod]
    public async Task PublicLinkAlsoWorksForUserNotMeetingUserPolicy()
    {
        var response = await GetAsync("/" + DocumentEndpoint.PublicUrl(5, _publicKeys.Create(5)), "no-terms");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task UnknownDocumentIsNotFound()
    {
        var response = await GetAsync("/Documents/404", "user");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    private Task<HttpResponseMessage> GetAsync(string url, string? user)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (user is not null) request.Headers.Add(TestAuthenticationHandler.UserHeader, user);
        return _client.SendAsync(request);
    }

    private sealed class FakeDocumentSource : IDocumentSource
    {
        public byte[] Content { get; } = [1, 2, 3, 4];
        public bool WasCalled { get; private set; }
        public int Id { get; private set; }

        public Task<DocumentFile?> FindAsync(int id)
        {
            (WasCalled, Id) = (true, id);
            return Task.FromResult(id == 404 ? null : new DocumentFile(Content, "application/pdf", "Station A.pdf"));
        }
    }
}