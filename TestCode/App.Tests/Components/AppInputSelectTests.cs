using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModulesRegistry.Components;
using ModulesRegistry.Data;

namespace ModulesRegistry.AppTests.Components;

/// <summary>
/// <see cref="AppInputSelect"/> with Typeahead marks the list and its options for <c>wwwroot/js/select-typeahead.js</c>;
/// without it, the list is rendered as before.
/// </summary>
[TestClass]
public sealed class AppInputSelectTests
{
    private sealed class Model
    {
        public int CargoId { get; set; } = 12;
    }

    private static readonly ListboxItem[] Items =
    [
        new(12, "7203 Steel coils (Shimmns)") { SearchCode = "72031000", SearchName = "Steel coils" },
        new(13, "---- Mixed goods (Gbs)") { SearchName = "Mixed goods" },
    ];

    [TestMethod]
    public async Task TypeaheadMarksTheListAndItsOptions()
    {
        var html = await RenderAsync(typeahead: true);

        StringAssert.Contains(html, "data-typeahead=\"\"");
        StringAssert.Contains(html, "data-typeahead-placeholder=\"Type NHM code or name\"");
        StringAssert.Contains(html, "data-typeahead-not-found=");
        StringAssert.Contains(html, "value=\"12\" data-code=\"72031000\" data-name=\"Steel coils\"");
        StringAssert.Contains(html, "<option value=\"13\" data-name=\"Mixed goods\">---- Mixed goods (Gbs)</option>");
    }

    [TestMethod]
    public async Task WithoutTypeaheadTheListIsAsBefore()
    {
        var html = await RenderAsync(typeahead: false);

        Assert.DoesNotContain("data-typeahead", html);
        Assert.DoesNotContain("data-code", html);
        Assert.DoesNotContain("data-name", html);
        StringAssert.Contains(html, "<option selected=\"selected\" value=\"12\" selected>7203 Steel coils (Shimmns)</option>");
    }

    private static async Task<string> RenderAsync(bool typeahead)
    {
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("en-GB");
        var services = new ServiceCollection();
        services.AddLogging().AddLocalization();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        var model = new Model();
        Expression<Func<int>> valueExpression = () => model.CargoId;
        // An input needs the form's EditContext, as inside EditTemplate.
        RenderFragment select = builder =>
        {
            builder.OpenComponent<AppInputSelect>(0);
            builder.AddComponentParameter(1, nameof(AppInputSelect.Label), "CargoType");
            builder.AddComponentParameter(2, nameof(AppInputSelect.Items), Items);
            builder.AddComponentParameter(3, nameof(AppInputSelect.ShowPleaseSelect), true);
            builder.AddComponentParameter(4, nameof(AppInputSelect.Typeahead), typeahead);
            builder.AddComponentParameter(5, nameof(AppInputSelect.TypeaheadPlaceholder), "TypeNhmCodeOrName");
            builder.AddComponentParameter(6, nameof(AppInputSelect.Value), model.CargoId);
            builder.AddComponentParameter(7, nameof(AppInputSelect.ValueExpression), valueExpression);
            builder.CloseComponent();
        };
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<CascadingValue<EditContext>>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(CascadingValue<EditContext>.Value)] = new EditContext(model),
                [nameof(CascadingValue<EditContext>.ChildContent)] = select,
            }));
            return System.Net.WebUtility.HtmlDecode(output.ToHtmlString());
        });
    }
}