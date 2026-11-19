namespace ModulesRegistry.Data.Extensions;

/// <summary>How a name is capitalised (<see cref="NameCapitalizationExtensions"/>).</summary>
public enum NameStyle
{
    /// <summary>Only the first letter of the name; for names that may contain abbreviations, such as stations and modules.</summary>
    FirstLetter,
    /// <summary>Every word, except connecting words such as "och" and "am" after the first word; for places, regions and groups.</summary>
    AllWords,
    /// <summary>Every word, also after a hyphen, without exceptions; for a person's first name.</summary>
    GivenName,
    /// <summary>
    /// Every word, also after a hyphen, except name particles, also first: "von", "af" and the like are always lower case;
    /// particles written differently by country ("van", "de", ...: "van Persie" but Belgian "De Bruyne", Swedish "De Geer") are
    /// kept as typed. For a person's middle and last name.
    /// </summary>
    FamilyName,
}

/// <summary>
/// The capitalisation rules for names, in one place (decided 2026-11-18, Migration step 3.4): names are corrected with
/// <see cref="AsCapitalizedName"/> when saved, and the remaining validators accept a name only when the correction wouldn't change it
/// (<see cref="IsCapitalizedName"/>). A name written in capitals only is first made lower case (except with
/// <see cref="NameStyle.FirstLetter"/>, where it may be an abbreviation); otherwise first letters change, and capitals inside a
/// word are kept only in a name form, such as "McDonald" or "O'Brien": a jumbled word such as "SÖdeR" becomes "Söder". A word
/// in capitals only in a name that isn't is kept, as an abbreviation.
/// </summary>
public static partial class NameCapitalizationExtensions
{
    extension(string? name)
    {
        public string? AsCapitalizedName(NameStyle style = NameStyle.AllWords)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (style == NameStyle.FirstLetter) return char.IsLower(name[0]) ? char.ToUpperInvariant(name[0]) + name[1..] : name;

            var text = IsAllUpperCase(name) ? name.ToLowerInvariant() : name;
            var result = new System.Text.StringBuilder(text.Length);
            var isFirstWord = true;
            var start = 0;
            for (var i = 0; i <= text.Length; i++)
            {
                if (i < text.Length && text[i] is not (' ' or '-')) continue;
                if (i > start)
                {
                    result.Append(Capitalized(text[start..i], style, isFirstWord));
                    isFirstWord = false;
                }
                if (i < text.Length) result.Append(text[i]);
                start = i + 1;
            }
            return result.ToString();
        }

        /// <summary>True when <see cref="AsCapitalizedName"/> wouldn't change the name; an empty name is valid.</summary>
        public bool IsCapitalizedName(NameStyle style = NameStyle.AllWords) => name.AsCapitalizedName(style) == name;
    }

    private static string Capitalized(string word, NameStyle style, bool isFirstWord)
    {
        if (style == NameStyle.FamilyName && VariableNameParticles.Contains(word)) return word;
        var isException = style switch
        {
            NameStyle.AllWords => !isFirstWord && ConnectingWords.Contains(word),
            NameStyle.FamilyName => NameParticles.Contains(word),
            _ => false,
        };
        if (isException) return word.ToLowerInvariant();
        // Capitals inside a word are kept only in a name form ("McDonald", "O'Brien", "DiCaprio"); jumbled ones ("SÖdeR") are not.
        if (word.Any(char.IsLower) && !NameForm().IsMatch(word)) word = word.ToLowerInvariant();
        return char.IsLower(word[0]) ? char.ToUpperInvariant(word[0]) + word[1..] : word;
    }

    /// <summary>
    /// A word in lower case after its first letter, or, beginning with a capital, in parts that each begin with one capital:
    /// "McDonald", "O'Brien".
    /// </summary>
    [System.Text.RegularExpressions.GeneratedRegex(@"^(\p{L}[\p{Ll}'’]*|\p{Lu}[\p{Ll}'’]*(\p{Lu}\p{Ll}[\p{Ll}'’]*)+)$")]
    private static partial System.Text.RegularExpressions.Regex NameForm();

    private static bool IsAllUpperCase(string text) => text.Count(char.IsLetter) > 1 && !text.Any(char.IsLower);

    /// <summary>
    /// Connecting words in place and group names, lower case except as the first word: the earlier validator's list and words used
    /// so in European place names, such as "Frankfurt an der Oder", "Bergen op Zoom", "Boulogne-sur-Mer", "Castiglione del Lago",
    /// "Stratford-upon-Avon" and "Ústí nad Labem".
    /// </summary>
    private static readonly HashSet<string> ConnectingWords = new(
    [
        // Earlier list
        "i", "af", "am", "an", "by", "in", "im", "auf", "och", "und", "van", "von",
        // Scandinavian
        "og", "på", "vid", "ved", "av",
        // German
        "der", "die", "das", "dem", "den", "des", "bei", "ob", "vor", "zum", "zur",
        // Dutch
        "aan", "op", "het",
        // French
        "de", "du", "la", "le", "les", "sur", "sous", "en", "et", "aux", "lès",
        // Italian, Spanish, Portuguese
        "di", "del", "della", "delle", "dei", "al", "sul", "sulla", "e", "y", "da", "do", "dos", "las", "los",
        // English
        "and", "of", "the", "on", "upon",
        // Czech, Slovak, Polish, Slovene
        "nad", "pod", "na", "u", "pri", "při",
    ], StringComparer.OrdinalIgnoreCase);

    /// <summary>Name particles that are always lower case in middle and last names.</summary>
    private static readonly HashSet<string> NameParticles = new(["af", "av", "von", "zu", "zum", "zur", "vom"], StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Name particles kept as typed, since the custom differs by country and family; a name in capitals only gets them in lower case.
    /// </summary>
    private static readonly HashSet<string> VariableNameParticles = new(
        ["van", "de", "der", "den", "des", "ten", "ter", "op", "in", "het", "da", "di", "du", "do", "dos", "das", "del", "della", "dei", "degli", "dal", "dalla", "la", "le", "y", "e"],
        StringComparer.OrdinalIgnoreCase);
}