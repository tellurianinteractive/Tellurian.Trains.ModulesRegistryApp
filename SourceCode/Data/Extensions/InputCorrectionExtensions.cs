using System.Globalization;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Data.Extensions;

/// <summary>
/// Corrections of input, applied when an entity is saved (decided 2026-11-19, Migration step 3.5): what the app can correct is
/// corrected, so users only get a message for what it can't. Each correction keeps a value it can't make sense of unchanged
/// (apart from trimming), so the validation that follows shows the message.
/// </summary>
public static partial class InputCorrectionExtensions
{
    extension(string? text)
    {
        /// <summary>
        /// Text as typed or pasted, made ordinary: typographic quotes, dashes and ellipses become their plain equivalents, all kinds
        /// of spaces a plain space, letters with separate accent marks single letters (Unicode normalisation), zero-width and control
        /// characters are removed, and the text is trimmed. A single line also has its line breaks and repeated spaces
        /// replaced by one space; <paramref name="multiline"/> text keeps its line breaks and indentation.
        /// </summary>
        public string? AsOrdinaryText(bool multiline = false)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var normalized = text.Normalize(NormalizationForm.FormC);
            var result = new StringBuilder(normalized.Length);
            foreach (var c in normalized)
            {
                switch (c)
                {
                    case '‘' or '’' or '‚' or '‛' or '′': result.Append('\''); break;
                    case '“' or '”' or '„' or '‟' or '″': result.Append('"'); break;
                    case '‐' or '‑' or '‒' or '–' or '—' or '―' or '−': result.Append('-'); break;
                    case '…': result.Append("..."); break;
                    case '\r': if (multiline) result.Append(c); break;
                    case '\n' or '\t': result.Append(multiline ? c : ' '); break;
                    case '​' or '‌' or '‍' or '⁠' or '﻿': break;
                    default:
                        if (char.IsControl(c)) break;
                        result.Append(char.GetUnicodeCategory(c) is UnicodeCategory.SpaceSeparator ? ' ' : c);
                        break;
                }
            }
            var corrected = result.ToString();
            return multiline ? corrected.Trim() : RepeatedSpaces().Replace(corrected, " ").Trim();
        }

        /// <summary>Trimmed, or null when empty or only spaces; for optional text fields.</summary>
        public string? AsNullIfEmpty() => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        /// <summary>
        /// Email addresses separated by semicolons: any of <c>; , space</c> or a line break separates them; each is trimmed and
        /// in lower case, and empty entries and duplicates are removed.
        /// </summary>
        public string? AsEmailAddresses()
        {
            if (text is null) return null;
            return string.Join(';', text.Split([';', ',', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
                .Select(address => address.ToLowerInvariant())
                .Distinct());
        }

        /// <summary>
        /// Loco addresses as single addresses and ranges, such as <c>3,10-15</c>: <c>; ,</c> or spaces separate them, a reversed
        /// range is turned, and the addresses are sorted with duplicates removed and consecutive addresses as ranges. Text that
        /// isn't loco addresses is only trimmed.
        /// </summary>
        public string? AsLocoAddresses()
        {
            if (string.IsNullOrWhiteSpace(text)) return text?.Trim();
            var addresses = new SortedSet<int>();
            foreach (var part in AroundHyphen().Replace(text, "-").Split([',', ';', ' ', '\t', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                var ends = part.Split('-');
                if (ends.Length is 0 or > 2 || !ends.All(e => int.TryParse(e, NumberStyles.None, CultureInfo.InvariantCulture, out var a) && a.IsDccAddress())) return text.Trim();
                var numbers = ends.Select(e => int.Parse(e, CultureInfo.InvariantCulture)).ToArray();
                var (first, last) = (numbers.Min(), numbers.Max());
                for (var address = first; address <= last; address++) addresses.Add(address);
            }
            return addresses.ToArray().AsCollapsedLocoAdresses();
        }

        /// <summary>A MAC address in any common notation (<c>AABBCCDDEEFF</c>, with <c>: - .</c> or spaces) as <c>AA:BB:CC:DD:EE:FF</c>.</summary>
        public string? AsMacAddress()
        {
            if (string.IsNullOrWhiteSpace(text)) return text?.Trim();
            var digits = MacSeparators().Replace(text, "");
            if (digits.Length != 12 || !digits.All(char.IsAsciiHexDigit)) return text.Trim();
            return string.Join(':', digits.ToUpperInvariant().Chunk(2).Select(pair => new string(pair)));
        }

        /// <summary>A colour as <c>#RRGGBB</c> in capitals: a missing <c>#</c> is added and the short form <c>#RGB</c> expanded.</summary>
        public string? AsHexColor()
        {
            if (string.IsNullOrWhiteSpace(text)) return text?.Trim();
            var digits = text.Trim().TrimStart('#');
            if (!digits.All(char.IsAsciiHexDigit) || digits.Length is not (3 or 6)) return text.Trim();
            if (digits.Length == 3) digits = string.Concat(digits.Select(d => $"{d}{d}"));
            return "#" + digits.ToUpperInvariant();
        }
    }

    extension<T>(T? value) where T : struct, INumber<T>
    {
        /// <summary>Null for zero, which in an optional number field means "not given".</summary>
        public T? AsNullIfZero() => value is { } number && T.IsZero(number) ? null : value;
    }

    extension(int address)
    {
        private bool IsDccAddress() => address is >= 1 and <= 9999;
    }

    [GeneratedRegex(" {2,}")]
    private static partial Regex RepeatedSpaces();

    [GeneratedRegex(@"\s*-\s*")]
    private static partial Regex AroundHyphen();

    [GeneratedRegex(@"[\s:.\-]")]
    private static partial Regex MacSeparators();
}