using Azure.Core;
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


    public static string[] Items(this string? value, char separator = ';') =>
         string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() :
         value.Trim().Split(separator);

    public static string HtmlFromMarkdown(this string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;
        return Markdown.ToHtml(markdown, Pipeline());

        static MarkdownPipeline Pipeline() => 
            new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    internal static string? ValueOrNull(this string? value) =>
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

    public static string AsHashedPassword(this string clearTextPassword)
    {
        byte[] salt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return Hash(clearTextPassword, salt);
    }

    private static string Hash(string clearTextPassword, byte[] salt)
    {
        byte[] hashed = KeyDerivation.Pbkdf2(
            password: clearTextPassword,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8);
        return $"{Convert.ToBase64String(salt)} {Convert.ToBase64String(hashed)}";
    }

    public static bool IsSamePasswordAs(this string clearTextPassword, string? hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword)) return false;
        var hashedPasswordItems = hashedPassword.Items(' ');
        if (hashedPasswordItems.Length != 2) return false;
        var salt = Convert.FromBase64String(hashedPasswordItems[0]);
        var hashedClearTextPassword = Hash(clearTextPassword, salt);
        return hashedClearTextPassword.Equals(hashedPassword, StringComparison.Ordinal);
    }

    #endregion

}
