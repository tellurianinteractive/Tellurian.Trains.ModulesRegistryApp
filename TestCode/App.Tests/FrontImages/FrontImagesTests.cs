using ModulesRegistry.Resources;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.AppTests.FrontImages;

/// <summary>
/// The front page photos in <c>wwwroot/images/front</c> (copied to the test output). A photo with a JSON file is meant to be
/// read: its meeting facts must be valid and its file small. Each caption, the shared one and a photo's own, is in all
/// fully supported languages. A photo without a JSON file isn't checked, so photos can be added before their facts.
/// </summary>
[TestClass]
public sealed class FrontImagesTests
{
    private const long MaxFileSize = 200 * 1024;
    private static readonly string Folder = Path.Combine(AppContext.BaseDirectory, "FrontImages");
    private static readonly FrontImageService Target = new(Folder, TimeProvider.System);

    [TestMethod]
    public void AtLeastOneImageIsDisplayed() =>
        Assert.IsNotEmpty(Target.DisplayedImages);

    [TestMethod]
    public void EachJsonFileBelongsToAReadImage()
    {
        var read = Target.Images.Select(i => Path.GetFileNameWithoutExtension(i.FileName)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var json in Directory.GetFiles(Folder, "*.json"))
        {
            var name = Path.GetFileNameWithoutExtension(json);
            Assert.Contains(name, read,
                $"{name}: needs {name}.jpg and valid JSON with meetingDate (yyyy-MM-dd), meetingName, place and country.");
        }
    }

    [TestMethod]
    public void EachImageHasMeetingFacts()
    {
        foreach (var image in Target.Images)
        {
            var meeting = image.Meeting;
            Assert.IsTrue(meeting.MeetingName.HasValue, $"{image.FileName}: meetingName");
            Assert.IsTrue(meeting.Place.HasValue, $"{image.FileName}: place");
            Assert.IsNotNull(AppTexts.Find(meeting.Country), $"{image.FileName}: country '{meeting.Country}' must be a country text key, such as Sweden or CzechRepublic.");
            Assert.IsTrue(meeting.Link is null || meeting.WebLink is not null, $"{image.FileName}: link must be an http or https address.");
            Assert.IsGreaterThan(0, image.Width, image.FileName);
            Assert.IsGreaterThan(0, image.Height, image.FileName);
        }
    }

    [TestMethod]
    public void SharedCaptionExists() =>
        Assert.IsTrue(File.Exists(Path.Combine(Folder, $"{FrontImageService.SharedCaptionName}.md")), $"The shared caption {FrontImageService.SharedCaptionName}.md is missing.");

    [TestMethod]
    public void EachCaptionIsInAllFullySupportedLanguages()
    {
        // Grouped by the name before the first dot: "front2.sv.md" is a caption of front2; "front" is the shared caption.
        foreach (var caption in Directory.GetFiles(Folder, "*.md").GroupBy(f => Path.GetFileName(f).Split('.')[0], StringComparer.OrdinalIgnoreCase))
        {
            var languages = caption.Select(f => Path.GetFileName(f).Split('.')).Select(parts => parts.Length == 2 ? "en" : parts[1]).ToArray();
            CollectionAssert.IsSubsetOf(LanguageExtensions.FullySupportedLanguages, languages, $"{caption.Key}: a caption needs {caption.Key}.md and a translation per language.");
            Assert.IsTrue(caption.All(f => File.ReadAllText(f).Trim().HasValue), $"{caption.Key}: a caption file is empty.");
            if (!caption.Key.Equals(FrontImageService.SharedCaptionName, StringComparison.OrdinalIgnoreCase))
                Assert.IsTrue(File.Exists(Path.Combine(Folder, $"{caption.Key}.jpg")), $"{caption.Key}: a caption without a photo.");
        }
    }

    [TestMethod]
    public void EachImageIsSmall()
    {
        foreach (var image in Target.Images)
        {
            var size = new FileInfo(Path.Combine(Folder, image.FileName)).Length;
            Assert.IsLessThanOrEqualTo(MaxFileSize, size, $"{image.FileName} is {size / 1024} KB; resize and compress it.");
        }
    }
}