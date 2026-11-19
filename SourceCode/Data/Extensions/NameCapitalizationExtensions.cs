namespace ModulesRegistry.Data.Extensions;

/// <summary>How a name is capitalised (<see cref="NameCapitalizationExtensions"/>): the kind of name decides.</summary>
public enum NameStyle
{
    /// <summary>
    /// Only the first letter of the name (and "IJ" for a Dutch "ij"); for names that may contain abbreviations and numbers, such as
    /// stations, signatures, modules and freight customers.
    /// </summary>
    FirstLetter,
    /// <summary>Every word, except connecting words such as "och", "am" and "nad" after the first word; for places and regions.</summary>
    AllWords,
    /// <summary>Every word, also after a hyphen, without exceptions; for a person's first name.</summary>
    GivenName,
    /// <summary>
    /// Every word, also after a hyphen, except name particles, also first: "von", "af" and the like are always lower case;
    /// particles written differently by country ("van", "de", ...: "van Persie" but Belgian "De Bruyne", Swedish "De Geer") are
    /// kept as typed, as is an elision such as "d'" ("d'Artagnan", "D'Alessandro"). For a person's middle and last name.
    /// </summary>
    FamilyName,
    /// <summary>
    /// As <see cref="AllWords"/>, but legal forms are written as they should be ("GmbH", "e.V.", "Sp. z o.o.", "S.p.A."), and in a
    /// name in capitals only, short words (up to five letters, such as "MEC" or "FREMO") are kept as abbreviations; for groups,
    /// clubs, companies and operators.
    /// </summary>
    Organisation,
}

/// <summary>
/// The capitalisation rules for names, in one place (decided 2026-11-18, Migration step 3.4; languages of the 17 countries the
/// app is used in, 2026-11-19): names are corrected with <see cref="AsCapitalizedName"/> when saved, and the remaining validators
/// accept a name only when the correction wouldn't change it (<see cref="IsCapitalizedName"/>). A name written in capitals only is
/// first made lower case (except with <see cref="NameStyle.FirstLetter"/>, where it may be an abbreviation, and short words with
/// <see cref="NameStyle.Organisation"/>); otherwise first letters change, and capitals inside a word are kept only in a name form,
/// such as "McDonald" or "O'Brien": a jumbled word such as "SÖdeR" becomes "Söder". A word in capitals only in a name that isn't
/// is kept, as an abbreviation. Also: elisions ("Cortina d'Ampezzo", "Pont-l'Évêque", "L'Aquila"), "IJ" in Dutch ("IJmuiden"),
/// "Mc" and "O'" ("McDonald", "O'Brien"). The language isn't needed: the lists are shared by all languages, with words that
/// conflict between languages left out (such as "by": English "Stoke-by-Nayland" but Danish "Den Gamle By").
/// </summary>
public static partial class NameCapitalizationExtensions
{
    extension(string? name)
    {
        public string? AsCapitalizedName(NameStyle style = NameStyle.AllWords)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (style == NameStyle.FirstLetter) return WithFirstCapital(name);

            var text = !IsAllUpperCase(name) ? name :
                style == NameStyle.Organisation ? LongCapitalWord().Replace(name, word => word.Value.ToLowerInvariant()) :
                name.ToLowerInvariant();
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
        if (style == NameStyle.Organisation && LegalForms.TryGetValue(word, out var spellings)) return spellings.Contains(word, StringComparer.Ordinal) ? word : spellings[0];
        if (style == NameStyle.FamilyName && VariableNameParticles.Contains(word)) return word;
        var isException = style switch
        {
            NameStyle.AllWords or NameStyle.Organisation => !isFirstWord && ConnectingWords.Contains(word),
            NameStyle.FamilyName => NameParticles.Contains(word),
            _ => false,
        };
        if (isException) return word.ToLowerInvariant();
        // Dutch "'s" and "'t" are lower case: "'s-Hertogenbosch", "'t Harde".
        if (word.Length == 2 && word[0] is '\'' or '’' && word[1] is 's' or 'S' or 't' or 'T') return word.ToLowerInvariant();
        // An abbreviation in an organisation's name, such as "KdMJK" or "MjBV", is kept as typed.
        if (style == NameStyle.Organisation && word.Count(char.IsUpper) >= 2) return word;

        var apostrophe = word.IndexOfAny(['\'', '’']);
        if (apostrophe > 0 && apostrophe < word.Length - 1)
        {
            var prefix = word[..apostrophe].ToLowerInvariant();
            var rest = WordCapitalized(word[(apostrophe + 1)..]);
            if (prefix == "o") return "O" + word[apostrophe] + rest;
            if (Elisions.Contains(prefix))
            {
                // "d'Artagnan" or "D'Alessandro" as typed; "Cortina d'Ampezzo", but "L'Aquila" as the first word or in a given name.
                var elision = style == NameStyle.FamilyName ? word[..apostrophe] :
                    isFirstWord || style == NameStyle.GivenName ? WithFirstCapital(prefix) : prefix;
                return elision + word[apostrophe] + rest;
            }
        }
        if (word.Length > 2 && word.StartsWith("mc", StringComparison.OrdinalIgnoreCase) && char.IsLetter(word[2])) return "Mc" + WordCapitalized(word[2..]);
        return WordCapitalized(word);
    }

    /// <summary>
    /// Each run of letters in the word gets its capital ("Horten/Jæren", a name in quotation marks); capitals inside a run are kept
    /// only in a name form ("McDonald", "DiCaprio"), jumbled ones ("SÖdeR") are not. A colon belongs to the run, as in "S:t".
    /// </summary>
    private static string WordCapitalized(string word) =>
        LetterRun().Replace(word, run => WithFirstCapital(run.Value.Any(char.IsLower) && !NameForm().IsMatch(run.Value) ? run.Value.ToLowerInvariant() : run.Value));

    [System.Text.RegularExpressions.GeneratedRegex(@"\p{L}[\p{L}'’:]*")]
    private static partial System.Text.RegularExpressions.Regex LetterRun();

    /// <summary>The first letter as a capital, and a Dutch "ij" at the start as "IJ" ("IJmuiden", "IJsselstein").</summary>
    private static string WithFirstCapital(string text)
    {
        if (text.Length >= 2 && text[0] is 'i' or 'I' && text[1] is 'j' or 'J' && (text.Length == 2 || !char.IsUpper(text[2]))) return "IJ" + text[2..];
        return char.IsLower(text[0]) ? char.ToUpperInvariant(text[0]) + text[1..] : text;
    }

    /// <summary>
    /// A word in lower case after its first letter, or, beginning with a capital, in parts that each begin with one capital:
    /// "McDonald", "O'Brien".
    /// </summary>
    [System.Text.RegularExpressions.GeneratedRegex(@"^(\p{L}[\p{Ll}'’]*|\p{Lu}[\p{Ll}'’]*(\p{Lu}\p{Ll}[\p{Ll}'’]*)+)$")]
    private static partial System.Text.RegularExpressions.Regex NameForm();

    /// <summary>A word of more than five capitals: in an organisation's name in capitals only, not an abbreviation.</summary>
    [System.Text.RegularExpressions.GeneratedRegex(@"\p{Lu}{6,}")]
    private static partial System.Text.RegularExpressions.Regex LongCapitalWord();

    /// <summary>French and Italian elisions before a vowel: "d'Ampezzo", "l'Évêque", "nell'Emilia".</summary>
    private static readonly HashSet<string> Elisions = new(["d", "l", "dell", "nell", "dall", "sull", "all"], StringComparer.OrdinalIgnoreCase);

    /// <summary>Legal forms of organisations in the app's countries, with their accepted spellings, the first being the correction.</summary>
    private static readonly Dictionary<string, string[]> LegalForms = new(StringComparer.OrdinalIgnoreCase)
    {
        // German, Austrian, Swiss
        ["GmbH"] = ["GmbH"], ["AG"] = ["AG"], ["KG"] = ["KG"], ["OHG"] = ["OHG"], ["e.V."] = ["e.V."], ["Co."] = ["Co."],
        // Nordic
        ["AB"] = ["AB", "Ab"], ["A/S"] = ["A/S"], ["ApS"] = ["ApS"], ["AS"] = ["AS"], ["ASA"] = ["ASA"], ["Oy"] = ["Oy"], ["Oyj"] = ["Oyj"],
        // Czech, Slovak, Polish, Hungarian
        ["s.r.o."] = ["s.r.o."], ["a.s."] = ["a.s."], ["Sp."] = ["Sp."], ["o.o."] = ["o.o."], ["Kft."] = ["Kft."], ["Zrt."] = ["Zrt."],
        // Italian, French, Belgian, Luxembourgish, Swiss
        ["S.p.A."] = ["S.p.A."], ["S.r.l."] = ["S.r.l."], ["SA"] = ["SA"], ["SARL"] = ["SARL"], ["Sàrl"] = ["Sàrl"],
        // Dutch, Belgian
        ["BV"] = ["BV"], ["B.V."] = ["B.V."], ["NV"] = ["NV"], ["N.V."] = ["N.V."],
        // British
        ["Ltd"] = ["Ltd"], ["Ltd."] = ["Ltd."], ["plc"] = ["plc", "PLC"],
    };

    private static bool IsAllUpperCase(string text) => text.Count(char.IsLetter) > 1 && !text.Any(char.IsLower);

    /// <summary>
    /// Connecting words in place and group names, lower case except as the first word: the earlier validator's list and words used
    /// so in European place names, such as "Frankfurt an der Oder", "Bergen op Zoom", "Boulogne-sur-Mer", "Castiglione del Lago",
    /// "Stratford-upon-Avon" and "Ústí nad Labem".
    /// </summary>
    private static readonly HashSet<string> ConnectingWords = new(
    [
        // Earlier list
        "i", "af", "am", "an", "in", "im", "auf", "och", "und", "van", "von", // not "by": Danish "Den Gamle By"
        // Scandinavian
        "og", "på", "vid", "ved", "av", "över",
        // German
        "der", "die", "das", "dem", "den", "des", "bei", "ob", "vor", "zum", "zur", "unter",
        // Dutch
        "aan", "op", "het",
        // French
        "de", "du", "la", "le", "les", "sur", "sous", "en", "et", "aux", "lès",
        // Italian, Spanish, Portuguese
        "di", "del", "della", "delle", "dei", "al", "sul", "sulla", "e", "y", "da", "do", "dos", "las", "los",
        // English
        "and", "of", "the", "on", "upon", "under", "next", "super",
        // Czech, Slovak, Polish, Slovene
        "nad", "pod", "na", "u", "pri", "při", "przy", "w", "z", "v",
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