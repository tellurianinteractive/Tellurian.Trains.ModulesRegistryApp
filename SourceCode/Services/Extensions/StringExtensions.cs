using Markdig;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Services.Extensions;

public static partial class StringExtensions
{
    internal static string Localized(this string? text) =>
        string.IsNullOrWhiteSpace(text) ? string.Empty :
        Resources.Strings.ResourceManager.GetString(text) ?? text;

    public static string? Max(this string? me, int max) =>
        me is null ? null : me.Length < max ? me : me[0..max];



    public static string HtmlFromMarkdown(this string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;
        return Markdown.ToHtml(markdown, Pipeline());

        static MarkdownPipeline Pipeline() => 
            new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public static string? ValueOrNull(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    [GeneratedRegex(@"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$")]
    private static partial Regex EmailRegex();

    public static bool IsEmailAddress([NotNullWhen(true)] this string? me) =>
        me.HasValue() && EmailRegex().IsMatch(me);

    public static bool IsPermittedFileExtension(this string? it) =>
        it is not null && DocumentService.PermittedFileExtenstions.Contains(it.ToLowerInvariant());

    public static string LineWrapped(this string? text, int width) =>
        text is null ? string.Empty :
        text.Length <= width ? text : 
        text.WordWrap(width).ReplaceLineEndings("<br/>");

    private static string WordWrap(this string text, int width)
    {
        int pos, next;
        StringBuilder sb = new StringBuilder();
        // Parse each line of text
        for (pos = 0; pos < text.Length; pos = next)
        {
            // Find end of line
            int eol = text.IndexOf(Environment.NewLine, pos);
            if (eol == -1)
                next = eol = text.Length;
            else
                next = eol + Environment.NewLine.Length;

            // Copy this line of text, breaking into smaller lines as needed
            if (eol > pos)
            {
                do
                {
                    int len = eol - pos;
                    if (len > width)
                        len = BreakLine(text, pos, width);
                    sb.Append(text, pos, len);
                    sb.Append(Environment.NewLine);

                    // Trim whitespace following break
                    pos += len;
                    while (pos < eol && Char.IsWhiteSpace(text[pos]))
                        pos++;
                } while (eol > pos);
            }
            else sb.Append(Environment.NewLine); // Empty line
        }
        return sb.ToString();
    }

    private static int BreakLine(this string text, int pos, int max)
    {
        // Find last whitespace in line
        int i = max;
        while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
            i--;
        // If no whitespace found, break at maximum length
        if (i < 0)
            return max;
        // Find start of whitespace
        while (i >= 0 && Char.IsWhiteSpace(text[pos + i]))
            i--;
        // Return length of text before whitespace
        return i + 1;
    }



    #region GUID string

    public static Guid? AsGuid(this string? me)
    {
        if (string.IsNullOrWhiteSpace(me)) return null;
        if (Guid.TryParse(me, out var guid)) return guid;
        return null;
    }

    public static Guid AsGuidOrNew(this string? me)
    {
        var guid = me.AsGuid();
        return guid is null ? Guid.NewGuid() : guid.Value;
    }

    #endregion

    #region Password hashing

    public readonly record struct PasswordVerification(bool Matched, bool NeedsRehash);

    private const string V2Prefix = "v2$";
    private const int V2IterationCount = 600_000;
    private const int V1IterationCount = 10_000;
    private const int SaltBytes = 16;
    private const int HashBytes = 32;

    public static string AsHashedPassword(this string clearTextPassword)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = HashV2(clearTextPassword, salt);
        return $"{V2Prefix}{Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public static PasswordVerification VerifyPassword(this string clearTextPassword, string? storedHash)
    {
        if (string.IsNullOrWhiteSpace(clearTextPassword) || string.IsNullOrWhiteSpace(storedHash)) return default;

        if (storedHash.StartsWith(V2Prefix, StringComparison.Ordinal))
        {
            if (!TryParse(storedHash.AsSpan(V2Prefix.Length), '$', out var salt, out var expected)) return default;
            var computed = HashV2(clearTextPassword, salt);
            return CryptographicOperations.FixedTimeEquals(computed, expected)
                ? new PasswordVerification(Matched: true, NeedsRehash: false)
                : default;
        }
        else
        {
            // Legacy v1 format: "<b64salt> <b64hash>" (PBKDF2-HMAC-SHA1, 10k iterations). Rehash on successful verify.
            if (!TryParse(storedHash, ' ', out var salt, out var expected)) return default;
            var computed = HashV1(clearTextPassword, salt);
            return CryptographicOperations.FixedTimeEquals(computed, expected)
                ? new PasswordVerification(Matched: true, NeedsRehash: true)
                : default;
        }
    }

    private static bool TryParse(ReadOnlySpan<char> payload, char delimiter, out byte[] salt, out byte[] hash)
    {
        salt = []; hash = [];
        var delimIndex = payload.IndexOf(delimiter);
        if (delimIndex < 0) return false;
        try
        {
            salt = Convert.FromBase64String(payload[..delimIndex].ToString());
            hash = Convert.FromBase64String(payload[(delimIndex + 1)..].ToString());
        }
        catch (FormatException)
        {
            return false;
        }
        return salt.Length == SaltBytes && hash.Length == HashBytes;
    }

    private static byte[] HashV2(string clearTextPassword, byte[] salt) =>
        KeyDerivation.Pbkdf2(
            password: clearTextPassword,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: V2IterationCount,
            numBytesRequested: HashBytes);

    private static byte[] HashV1(string clearTextPassword, byte[] salt) =>
        KeyDerivation.Pbkdf2(
            password: clearTextPassword,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: V1IterationCount,
            numBytesRequested: HashBytes);

    #endregion

}
