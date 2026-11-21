using Microsoft.AspNetCore.DataProtection;
using System.Globalization;
using System.Security.Cryptography;

namespace ModulesRegistry.Security;

/// <summary>What an invitation link says: who is invited, to which address, and whether the user will be read-only.</summary>
public sealed record InvitationClaim(int PersonId, string EmailAddress, bool IsReadOnly);

/// <summary>
/// Tokens in invitation links (decided 2026-11-21): the user account is created when the invited person sets a password, not when the
/// invitation is sent, so the link carries the invitation itself, signed with ASP.NET Core Data Protection. The key ring survives restarts
/// and deploys on App Service (<c>%HOME%\ASP.NET\DataProtection-Keys</c>), as for <see cref="Endpoints.PublicDocumentKeys"/>.
/// </summary>
public sealed class InvitationTokens(IDataProtectionProvider provider, TimeProvider timeProvider)
{
    /// <summary>How long an invitation link works.</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(30);

    // The expiry is in the signed payload and checked against the app's clock (a time-limited protector uses the machine's).
    private readonly IDataProtector Protector = provider.CreateProtector("ModulesRegistry.Invitation.v1");

    public string Create(InvitationClaim claim) =>
        Protector.Protect(string.Join('\n',
            claim.PersonId.ToString(CultureInfo.InvariantCulture),
            claim.EmailAddress,
            claim.IsReadOnly ? "1" : "0",
            timeProvider.GetUtcNow().Add(Lifetime).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));

    /// <summary>The invitation in a token, or null when it is invalid, tampered with or expired.</summary>
    public InvitationClaim? Read(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var parts = Protector.Unprotect(token).Split('\n');
            return parts is [var id, var email, var readOnly, var expires] &&
                int.TryParse(id, NumberStyles.None, CultureInfo.InvariantCulture, out var personId) && personId > 0 &&
                long.TryParse(expires, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds) &&
                timeProvider.GetUtcNow() <= DateTimeOffset.FromUnixTimeSeconds(seconds)
                ? new InvitationClaim(personId, email, readOnly == "1")
                : null;
        }
        catch (CryptographicException)
        {
            // Expected, not logged: an invalid or tampered token gives nothing.
            return null;
        }
    }

    public static string Link(string baseUri, string token) => $"{baseUri}users/confirm?invitation={Uri.EscapeDataString(token)}";
}