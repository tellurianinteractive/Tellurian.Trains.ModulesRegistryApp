using Markdig;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ModulesRegistry.Services.Implementations;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Services.Extensions
{
    public static class StringExtensions
    {
        public static bool HasValue([NotNullWhen(true)] this string? me) => 
            !string.IsNullOrWhiteSpace(me);

        public static bool HasNoValue([NotNullWhen(false)] this string? me) => 
            string.IsNullOrWhiteSpace(me);

        public static bool IsEmailAddress([NotNullWhen(true)] this string? me) => 
            me.HasValue() && Regex.IsMatch(me, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

       public static string[] Items(this string? value, char separator = ';') =>
            string.IsNullOrWhiteSpace(value) ? Array.Empty<string>() :
            value.Trim().Split(separator);

        #region Special string conversions

        public static string HtmlFromMarkdown(this string? markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown)) return string.Empty;
            return Markdown.ToHtml(markdown, Pipeline());

            static MarkdownPipeline Pipeline()
            {
                var builder = new MarkdownPipelineBuilder();
                builder.UseAdvancedExtensions();
                return builder.Build();
            }
        }

        public static bool IsPermittedFileExtension(this string? it) =>
            it is not null && DocumentService.PermittedFileExtenstions.Contains(it.ToLowerInvariant());

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
}
