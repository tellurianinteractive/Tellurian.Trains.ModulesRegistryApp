using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Services.Diagnostics;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Security events (Migration step 2C.4): successful sign-in Information, everything else Warning, ids only.
/// </summary>
[TestClass]
public sealed class SecurityLogTests
{
    [TestMethod]
    public void SignedInIsInformation() =>
        AssertLogged(logger => logger.SignedIn(42), LogLevel.Information, SecurityLog.SignedInEventId, "User 42 signed in");

    [TestMethod]
    [DataRow(SignInFailure.WrongPassword)]
    [DataRow(SignInFailure.AlreadyLockedOut)]
    [DataRow(SignInFailure.NoPerson)]
    public void SignInFailedIsWarningWithTheReason(SignInFailure reason) =>
        AssertLogged(logger => logger.SignInFailed(42, reason), LogLevel.Warning, SecurityLog.SignInFailedEventId, $"Sign-in failed for user 42: {reason}");

    [TestMethod]
    public void SignInFailedForAnUnknownAddressHasNoIdOrAddress() =>
        AssertLogged(logger => logger.SignInFailedUnknownUser(), LogLevel.Warning, SecurityLog.SignInFailedUnknownUserEventId, "Sign-in failed for an address that has no user");

    [TestMethod]
    public void SignedOutIsWarning() =>
        AssertLogged(logger => logger.SignedOut(42), LogLevel.Warning, SecurityLog.SignedOutEventId, "User 42 signed out");

    [TestMethod]
    public void LockedOutIsWarning() =>
        AssertLogged(logger => logger.LockedOut(42, 4), LogLevel.Warning, SecurityLog.LockedOutEventId, "User 42 is locked out after 4 failed sign-ins");

    [TestMethod]
    [DataRow(1, false)]
    [DataRow(4, true)]
    public void PasswordResetRequestedIsWarning(int attempt, bool limitReached) =>
        AssertLogged(logger => logger.PasswordResetRequested(42, attempt, limitReached), LogLevel.Warning, SecurityLog.PasswordResetRequestedEventId,
            $"Password reset requested for user 42, request {attempt}, limit reached: {limitReached}");

    [TestMethod]
    public void PasswordResetForAnUnknownAddressHasNoIdOrAddress() =>
        AssertLogged(logger => logger.PasswordResetUnknownUser(), LogLevel.Warning, SecurityLog.PasswordResetUnknownUserEventId, "Password reset requested for an address that has no user");

    [TestMethod]
    public void EventIdsAreUniqueAndInTheSecurityRange()
    {
        int[] ids = [SecurityLog.SignedInEventId, SecurityLog.SignInFailedEventId, SecurityLog.SignInFailedUnknownUserEventId, SecurityLog.SignedOutEventId,
            SecurityLog.LockedOutEventId, SecurityLog.PasswordResetRequestedEventId, SecurityLog.PasswordResetUnknownUserEventId];
        Assert.HasCount(ids.Length, ids.Distinct());
        Assert.IsTrue(ids.All(id => id is >= 1000 and <= 1999));
    }

    private static void AssertLogged(Action<ILogger> log, LogLevel level, int eventId, string message)
    {
        var logger = new FakeLogger();

        log(logger);

        var record = logger.LatestRecord;
        Assert.AreEqual(level, record.Level);
        Assert.AreEqual(eventId, record.Id.Id);
        Assert.AreEqual(message, record.Message);
        Assert.IsNull(record.Exception);
    }
}