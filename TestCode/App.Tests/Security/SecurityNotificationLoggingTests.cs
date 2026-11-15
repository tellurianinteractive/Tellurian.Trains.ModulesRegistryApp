using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using ModulesRegistry.Data;
using ModulesRegistry.Security;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.AppTests.Security;

/// <summary>
/// A security notification is best-effort: a failure doesn't stop the sign-in flow, and is logged as Error (Migration step 2C.3).
/// </summary>
[TestClass]
public sealed class SecurityNotificationLoggingTests
{
    [TestMethod]
    public async Task FailedNotificationIsLoggedAsErrorWithTheUserIdOnly()
    {
        var logger = new FakeLogger();
        var user = new User { Id = 42, EmailAddress = "someone@example.com", Person = new Person { FirstName = "Some", LastName = "One" } };
        var contentFolder = Directory.CreateTempSubdirectory().FullName;
        File.WriteAllText(Path.Combine(contentFolder, "AccountLockedMail.md"), "Your account is locked.");
        var contentService = new ContentService(new NoHttpClientFactory(), contentFolder);

        await LoginLogoutHandler.SendSecurityNotification(user, "AccountLockedMail", "AccountLockedMailSubject", new ThrowingMailSender(), contentService, logger);

        var record = logger.LatestRecord;
        Assert.AreEqual(LogLevel.Error, record.Level);
        Assert.IsInstanceOfType<SmtpException>(record.Exception, "The failure comes from sending, so the notification got that far.");
        Assert.AreEqual("Sending the security notification AccountLockedMail to user 42 failed", record.Message);
        Assert.DoesNotContain("someone@example.com", record.Message);
    }

    private sealed class ThrowingMailSender : IMailSender
    {
        public Task<int> SendMailMessageAsync(MailMessage message) => throw new SmtpException("Test failure");
    }

    private sealed class NoHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => throw new NotSupportedException();
    }
}