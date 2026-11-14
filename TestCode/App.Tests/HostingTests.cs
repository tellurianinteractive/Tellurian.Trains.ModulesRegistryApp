using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ModulesRegistry.AppTests;

/// <summary>
/// Starts the real app and checks the Blazor Web App hosting model (Migration step 2.2): the App document, the interactive
/// root, the catch-all for unknown addresses and the endpoints that replaced the Razor Pages. No database is needed:
/// nothing is prerendered, so no page runs on the server during these requests.
/// </summary>
[TestClass]
public sealed class HostingTests
{
    private static WebApplicationFactory<Program> _factory = null!;

    [ClassInitialize]
    public static void ClassInitialize(TestContext _) =>
        // Static web assets (such as _framework/blazor.web.js) come from the build manifest, as in Development; a publish puts them in wwwroot.
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Testing").UseStaticWebAssets());

    [ClassCleanup]
    public static void ClassCleanup() => _factory.Dispose();

    private static HttpClient CreateClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    [TestMethod]
    public async Task StartPageIsTheAppDocumentWithAnInteractiveRoot()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(html, "_framework/blazor.web.js");
        Assert.DoesNotContain("blazor.server.js", html);
        StringAssert.Contains(html, "<!--Blazor:", "The interactive root component marker.");
        StringAssert.Contains(html, "<base href=\"/\">");
        StringAssert.Contains(html, "id=\"blazor-error-ui\"");
    }

    [TestMethod]
    public async Task StartPageSetsTheCultureAndAntiforgeryCookies()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/");

        var cookies = response.Headers.GetValues("Set-Cookie").Select(c => c.Split('=')[0]).ToList();
        Assert.Contains(".AspNetCore.Culture", cookies);
        Assert.IsTrue(cookies.Any(c => c.StartsWith(".AspNetCore.Antiforgery.")), "The login form needs the antiforgery cookie.");
    }

    [TestMethod]
    [DataRow("/no/such/page")]
    [DataRow("/Closed")]
    public async Task UnknownAddressStillServesTheApp(string url)
    {
        using var client = CreateClient();

        var response = await client.GetAsync(url);

        // As with the former MapFallbackToPage("/_Host"): the app loads and the router shows "nothing to display".
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "_framework/blazor.web.js");
    }

    [TestMethod]
    public async Task MissingFileIsNotFound()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/images/no-such-image.png");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task BlazorScriptIsServed()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/_framework/blazor.web.js");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task CircuitHubAcceptsConnections()
    {
        using var client = CreateClient();

        var response = await client.PostAsync("/_blazor/negotiate?negotiateVersion=1", content: null);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    public async Task LogoutRedirectsToTheStartPage()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/logout?returnUrl=/");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.AreEqual("/", response.Headers.Location?.OriginalString);
        Assert.IsTrue(response.Headers.GetValues("Set-Cookie").Any(c => c.StartsWith(".AspNetCore.Cookies=;")), "The authentication cookie is cleared.");
    }

    [TestMethod]
    public async Task PublicApiAndLoginStillTakePrecedenceOverTheCatchAll()
    {
        using var client = CreateClient();

        Assert.AreEqual(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/cargoes")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Redirect, (await client.GetAsync("/login")).StatusCode);
        Assert.AreEqual(HttpStatusCode.Redirect, (await client.GetAsync("/Documents/1")).StatusCode);
    }
}