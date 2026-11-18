using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;
using Microsoft.JSInterop;
using ModulesRegistry.Components;

namespace ModulesRegistry.AppTests.Components;

/// <summary>
/// <see cref="FilterColumn{TGridItem, TProp}"/> renders statically (Migration step 3.3): QuickGrid's sort link stays in the
/// header, with the filter field below it, joined to the filter form.
/// </summary>
[TestClass]
public sealed class FilterColumnTests
{
    private sealed record Item(string Name);

    [TestMethod]
    public async Task HeaderHasTheSortLinkAndTheFilterField()
    {
        var html = await RenderAsync("http://localhost/list?name=ab&sort=Name&direction=asc", "ab");

        StringAssert.Contains(html, "<a ", "The sortable title is still a link.");
        StringAssert.Contains(html, "class=\"col-title\"");
        StringAssert.Contains(html, "name=\"name\"");
        StringAssert.Contains(html, "form=\"the-filter\"");
        StringAssert.Contains(html, "value=\"ab\"");
        StringAssert.Contains(html, "aria-label=\"Name\"");
        StringAssert.Contains(html, "placeholder=\"Enter search value\"", "It isn't obvious that the field is for filtering.");
        StringAssert.Contains(html, "title=\"Enter search value\"");
        StringAssert.Contains(html, "abc", "The rows are rendered.");
    }

    [TestMethod]
    public async Task EmptyFilterHasNoValue()
    {
        var html = await RenderAsync("http://localhost/list", null);

        StringAssert.Contains(html, "name=\"name\"");
        Assert.DoesNotContain("value=", html.Split("col-filter")[1].Split('>')[0], "No value attribute without a filter.");
    }

    private static async Task<string> RenderAsync(string uri, string? filterValue)
    {
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("en-GB");
        var services = new ServiceCollection();
        services.AddLogging().AddLocalization();
        services.AddSingleton<NavigationManager>(new TestNavigationManager(uri));
        services.AddSingleton<IJSRuntime, NoJSRuntime>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        Item[] list = [new("abc"), new("xyz")];
        var items = list.AsQueryable();

        RenderFragment columns = builder =>
        {
            builder.OpenComponent<FilterColumn<Item, string>>(0);
            builder.AddComponentParameter(1, nameof(FilterColumn<Item, string>.Property), (System.Linq.Expressions.Expression<Func<Item, string>>)(i => i.Name));
            builder.AddComponentParameter(2, nameof(FilterColumn<Item, string>.Title), "Name");
            builder.AddComponentParameter(3, nameof(FilterColumn<Item, string>.Sortable), true);
            builder.AddComponentParameter(4, nameof(FilterColumn<Item, string>.FilterName), "name");
            builder.AddComponentParameter(5, nameof(FilterColumn<Item, string>.FilterForm), "the-filter");
            builder.AddComponentParameter(6, nameof(FilterColumn<Item, string>.FilterValue), filterValue);
            builder.CloseComponent();
        };
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<QuickGrid<Item>>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(QuickGrid<Item>.Items)] = items,
                [nameof(QuickGrid<Item>.ChildContent)] = columns,
            }));
            return output.ToHtmlString();
        });
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string uri) => Initialize("http://localhost/", uri);
        protected override void NavigateToCore(string uri, NavigationOptions options) => Uri = ToAbsoluteUri(uri).ToString();
    }

    private sealed class NoJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("No JavaScript in static rendering.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new InvalidOperationException("No JavaScript in static rendering.");
    }
}