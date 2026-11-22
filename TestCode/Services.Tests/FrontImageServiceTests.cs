using Microsoft.Extensions.Time.Testing;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public sealed class FrontImageServiceTests
{
    private static readonly DateOnly Today = new(2026, 11, 22);
    private DirectoryInfo _folder = null!;

    [TestInitialize]
    public void CreateTemporaryDirectory() =>
        _folder = Directory.CreateTempSubdirectory("FrontImageServiceTests");

    [TestCleanup]
    public void DeleteTemporaryDirectory() =>
        _folder.Delete(recursive: true);

    [TestMethod]
    public void ReadsMeetingSizeAndCaptions()
    {
        CreateImage("front2.jpg", 1920, 437);
        CreateFile("front2.json", """
            {
                "meetingDate": "2023-06-10",
                "meetingName": "Milevsko 2023",
                "place": "Milevsko",
                "country": "CzechRepublic",
                "link": "https://example.com/milevsko",
                "credit": "A photographer",
                "display": true
            }
            """);
        CreateFile("front2.md", "*Module meetings*");
        CreateFile("front2.sv.md", "*Modulträffar*");

        var image = CreateTarget().Images.Single();

        Assert.AreEqual("front2.jpg", image.FileName);
        Assert.AreEqual(1920, image.Width);
        Assert.AreEqual(437, image.Height);
        Assert.AreEqual(new DateOnly(2023, 6, 10), image.Meeting.MeetingDate);
        Assert.AreEqual("Milevsko 2023", image.Meeting.MeetingName);
        Assert.AreEqual("Milevsko", image.Meeting.Place);
        Assert.AreEqual("CzechRepublic", image.Meeting.Country);
        Assert.AreEqual(new Uri("https://example.com/milevsko"), image.Meeting.WebLink);
        Assert.AreEqual("A photographer", image.Meeting.Credit);
        Assert.IsTrue(image.Meeting.Display);
        StringAssert.Contains(image.CaptionHtml("en"), "<em>Module meetings</em>");
        StringAssert.Contains(image.CaptionHtml("sv"), "<em>Modulträffar</em>");
    }

    [TestMethod]
    public void OptionalPropertiesCanBeLeftOut()
    {
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));
        CreateFile("front2.md", "English");

        var meeting = CreateTarget().Images.Single().Meeting;

        Assert.IsNull(meeting.Link);
        Assert.IsNull(meeting.Credit);
        Assert.IsTrue(meeting.Display, "Displayed unless said otherwise.");
    }

    [TestMethod]
    public void MissingTranslationIsEnglish()
    {
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));
        CreateFile("front2.md", "English");

        var image = CreateTarget().Images.Single();

        Assert.AreEqual(image.CaptionHtml(string.Empty), image.CaptionHtml("de"));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("{ \"meetingDate\": \"2023-06-10\" }")]
    [DataRow("{ \"meetingDate\": \"2023\", \"meetingName\": \"M\", \"place\": \"P\", \"country\": \"Sweden\" }")]
    [DataRow("not json")]
    [DataRow(null)]
    public void ImageWithoutValidMeetingIsNotRead(string? json)
    {
        CreateImage("front2.jpg", 10, 10);
        if (json is not null) CreateFile("front2.json", json);
        CreateFile("front2.md", "English");

        Assert.IsEmpty(CreateTarget().Images);
    }

    [TestMethod]
    public void ImageWithoutOwnCaptionGetsSharedCaption()
    {
        CreateSharedCaption();
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));

        var image = CreateTarget().Images.Single();

        StringAssert.Contains(image.CaptionHtml(string.Empty), "Shared");
        StringAssert.Contains(image.CaptionHtml("sv"), "Gemensam");
        StringAssert.Contains(image.CaptionHtml("de"), "Shared");
    }

    [TestMethod]
    public void OwnCaptionReplacesSharedCaption()
    {
        CreateSharedCaption();
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));
        CreateFile("front2.md", "Own");
        CreateFile("front2.sv.md", "Egen");

        var image = CreateTarget().Images.Single();

        StringAssert.Contains(image.CaptionHtml(string.Empty), "Own");
        StringAssert.Contains(image.CaptionHtml("sv"), "Egen");
        Assert.DoesNotContain("Gemensam", image.CaptionHtml("sv"));
    }

    [TestMethod]
    public void OwnTranslationWithoutEnglishKeepsSharedCaption()
    {
        CreateSharedCaption();
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));
        CreateFile("front2.sv.md", "Egen");

        var image = CreateTarget().Images.Single();

        StringAssert.Contains(image.CaptionHtml("sv"), "Gemensam");
        StringAssert.Contains(image.CaptionHtml("de"), "Shared");
    }

    [TestMethod]
    public void OneParagraphCaptionIsInline()
    {
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));
        CreateFile("front2.md", "*Module meetings are social events.*\r\n");

        Assert.AreEqual("<em>Module meetings are social events.</em>", CreateTarget().Images.Single().CaptionHtml(string.Empty));
    }

    [TestMethod]
    public void ImageWithoutAnyCaptionHasEmptyCaption()
    {
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));

        Assert.AreEqual(string.Empty, CreateTarget().Images.Single().CaptionHtml("sv"));
    }

    [TestMethod]
    public void HiddenImageIsReadButNotDisplayed()
    {
        CreateImage("front1.jpg", 10, 10);
        CreateFile("front1.json", Meeting("2023-06-10", display: false));
        CreateFile("front1.md", "Hidden");
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));
        CreateFile("front2.md", "Shown");

        var target = CreateTarget();

        Assert.HasCount(2, target.Images);
        Assert.AreEqual("front2.jpg", target.DisplayedImages.Single().FileName);
        Assert.AreEqual("front2.jpg", target.Pick(0.0)?.FileName);
        Assert.AreEqual("front2.jpg", target.Pick(0.99)?.FileName);
    }

    [TestMethod]
    public void CaptionOfSimilarNamedImageIsNotTaken()
    {
        CreateImage("front2.jpg", 10, 10);
        CreateFile("front2.json", Meeting("2023-06-10"));
        CreateFile("front2.md", "Two");
        CreateFile("front20.md", "Twenty");

        var image = CreateTarget().Images.Single();

        StringAssert.Contains(image.CaptionHtml(string.Empty), "Two");
        Assert.DoesNotContain("Twenty", image.CaptionHtml(string.Empty));
    }

    [TestMethod]
    [DataRow("https://example.com", true)]
    [DataRow("http://example.com", true)]
    [DataRow("javascript:alert(1)", false)]
    [DataRow("example.com", false)]
    [DataRow(null, false)]
    public void OnlyWebAddressesAreLinked(string? link, bool isLinked)
    {
        var meeting = new FrontImageMeeting { MeetingDate = Today, MeetingName = "M", Place = "P", Country = "Sweden", Link = link };

        Assert.AreEqual(isLinked, meeting.WebLink is not null);
    }

    [TestMethod]
    [DataRow("2026-11-22", 1.0)]
    [DataRow("2024-11-22", 0.5)]
    [DataRow("2022-11-22", 0.25)]
    [DataRow("2010-11-22", FrontImageService.MinimumWeight)]
    [DataRow("2027-06-01", 1.0)]
    public void WeightHalvesEveryTwoYears(string meetingDate, double expected) =>
        Assert.AreEqual(expected, FrontImageService.Weight(DateOnly.Parse(meetingDate), Today), 0.01);

    [TestMethod]
    public void RecentImageIsPickedMoreOften()
    {
        CreateImage("new.jpg", 10, 10);
        CreateFile("new.json", Meeting("2026-11-01"));
        CreateFile("new.md", "New");
        CreateImage("old.jpg", 10, 10);
        CreateFile("old.json", Meeting("2010-06-01"));
        CreateFile("old.md", "Old");
        var target = CreateTarget();

        // Weights are about 1.0 (new) and 0.1 (old), so the old one gets the last tenth of the range.
        Assert.AreEqual("new.jpg", target.Pick(0.0)?.FileName);
        Assert.AreEqual("new.jpg", target.Pick(0.85)?.FileName);
        Assert.AreEqual("old.jpg", target.Pick(0.95)?.FileName);
    }

    [TestMethod]
    public void NoImagesPicksNothing() =>
        Assert.IsNull(CreateTarget().Pick(0.5));

    private static string Meeting(string meetingDate, bool display = true) =>
        $$"""{ "meetingDate": "{{meetingDate}}", "meetingName": "Meeting", "place": "Place", "country": "Sweden", "display": {{(display ? "true" : "false")}} }""";

    private void CreateSharedCaption()
    {
        CreateFile($"{FrontImageService.SharedCaptionName}.md", "Shared");
        CreateFile($"{FrontImageService.SharedCaptionName}.sv.md", "Gemensam");
    }

    private FrontImageService CreateTarget() =>
        new(_folder.FullName, new FakeTimeProvider(new DateTimeOffset(Today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)));

    private void CreateFile(string fileName, string text) =>
        File.WriteAllText(Path.Combine(_folder.FullName, fileName), text);

    // The start of a JPEG file: SOI, an APP0 segment and a baseline frame header (SOF0) with the size.
    private void CreateImage(string fileName, int width, int height) =>
        File.WriteAllBytes(Path.Combine(_folder.FullName, fileName),
        [
            0xFF, 0xD8,
            0xFF, 0xE0, 0x00, 0x04, 0x00, 0x00,
            0xFF, 0xC0, 0x00, 0x11, 0x08, (byte)(height >> 8), (byte)height, (byte)(width >> 8), (byte)width, 0x03,
        ]);
}