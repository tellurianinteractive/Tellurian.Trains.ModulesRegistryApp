using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ModulesRegistry.Api;

namespace ModulesRegistry.AppTests.Api;

/// <summary>
/// The real data source of the public API, from the app's services. The languages come from Tellurian.Localization's
/// <c>ILanguageService</c> since Migration step 2B.3, and must be what the API returned before.
/// </summary>
[TestClass]
public sealed class PublicApiSourceTests
{
    [TestMethod]
    public void SupportedLanguagesAreTheUserInterfaceLanguagesAsAnArray()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));
        using var scope = factory.Services.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<IPublicApiSource>();

        var languages = source.SupportedLanguages();

        // An array, not a list: the former format of /api/languages/all/supported is a plain JSON array.
        Assert.IsInstanceOfType<string[]>(languages);
        CollectionAssert.AreEqual(new[] { "en", "de", "da", "sv", "nb" }, languages.ToArray());
    }
}