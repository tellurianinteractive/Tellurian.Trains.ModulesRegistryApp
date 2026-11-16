using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ModulesRegistry.AppTests;

/// <summary>
/// Starts the real app and checks the Blazor Web App hosting model (Migration step 2.2): the App document, the interactive
/// root, the catch-all for unknown addresses and the endpoints that replaced the Razor Pages. No database is needed:
/// nothing is prerendered, so no page runs on the server during these requests.
/// </summary>
[TestClass]
public sealed partial class HostingTests
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
    public async Task InteractivePageIsTheAppDocumentWithAnInteractiveRoot()
    {
        using var client = CreateClient();

        // Any page not yet moved to static SSR (the start page was until Migration step 3.2b).
        var response = await client.GetAsync("/Meetings/Index");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        StringAssert.Matches(html, BlazorScript());
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

        // As with the former MapFallbackToPage("/_Host"): the app loads and shows "nothing to display".
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Matches(await response.Content.ReadAsStringAsync(), BlazorScript());
    }

    [TestMethod]
    public async Task UnknownAddressIsRenderedAsStaticPageWithTheLayout()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/no/such/page");

        // NotFound.razor is excluded from interactive routing (Migration step 3.1): the server renders the page and the
        // layout in the response, with no interactive root, and the layout works without a circuit.
        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<!--Blazor:", html, "No interactive root on a static page.");
        StringAssert.Contains(html, "Sorry, nothing to display here.");
        StringAssert.Contains(html, "id=\"nav-menu-state\"", "The menu toggle is a checkbox.");
        StringAssert.Contains(html, "action=\"login\"", "The login form.");
        StringAssert.Contains(html, "class=\"nav-menu\"", "The menu.");
        // Signed out, the returnUrl is the page itself, as in interactive pages.
        StringAssert.Contains(html, "name=\"returnUrl\" value=\"no/such/page\"");
    }

    [TestMethod]
    [DataRow("/About", "<h4 id=\"zweck\">Zweck</h4>")]
    [DataRow("/tools/locopreparationguide", "Bereite Deine Lok für das Modultreffen vor")]
    [DataRow("/Content/articles-documentation", "Dokumentationsübersicht")]
    public async Task ContentPagesAreStaticInTheUsersLanguage(string url, string expected)
    {
        // Migration step 3.2: the content pages are excluded from interactive routing, so they work without JavaScript.
        using var client = CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("Cookie", ".AspNetCore.Culture=c%3Dde%7Cuic%3Dde");

        var response = await client.SendAsync(request);

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<!--Blazor:", html, "No interactive root on a static page.");
        Assert.DoesNotContain("Sorry, nothing to display here.", html);
        StringAssert.Contains(html, "<html lang=\"de\"");
        StringAssert.Contains(html, expected);
    }

    [TestMethod]
    public async Task StartPageAndTermsOfUseAreStatic()
    {
        // Migration step 3.2b. Signed out, there is nothing to accept and no redirect to the terms.
        using var client = CreateClient();

        var start = await client.GetAsync("/");
        var terms = await client.GetAsync("/TermsOfUse");

        Assert.AreEqual(HttpStatusCode.OK, start.StatusCode);
        Assert.AreEqual(HttpStatusCode.OK, terms.StatusCode);
        var startHtml = await start.Content.ReadAsStringAsync();
        var termsHtml = await terms.Content.ReadAsStringAsync();
        Assert.DoesNotContain("<!--Blazor:", startHtml, "No interactive root on the start page.");
        Assert.DoesNotContain("<!--Blazor:", termsHtml, "No interactive root on the terms of use.");
        StringAssert.Contains(termsHtml, "fa-file-signature");
        Assert.DoesNotContain("accept-terms-of-use", termsHtml, "Signed out, there is no accept form.");
    }

    [TestMethod]
    public async Task StaticPageForUsersRequiresSignIn()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/tools");

        Assert.AreEqual(HttpStatusCode.Redirect, response.StatusCode);
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

    [TestMethod]
    public async Task StylesheetsAndScriptsAreFingerprinted()
    {
        // Issue #233: unfingerprinted URLs let browsers keep an old ModulesRegistry.styles.css whose scope ids no longer
        // matched the components, so the waybills lost their layout. A fingerprinted URL changes with the file's content.
        // (Only the publish manifest marks them immutable; the build manifest used here says no-cache.)
        using var client = CreateClient();
        var html = await (await client.GetAsync("/")).Content.ReadAsStringAsync();

        var urls = AssetReference().Matches(html).Select(m => m.Groups["url"].Value).ToList();

        Assert.IsGreaterThanOrEqualTo(6, urls.Count, "Bootstrap, Font Awesome, site.css, the scoped CSS bundle, blazor.web.js and person-search.js.");
        foreach (var url in urls)
        {
            StringAssert.Matches(url, Fingerprinted(), $"{url} is not fingerprinted.");
            var response = await client.GetAsync("/" + url);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, url);
        }
    }

    [GeneratedRegex(@"_framework/blazor\.web\.[a-z0-9]+\.js")]
    private static partial Regex BlazorScript();

    [GeneratedRegex(@"(?:<link href|<script src)=""(?<url>[^""]+)""")]
    private static partial Regex AssetReference();

    [GeneratedRegex(@"\.[a-z0-9]{10}\.(?:styles\.css|css|js)$")]
    private static partial Regex Fingerprinted();
}