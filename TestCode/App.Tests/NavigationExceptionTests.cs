using System.Text.Json;

namespace ModulesRegistry.AppTests;

/// <summary>
/// A redirect from a static page (<c>NavigationManager.NavigateTo</c>) must not throw <c>NavigationException</c>: the
/// exception was also reported as an unobserved task exception and logged as Critical, once for every redirect.
/// <c>BlazorDisableThrowNavigationException</c> in App.csproj turns it off; it ends up in the app's runtime configuration.
/// </summary>
[TestClass]
public sealed class NavigationExceptionTests
{
    private const string Switch = "Microsoft.AspNetCore.Components.Endpoints.NavigationManager.DisableThrowNavigationException";

    [TestMethod]
    public void RedirectsFromStaticPagesDoNotThrow()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "ModulesRegistry.runtimeconfig.json");
        using var config = JsonDocument.Parse(File.ReadAllText(path));

        var options = config.RootElement.GetProperty("runtimeOptions").GetProperty("configProperties");

        Assert.IsTrue(options.TryGetProperty(Switch, out var value) && value.GetBoolean(), $"{Switch} is not set; set BlazorDisableThrowNavigationException in App.csproj.");
    }
}