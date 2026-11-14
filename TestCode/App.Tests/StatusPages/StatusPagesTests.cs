using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.StatusPages;

namespace ModulesRegistry.AppTests.StatusPages;

/// <summary>
/// The error page and the site unavailable page must work when nothing else does: self-contained, in all fully supported languages.
/// </summary>
[TestClass]
public sealed partial class StatusPagesTests
{
    private static string PagePath(string fileName) => Path.Combine(AppContext.BaseDirectory, "StatusPages", fileName);

    [TestMethod]
    public async Task FailingRequestGetsTheErrorPage()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        await using var app = builder.Build();
        app.UseErrorPage();
        app.MapGet("/fail", (HttpContext _) => { throw new InvalidOperationException("Test failure"); });
        await app.StartAsync();
        using var client = app.GetTestClient();

        var response = await client.GetAsync("/fail");

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.AreEqual("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.IsTrue(response.Headers.CacheControl?.NoStore);
        var html = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(html, "data-page=\"error\"");
        Assert.DoesNotContain(ErrorPage.RequestIdPlaceholder, html, "The reference must be filled in.");
        Assert.DoesNotContain("Test failure", html, "Exception details must not be shown.");
    }

    [TestMethod]
    [DataRow(ErrorPage.FileName)]
    [DataRow("site-unavailable.html")]
    public void PageHasAllFullySupportedLanguages(string fileName)
    {
        var html = File.ReadAllText(PagePath(fileName));

        var sections = SectionLanguage().Matches(html).Select(m => m.Groups[1].Value).ToArray();
        CollectionAssert.AreEquivalent(LanguageExtensions.FullySupportedLanguages, sections);
        var scriptLanguages = ScriptLanguages().Match(html).Groups[1].Value.Split(',').Select(l => l.Trim().Trim('"')).ToArray();
        CollectionAssert.AreEquivalent(LanguageExtensions.FullySupportedLanguages, scriptLanguages);
    }

    [TestMethod]
    [DataRow(ErrorPage.FileName)]
    [DataRow("site-unavailable.html")]
    public void PageLoadsNothingElse(string fileName)
    {
        var html = File.ReadAllText(PagePath(fileName));

        Assert.DoesNotContain("<link", html);
        Assert.DoesNotContain(" src=", html);
        Assert.DoesNotContain("@import", html);
        Assert.DoesNotContain("url(", html);
    }

    [TestMethod]
    public void SiteUnavailablePageIsNeverInTheOutputAsAppOffline()
    {
        // An app_offline.htm next to the app takes the site offline; only the deploy workflow may put it there.
        Assert.IsFalse(File.Exists(Path.Combine(AppContext.BaseDirectory, "app_offline.htm")));
        Assert.IsFalse(File.Exists(Path.Combine(AppContext.BaseDirectory, "wwwroot", "app_offline.htm")));
    }

    [GeneratedRegex("<section data-lang=\"([a-z]{2})\"")]
    private static partial Regex SectionLanguage();

    [GeneratedRegex(@"var supported = \[([^\]]*)\]")]
    private static partial Regex ScriptLanguages();
}