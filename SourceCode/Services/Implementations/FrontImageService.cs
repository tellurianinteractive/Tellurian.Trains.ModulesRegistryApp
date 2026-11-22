using System.Text.Json;

namespace ModulesRegistry.Services.Implementations;

/// <summary>
/// The front page photos (<c>wwwroot/images/front</c>), read once at startup. Each <c>{name}.jpg</c> has its files next to it:
/// <c>{name}.json</c> with the facts about the meeting (<see cref="FrontImageMeeting"/>), and optionally its own caption,
/// <c>{name}.md</c> in English and <c>{name}.{lang}.md</c> per language, which replaces the shared caption
/// (<c>front.md</c>, <c>front.{lang}.md</c>) for that photo. A photo without a valid JSON file isn't read,
/// and one with <c>"display": false</c> isn't shown.
/// </summary>
/// <remarks>
/// A random photo is picked per request, weighted by the meeting's age: the weight halves every <see cref="HalfLifeYears"/>
/// years, but never goes below <see cref="MinimumWeight"/>, so old photos still appear now and then.
/// </remarks>
public sealed class FrontImageService
{
    public const double HalfLifeYears = 2;
    public const double MinimumWeight = 0.1;
    /// <summary>The name of the caption files shown with photos that have no caption of their own.</summary>
    public const string SharedCaptionName = "front";
    private const string ImageExtension = ".jpg";
    private const string MeetingExtension = ".json";
    private const string CaptionExtension = ".md";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };
    private readonly TimeProvider _timeProvider;

    public FrontImageService(string folder, TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        Images = Read(folder);
        DisplayedImages = [.. Images.Where(image => image.Meeting.Display)];
    }

    /// <summary>All photos with valid files, also those not displayed.</summary>
    public IReadOnlyList<FrontImage> Images { get; }

    /// <summary>The photos in the rotation.</summary>
    public IReadOnlyList<FrontImage> DisplayedImages { get; }

    public FrontImage? Random() => Pick(System.Random.Shared.NextDouble());

    /// <param name="random">A number from 0 up to, but not including, 1.</param>
    public FrontImage? Pick(double random)
    {
        if (DisplayedImages.Count == 0) return null;
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var weights = DisplayedImages.Select(image => Weight(image.Meeting.MeetingDate, today)).ToArray();
        var remaining = random * weights.Sum();
        for (var i = 0; i < weights.Length; i++)
        {
            remaining -= weights[i];
            if (remaining < 0) return DisplayedImages[i];
        }
        return DisplayedImages[^1];
    }

    public static double Weight(DateOnly meetingDate, DateOnly today)
    {
        var ageInYears = Math.Max(0, today.DayNumber - meetingDate.DayNumber) / 365.25;
        return Math.Max(Math.Pow(0.5, ageInYears / HalfLifeYears), MinimumWeight);
    }

    private static List<FrontImage> Read(string folder)
    {
        if (!Directory.Exists(folder)) return [];
        var files = Directory.GetFiles(folder);
        var captionFiles = files.Where(f => f.EndsWith(CaptionExtension, StringComparison.OrdinalIgnoreCase)).ToArray();
        var sharedCaptions = ReadCaptions(captionFiles, SharedCaptionName);
        List<FrontImage> images = [];
        foreach (var imageFile in files.Where(f => f.EndsWith(ImageExtension, StringComparison.OrdinalIgnoreCase)).Order(StringComparer.OrdinalIgnoreCase))
        {
            var name = Path.GetFileNameWithoutExtension(imageFile);
            var meeting = ReadMeeting(files.FirstOrDefault(f => Path.GetFileName(f).Equals($"{name}{MeetingExtension}", StringComparison.OrdinalIgnoreCase)));
            if (meeting is null) continue;
            var ownCaptions = ReadCaptions(captionFiles, name);
            // The photo's own caption counts only with its English master, so a lone translation can't leave other languages empty.
            var captions = ownCaptions.ContainsKey(string.Empty) ? ownCaptions : sharedCaptions;
            var (width, height) = JpegSize(imageFile);
            images.Add(new(Path.GetFileName(imageFile), width, height, meeting, captions));
        }
        return images;
    }

    private static Dictionary<string, string> ReadCaptions(string[] captionFiles, string name)
    {
        Dictionary<string, string> captions = [];
        foreach (var captionFile in captionFiles)
        {
            var language = CaptionLanguage(name, Path.GetFileNameWithoutExtension(captionFile));
            if (language is null) continue;
            captions[language] = InlineHtml(File.ReadAllText(captionFile));
        }
        return captions;
    }

    // The caption is shown on one line with the meeting's facts, so a single paragraph loses its <p> wrapper.
    private static string InlineHtml(string markdown)
    {
        const string Start = "<p>";
        const string End = "</p>";
        var html = markdown.Trim().HtmlFromMarkdown().Trim();
        return html.StartsWith(Start, StringComparison.Ordinal) && html.EndsWith(End, StringComparison.Ordinal) && html.IndexOf(Start, 1, StringComparison.Ordinal) < 0
            ? html[Start.Length..^End.Length]
            : html;
    }

    // A missing or invalid file (a required property missing, a date not yyyy-MM-dd) gives null; the content test reports it.
    private static FrontImageMeeting? ReadMeeting(string? path)
    {
        if (path is null) return null;
        try
        {
            return JsonSerializer.Deserialize<FrontImageMeeting>(File.ReadAllText(path), JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    // "front2" is the English caption of the image front2 (empty code), "front2.sv" the Swedish one; null if another image's.
    private static string? CaptionLanguage(string imageName, string captionName) =>
        captionName.Equals(imageName, StringComparison.OrdinalIgnoreCase) ? string.Empty :
        captionName.StartsWith($"{imageName}.", StringComparison.OrdinalIgnoreCase) ? captionName[(imageName.Length + 1)..].ToLowerInvariant() :
        null;

    // Width and height from the JPEG frame header (a SOFn marker); zero when not found.
    private static (int Width, int Height) JpegSize(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);
        try
        {
            if (reader.ReadByte() != 0xFF || reader.ReadByte() != 0xD8) return (0, 0);
            while (true)
            {
                if (reader.ReadByte() != 0xFF) return (0, 0);
                var marker = reader.ReadByte();
                while (marker == 0xFF) marker = reader.ReadByte();
                var length = ReadUInt16(reader);
                if (marker is >= 0xC0 and <= 0xCF and not 0xC4 and not 0xC8 and not 0xCC)
                {
                    reader.ReadByte(); // Sample precision
                    var height = ReadUInt16(reader);
                    var width = ReadUInt16(reader);
                    return (width, height);
                }
                stream.Seek(length - 2, SeekOrigin.Current);
            }
        }
        catch (EndOfStreamException)
        {
            return (0, 0);
        }
    }

    private static int ReadUInt16(BinaryReader reader) => (reader.ReadByte() << 8) | reader.ReadByte();
}