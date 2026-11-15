using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// Handled exceptions are logged as Error, without personal data, and expected input errors aren't logged (Migration step 2C.3).
/// </summary>
[TestClass]
public sealed class HandledExceptionLoggingTests
{
    private const string Address = "someone@example.com";

    [TestMethod]
    public void FailedMailIsLoggedAsErrorWithTheException()
    {
        var logger = new FakeLogger();
        var exception = new SmtpException("Failure sending mail.");

        CloudMailSender.LogFailure(logger, exception, recipientCount: 2);

        Assert.AreEqual(LogLevel.Error, logger.LatestRecord.Level);
        Assert.AreSame(exception, logger.LatestRecord.Exception);
        Assert.AreEqual("Sending a mail to 2 recipients failed", logger.LatestRecord.Message);
    }

    [TestMethod]
    public void FailedRecipientIsLoggedWithoutTheEmailAddress()
    {
        var logger = new FakeLogger();
        var exception = new SmtpFailedRecipientException(SmtpStatusCode.MailboxUnavailable, Address);
        Assert.AreEqual(Address, exception.FailedRecipient, "Precondition: the exception holds the address (and the server response in its message often does too).");

        CloudMailSender.LogFailure(logger, exception, recipientCount: 1);

        var record = logger.LatestRecord;
        Assert.AreEqual(LogLevel.Error, record.Level);
        Assert.IsNull(record.Exception, "The exception holds the e-mail address, so it isn't logged.");
        Assert.DoesNotContain(Address, record.Message);
        Assert.AreEqual("Sending a mail to 1 recipients failed: SmtpFailedRecipientException MailboxUnavailable", record.Message);
    }

    private static readonly ErrorCase[] Cases = [new("UX_WIFredThrottle_MacAddress", "Duplicated", "MacAddress")];

    [TestMethod]
    public void DuplicateInAUniqueIndexIsAnExpectedCase() =>
        Assert.IsTrue(new InvalidOperationException("Save failed", new InvalidOperationException("Cannot insert duplicate key row with unique index 'UX_WIFredThrottle_MacAddress'.")).IsAnyOf(Cases));

    [TestMethod]
    [DataRow("Timeout expired.")]
    [DataRow("Cannot insert duplicate key row with unique index 'UX_Other'.")]
    public void OtherDatabaseErrorsAreNotExpected(string innerMessage) =>
        Assert.IsFalse(new InvalidOperationException("Save failed", new InvalidOperationException(innerMessage)).IsAnyOf(Cases));

    [TestMethod]
    public void ExceptionWithoutInnerExceptionIsNotExpected() =>
        Assert.IsFalse(new InvalidOperationException("Save failed").IsAnyOf(Cases));
}