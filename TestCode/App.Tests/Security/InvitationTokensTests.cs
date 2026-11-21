using Microsoft.AspNetCore.DataProtection;
using ModulesRegistry.Security;

namespace ModulesRegistry.AppTests.Security;

/// <summary>
/// Invitation links (decided 2026-11-21): the account is created when the invitation is answered, so the link carries the invitation,
/// signed and time-limited. A changed, foreign or expired token gives nothing.
/// </summary>
[TestClass]
public sealed class InvitationTokensTests
{
    private static readonly IDataProtectionProvider Provider = new EphemeralDataProtectionProvider();
    private readonly TestClock _clock = new(new DateTimeOffset(2026, 11, 21, 12, 0, 0, TimeSpan.Zero));
    private InvitationTokens Target => new(Provider, _clock);

    [TestMethod]
    public void ATokenGivesBackTheInvitation()
    {
        var token = Target.Create(new InvitationClaim(11, "erik@example.se", true));
        Assert.AreEqual(new InvitationClaim(11, "erik@example.se", true), Target.Read(token));
    }

    [TestMethod]
    public void AChangedTokenGivesNothing()
    {
        var token = Target.Create(new InvitationClaim(11, "erik@example.se", false));
        Assert.IsNull(Target.Read(token[..^2] + (token[^2] == 'A' ? "B" : "A") + token[^1]));
        Assert.IsNull(Target.Read("not a token"));
        Assert.IsNull(Target.Read(null));
    }

    [TestMethod]
    public void ATokenForAnotherPurposeGivesNothing()
    {
        var other = Provider.CreateProtector("ModulesRegistry.PublicDocument.v1").Protect("11\nerik@example.se\n0");
        Assert.IsNull(Target.Read(other));
    }

    [TestMethod]
    public void AnExpiredTokenGivesNothing()
    {
        var token = Target.Create(new InvitationClaim(11, "erik@example.se", false));
        _clock.Now = _clock.Now.Add(InvitationTokens.Lifetime).AddMinutes(1);
        Assert.IsNull(Target.Read(token));
    }

    [TestMethod]
    public void TheLinkEscapesTheToken() =>
        Assert.AreEqual("https://example.com/users/confirm?invitation=a%2Bb%2F%3D", InvitationTokens.Link("https://example.com/", "a+b/="));

    private sealed class TestClock(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;
        public override DateTimeOffset GetUtcNow() => Now;
    }
}