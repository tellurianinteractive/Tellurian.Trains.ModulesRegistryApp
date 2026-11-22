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
/// <see cref="AppInputColorSelect"/> with HideLabel shows no label but is still named for screen readers, and keeps the place of
/// the label so the swatches line up with the fields beside them.
/// </summary>
[TestClass]
public sealed class AppInputColorSelectTests
{
    private sealed class Model
    {
        public string Color { get; set; } = RegionColors.Red;
    }

    [TestMethod]
    public async Task HiddenLabelIsReadByScreenReadersOnly()
    {
        var html = await RenderAsync(hideLabel: true);

        Assert.DoesNotContain("<legend", html);
        StringAssert.Contains(html, "aria-label=\"Colour\"");
        StringAssert.Contains(html, "color-swatch-list-aligned");
    }

    [TestMethod]
    public async Task VisibleLabelIsTheLegend()
    {
        var html = await RenderAsync(hideLabel: false);

        StringAssert.Contains(html, "<legend class=\"form-control-label\">Colour</legend>");
        Assert.DoesNotContain("aria-label", html);
        Assert.DoesNotContain("color-swatch-list-aligned", html);
    }

    private static async Task<string> RenderAsync(bool hideLabel)
    {
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("en-GB");
        var services = new ServiceCollection();
        services.AddLogging().AddLocalization();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        var model = new Model();
        Expression<Func<string>> valueExpression = () => model.Color;
        // An input needs the form's EditContext, as inside EditTemplate.
        RenderFragment input = builder =>
        {
            builder.OpenComponent<AppInputColorSelect>(0);
            builder.AddComponentParameter(1, nameof(AppInputColorSelect.Label), "Color");
            builder.AddComponentParameter(2, nameof(AppInputColorSelect.HideLabel), hideLabel);
            builder.AddComponentParameter(3, nameof(AppInputColorSelect.Colors), RegionColors.All);
            builder.AddComponentParameter(4, nameof(AppInputColorSelect.Value), model.Color);
            builder.AddComponentParameter(5, nameof(AppInputColorSelect.ValueExpression), valueExpression);
            builder.CloseComponent();
        };
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<CascadingValue<EditContext>>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(CascadingValue<EditContext>.Value)] = new EditContext(model),
                [nameof(CascadingValue<EditContext>.ChildContent)] = input,
            }));
            return System.Net.WebUtility.HtmlDecode(output.ToHtmlString());
        });
    }
}