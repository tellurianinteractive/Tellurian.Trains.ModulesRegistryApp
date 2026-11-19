namespace ModulesRegistry.Forms.Validation;

/// <summary>
/// The text rules behind the custom validation attributes (Migration section 3.3), in one place, so the JavaScript validators
/// of step 3.5 can be written and tested against the same table of values. An empty value is valid: [Required] decides that.
/// Capitalisation of names isn't here: see NameCapitalizationExtensions in Data, which corrects names when saved.
/// </summary>
public static class TextRules
{
    extension(string? text)
    {
        /// <summary>Only Latin letters, digits and some punctuation and symbols.</summary>
        public bool IsOrdinaryText()
        {
            if (string.IsNullOrEmpty(text)) return true;
            foreach (var c in text)
            {
                if (c.IsLatinChar()) continue;
                if (c.IsDigit()) continue;
                if (c.IsPermittedPunctuationOrSymbol()) continue;
                return false;
            }
            return true;
        }
    }

    extension(char c)
    {
        private bool IsDigit() => c.IsInRange(0x0030, 0x0039);

        private bool IsPermittedPunctuationOrSymbol() => " (),.:;-+*!?%&§#±°²³/«»£€´'\"ºª".Contains(c);
        private bool IsLatinChar() =>
            c.IsInRange(0x0061, 0x007A) ||
            c.IsInRange(0x0041, 0x005A) ||
            c.IsInRange(0x00C0, 0x00FF) ||
            c.IsInRange(0x0100, 0x0148) ||
            c.IsInRange(0x014A, 0x01BF) ||
            c.IsInRange(0x1E00, 0x1E9E); // Up to the capital sharp s, ẞ.
        private bool IsInRange(int firstCodePoint, int lastCodePoint) => c >= firstCodePoint && c <= lastCodePoint;
    }
}