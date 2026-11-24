using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Tellurian.Trains.Drawing.Components.Views;
using Tellurian.Trains.Schedules.Exporters.Drawing;
using Tellurian.Trains.Schedules.Exporters.Interfaces;
using Tellurian.Trains.Schedules.Model.Layouts.Halls;

namespace ModulesRegistry.AppTests.Components;

/// <summary>
/// Module Draw's <see cref="HallView"/>, as the hall drawing page uses it: rendered statically (no circuit), it draws the hall as SVG.
/// </summary>
[TestClass]
public sealed class HallViewTests
{
    [TestMethod]
    public async Task AHallFileIsDrawnAsSvgWithoutInteractivity()
    {
        var json = new HallExporter().Write(Hall.Rectangle("Sporthallen", 30000, 20000), ExportOptions.Default);

        var html = await RenderAsync(json);

        StringAssert.Contains(html, "<svg");
        StringAssert.Contains(html, "Sporthallen");
    }

    [TestMethod]
    public async Task AFileThatCantBeReadShowsTheError()
    {
        var html = await RenderAsync("{}");

        StringAssert.Contains(html, "Can't show");
    }

    private static async Task<string> RenderAsync(string json)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IJSRuntime, NoJSRuntime>();
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?> { ["Json"] = json, ["Error"] = "Can't show" });
        return await renderer.Dispatcher.InvokeAsync(async () =>
            System.Net.WebUtility.HtmlDecode((await renderer.RenderComponentAsync<HallView>(parameters)).ToHtmlString()));
    }

    // Static rendering has no JavaScript; the component must not need it to render.
    private sealed class NoJSRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("No JavaScript in static rendering.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new InvalidOperationException("No JavaScript in static rendering.");
    }
}