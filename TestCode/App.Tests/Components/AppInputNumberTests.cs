using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModulesRegistry.Components;

namespace ModulesRegistry.AppTests.Components;

/// <summary>
/// A year field of a period (<see cref="AppInputNumber{TValue}.PeriodEnd"/>) says in the empty field and in its tooltip how an
/// empty year is read; other number fields have neither.
/// </summary>
[TestClass]
public sealed class AppInputNumberTests
{
    private sealed class Model
    {
        public int? Year { get; set; }
    }

    [TestMethod]
    public async Task FromYearSaysNoLimitAndNoStartYear()
    {
        var html = await RenderAsync(YearPeriodEnd.From);

        StringAssert.Contains(html, "placeholder=\"No limit\"");
        StringAssert.Contains(html, "title=\"Empty means no start year. The year itself is included.\"");
    }

    [TestMethod]
    public async Task UptoYearSaysNoLimitAndNoEndYear()
    {
        var html = await RenderAsync(YearPeriodEnd.Upto);

        StringAssert.Contains(html, "placeholder=\"No limit\"");
        StringAssert.Contains(html, "title=\"Empty means no end year. The year itself is included.\"");
    }

    [TestMethod]
    public async Task OtherNumberFieldsHaveNoPlaceholderOrTooltip()
    {
        var html = await RenderAsync(null);

        Assert.DoesNotContain("placeholder", html);
        Assert.DoesNotContain("title=", html);
    }

    private static async Task<string> RenderAsync(YearPeriodEnd? periodEnd)
    {
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo("en-GB");
        var services = new ServiceCollection();
        services.AddLogging().AddLocalization();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        var model = new Model();
        Expression<Func<int?>> valueExpression = () => model.Year;
        // An input needs the form's EditContext, as inside EditTemplate.
        RenderFragment input = builder =>
        {
            builder.OpenComponent<AppInputNumber<int?>>(0);
            builder.AddComponentParameter(1, nameof(AppInputNumber<int?>.Label), "FromYear");
            builder.AddComponentParameter(2, nameof(AppInputNumber<int?>.PeriodEnd), periodEnd);
            builder.AddComponentParameter(3, nameof(AppInputNumber<int?>.Value), model.Year);
            builder.AddComponentParameter(4, nameof(AppInputNumber<int?>.ValueExpression), valueExpression);
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