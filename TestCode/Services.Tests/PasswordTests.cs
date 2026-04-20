using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Models;
using System;
using System.Security.Cryptography;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class PasswordTests
{
    [TestMethod]
    public void IsValidPasswords()
    {
        var policy = new PasswordPolicy();
        Assert.IsTrue(policy.IsValid("Fremo2?!wW"));
    }

    [TestMethod]
    public void CreatePasswordHash()
    {
        const string Password = "secret_password";
        var hashedPassword = Password.AsHashedPassword();
        Assert.IsNotNull(hashedPassword);
        Assert.IsTrue(hashedPassword.StartsWith("v2$", StringComparison.Ordinal));
    }

    [TestMethod]
    public void VerifyPassword_V2RoundTrip_MatchesAndDoesNotNeedRehash()
    {
        const string Password = "secret_password";
        var stored = Password.AsHashedPassword();

        var result = Password.VerifyPassword(stored);

        Assert.IsTrue(result.Matched);
        Assert.IsFalse(result.NeedsRehash);
    }

    [TestMethod]
    public void VerifyPassword_V2WrongPassword_DoesNotMatch()
    {
        var stored = "secret_password".AsHashedPassword();

        var result = "wrong_password".VerifyPassword(stored);

        Assert.IsFalse(result.Matched);
        Assert.IsFalse(result.NeedsRehash);
    }

    [TestMethod]
    public void VerifyPassword_LegacyV1Hash_MatchesAndRequestsRehash()
    {
        const string Password = "legacy_user_password";
        var legacyHash = CreateLegacyV1Hash(Password);

        var result = Password.VerifyPassword(legacyHash);

        Assert.IsTrue(result.Matched);
        Assert.IsTrue(result.NeedsRehash);
    }

    [TestMethod]
    public void VerifyPassword_LegacyV1WrongPassword_DoesNotMatch()
    {
        var legacyHash = CreateLegacyV1Hash("legacy_user_password");

        var result = "wrong_password".VerifyPassword(legacyHash);

        Assert.IsFalse(result.Matched);
    }

    [TestMethod]
    [DataRow(null, DisplayName = "null stored hash")]
    [DataRow("", DisplayName = "empty stored hash")]
    [DataRow("   ", DisplayName = "whitespace stored hash")]
    [DataRow("not_base64_and_no_delimiter", DisplayName = "legacy hash missing delimiter")]
    [DataRow("v2$onlyonepart", DisplayName = "v2 hash missing delimiter")]
    [DataRow("v2$!!!invalid!!!$###notbase64###", DisplayName = "v2 hash invalid base64")]
    public void VerifyPassword_MalformedStoredHash_DoesNotMatch(string? storedHash)
    {
        var result = "anything".VerifyPassword(storedHash);

        Assert.IsFalse(result.Matched);
        Assert.IsFalse(result.NeedsRehash);
    }

    [TestMethod]
    public void VerifyPassword_EmptyClearText_DoesNotMatch()
    {
        var stored = "secret_password".AsHashedPassword();

        Assert.IsFalse("".VerifyPassword(stored).Matched);
    }

    private static string CreateLegacyV1Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10_000,
            numBytesRequested: 32);
        return $"{Convert.ToBase64String(salt)} {Convert.ToBase64String(hash)}";
    }
}
