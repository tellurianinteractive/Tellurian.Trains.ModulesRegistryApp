using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using ModulesRegistry.Services.Models;

namespace ModulesRegistry.Security;

internal static class LoginLogoutHandler
{
    internal static async Task SendSecurityNotification(User recipient, string contentName, string subjectKey, IMailSender mailSender, ContentService contentService, ILogger logger)
    {
        try
        {
            var language = recipient.PreferredLanguage();
            var markdown = await contentService.GetTextContent(contentName, language);
            var subject = LanguageExtensions.GetLocalizedString(subjectKey, language);
            var notification = new SecurityNotification(recipient, subject, markdown);
            var message = notification.MailMessage;
            if (message is not null) _ = await mailSender.SendMailMessageAsync(message);
        }
        catch (Exception ex)
        {
            // Best-effort: a security notification never blocks the primary flow.
            logger.LogError(ex, "Sending the security notification {ContentName} to user {UserId} failed", contentName, recipient.Id);
        }
    }
}