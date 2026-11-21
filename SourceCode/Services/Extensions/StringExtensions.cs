using ModulesRegistry.Resources;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using ModulesRegistry.Services.Implementations;
using System.Security.Cryptography;

namespace ModulesRegistry.Services.Extensions;

public static class StringExtensions
{
    internal static string Localized(this string? text) =>
        text.IsEmpty ? string.Empty :
        AppTexts.Find(text) ?? text;

    public static bool IsPermittedFileExtension(this string? it) =>
        it is not null && DocumentService.PermittedFileExtenstions.Contains(it.ToLowerInvariant());

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
        if (clearTextPassword.IsEmpty || storedHash.IsEmpty) return default;

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
            // Expected, not logged: a stored value that isn't a hash in this format.
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
