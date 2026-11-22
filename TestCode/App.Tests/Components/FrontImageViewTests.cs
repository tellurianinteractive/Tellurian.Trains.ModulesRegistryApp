using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ModulesRegistry.Components;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.AppTests.Components;

/// <summary>
/// <see cref="FrontImageView"/> shows the caption and the meeting's facts as one line in the same font, in the reader's language.
/// </summary>
[TestClass]
public sealed class FrontImageViewTests
{
    private DirectoryInfo _folder = null!;

    [TestInitialize]
    public void CreateTemporaryDirectory()
    {
        _folder = Directory.CreateTempSubdirectory("FrontImageViewTests");
        File.WriteAllBytes(Path.Combine(_folder.FullName, "front1.jpg"),
            [0xFF, 0xD8, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x02, 0x63, 0x07, 0x80, 0x03]);
        File.WriteAllText(Path.Combine(_folder.FullName, "front.md"), "*Module meetings are social events.*");
        File.WriteAllText(Path.Combine(_folder.FullName, "front.sv.md"), "*Modulträffar är sociala evenemang.*");
    }

    [TestCleanup]
    public void DeleteTemporaryDirectory() =>
        _folder.Delete(recursive: true);

    [TestMethod]
    public async Task CaptionAndFactsAreOneLine()
    {
        WriteMeeting("""{ "meetingDate": "2022-03-15", "meetingName": "Grimslöv 2022", "place": "Grimslöv", "country": "Sweden" }""");

        var html = await RenderAsync("sv-SE");

        StringAssert.Contains(html, "<p><em>Modulträffar är sociala evenemang.</em> <em>Grimslöv, Sverige, 2022-03-15.</em></p>");
        StringAssert.Contains(html, "src=\"images/front/front1.jpg\"");
        StringAssert.Contains(html, "width=\"1920\" height=\"611\"");
    }

    [TestMethod]
    [DataRow("en", "Sweden, 2022-03-15.")]
    [DataRow("en-US", "Sweden, 2022-03-15.")]
    [DataRow("en-GB", "Sweden, 2022-03-15.")]
    [DataRow("de-DE", "Schweden, 2022-03-15.")]
    [DataRow("da-DK", "Sverige, 2022-03-15.")]
    [DataRow("nb-NO", "Sverige, 2022-03-15.")]
    public async Task MeetingDateIsIsoInAllLanguages(string culture, string expected)
    {
        WriteMeeting("""{ "meetingDate": "2022-03-15", "meetingName": "Grimslöv 2022", "place": "Grimslöv", "country": "Sweden" }""");

        var html = await RenderAsync(culture);

        StringAssert.Contains(html, $"Grimslöv, {expected}</em>");
    }

    [TestMethod]
    public async Task LinkAndCreditAreOnTheSameLine()
    {
        WriteMeeting("""{ "meetingDate": "2022-03-15", "meetingName": "Grimslöv 2022", "place": "Grimslöv", "country": "Sweden", "link": "https://example.com/", "credit": "A photographer" }""");

        var html = await RenderAsync("sv-SE");

        StringAssert.Contains(html, "<em>Modulträffar är sociala evenemang.</em> <em><a href=\"https://example.com/\" target=\"_blank\" rel=\"noopener\" title=\"Grimslöv 2022\">Grimslöv</a>, Sverige, 2022-03-15. Foto: A photographer</em></p>");
    }

    private void WriteMeeting(string json) =>
        File.WriteAllText(Path.Combine(_folder.FullName, "front1.json"), json);

    private async Task<string> RenderAsync(string culture)
    {
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = new CultureInfo(culture);
        var services = new ServiceCollection();
        services.AddLogging().AddLocalization();
        services.AddSingleton(new FrontImageService(_folder.FullName, TimeProvider.System));
        await using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, NullLoggerFactory.Instance);
        return await renderer.Dispatcher.InvokeAsync(async () =>
            System.Net.WebUtility.HtmlDecode((await renderer.RenderComponentAsync<FrontImageView>(ParameterView.Empty)).ToHtmlString()));
    }
}