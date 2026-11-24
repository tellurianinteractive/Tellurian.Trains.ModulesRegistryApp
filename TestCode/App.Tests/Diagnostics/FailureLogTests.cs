using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Components;
using ModulesRegistry.Diagnostics;
using ModulesRegistry.Extensions;
using ModulesRegistry.Resources;

namespace ModulesRegistry.AppTests.Diagnostics;

/// <summary>
/// Every failure shown to a user is logged as a warning with the message's key, never its text.
/// </summary>
[TestClass]
public sealed class FailureLogTests
{
    [TestMethod]
    public void KeyIsReturnedAsItIs() =>
        Assert.AreEqual("SaveFailed", AppTexts.KeyOf("SaveFailed", new CultureInfo("sv")));

    [TestMethod]
    public void TranslatedTextGivesItsKey() =>
        Assert.AreEqual("SaveFailed", AppTexts.KeyOf("Det gick inte att spara!", new CultureInfo("sv")));

    [TestMethod]
    public void TextWithValuesFilledInHasNoKey() =>
        Assert.IsNull(AppTexts.KeyOf("Adressen anna@example.com används av en annan användare", new CultureInfo("sv")));

    [TestMethod]
    public void FailureAfterRedirectIsLogged()
    {
        var logs = new FakeLogCollector();
        var context = Context(logs, "/Meetings/5/Delete");

        context.SetFlashMessage("DeleteFailed", isSuccess: false);

        var record = logs.GetSnapshot().Single();
        Assert.AreEqual(LogLevel.Warning, record.Level);
        Assert.AreEqual(FailureLog.FlashMessageEventId, record.Id.Id);
        Assert.AreEqual("Failure shown after POST /Meetings/5/Delete: DeleteFailed, user 42", record.Message);
    }

    [TestMethod]
    [DataRow(FlashMessageLevel.Success)]
    [DataRow(FlashMessageLevel.Warning)]
    [DataRow(FlashMessageLevel.Info)]
    public void OtherMessagesAfterRedirectAreNotLogged(FlashMessageLevel level)
    {
        var logs = new FakeLogCollector();

        Context(logs, "/Meetings/5/Edit").SetFlashMessage("Saved", level);

        Assert.IsEmpty(logs.GetSnapshot());
    }

    [TestMethod]
    public async Task FailureOnPageIsLogged()
    {
        var logs = new FakeLogCollector();

        await RenderActionMessage(logs, "SaveFailed", "stop");

        var record = logs.GetSnapshot().Single();
        Assert.AreEqual(LogLevel.Warning, record.Level);
        Assert.AreEqual(FailureLog.PageMessageEventId, record.Id.Id);
        Assert.AreEqual("SaveFailed", record.StructuredState!.Single(s => s.Key == "MessageKey").Value);
    }

    [TestMethod]
    public async Task UnknownTextOnPageIsLoggedWithoutTheText()
    {
        var logs = new FakeLogCollector();

        await RenderActionMessage(logs, "anna@example.com is taken", "error");

        var record = logs.GetSnapshot().Single();
        Assert.AreEqual(FailureLog.UnknownText, record.StructuredState!.Single(s => s.Key == "MessageKey").Value);
        Assert.DoesNotContain("anna", record.Message);
    }

    [TestMethod]
    [DataRow("info")]
    [DataRow("warning")]
    public async Task OtherMessagesOnPageAreNotLogged(string alertType)
    {
        var logs = new FakeLogCollector();

        await RenderActionMessage(logs, "NoRecordsToDisplay", alertType);

        Assert.IsEmpty(logs.GetSnapshot());
    }

    private static DefaultHttpContext Context(FakeLogCollector logs, string path)
    {
        var context = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging(logging => logging.AddProvider(new FakeLoggerProvider(logs))).BuildServiceProvider(),
            User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("UserId", "42")], "Test")),
        };
        context.Request.Method = "POST";
        context.Request.Path = path;
        return context;
    }

    private static async Task RenderActionMessage(FakeLogCollector logs, string label, string alertType)
    {
        var services = new ServiceCollection()
            .AddLogging(logging => logging.AddProvider(new FakeLoggerProvider(logs)))
            .AddSingleton<IStringLocalizer<Strings>, KeyLocalizer>()
            .BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<ActionMessage>(ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(ActionMessage.Label)] = label,
            [nameof(ActionMessage.AlertType)] = alertType,
        })));
    }

    private sealed class KeyLocalizer : IStringLocalizer<Strings>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => new(name, name);
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}