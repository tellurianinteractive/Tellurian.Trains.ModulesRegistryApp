namespace ModulesRegistry.Services.Models;

/// <summary>
/// A front page photo from a module meeting: the facts about the meeting (<c>{name}.json</c>) and the caption as HTML
/// per two-letter language code, the photo's own (<c>{name}.{lang}.md</c>) or else the shared one (<c>front.{lang}.md</c>);
/// the empty code is the English master caption.
/// </summary>
public sealed record FrontImage(string FileName, int Width, int Height, FrontImageMeeting Meeting, IReadOnlyDictionary<string, string> Captions)
{
    /// <summary>The caption in the language, otherwise in English.</summary>
    public string CaptionHtml(string language) =>
        Captions.TryGetValue(language, out var caption) ? caption : Captions.GetValueOrDefault(string.Empty, string.Empty);
}