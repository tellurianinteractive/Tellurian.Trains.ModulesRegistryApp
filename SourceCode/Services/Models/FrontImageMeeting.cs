namespace ModulesRegistry.Services.Models;

/// <summary>
/// The facts about the meeting a front page photo is from, read from the photo's <c>{name}.json</c> file.
/// </summary>
/// <remarks>
/// <see cref="MeetingName"/> and <see cref="Place"/> are proper names, shown as written in all languages.
/// <see cref="Country"/> is the country's text key, as <c>Country.EnglishName</c> (for example <c>Sweden</c> or <c>CzechRepublic</c>),
/// so its name is shown in the reader's language.
/// </remarks>
public sealed record FrontImageMeeting
{
    public required DateOnly MeetingDate { get; init; }
    public required string MeetingName { get; init; }
    public required string Place { get; init; }
    public required string Country { get; init; }
    /// <summary>An optional web address with more about the meeting.</summary>
    public string? Link { get; init; }
    /// <summary>An optional name of who took the photo.</summary>
    public string? Credit { get; init; }
    /// <summary>False keeps the photo out of the rotation without removing it.</summary>
    public bool Display { get; init; } = true;

    /// <summary>The <see cref="Link"/> when it is an absolute web address (http or https), otherwise null.</summary>
    public Uri? WebLink =>
        Uri.TryCreate(Link, UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp) ? uri : null;
}