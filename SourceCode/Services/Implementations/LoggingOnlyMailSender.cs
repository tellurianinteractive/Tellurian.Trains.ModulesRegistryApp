using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace ModulesRegistry.Services.Implementations;

public sealed class LoggingOnlyMailSender(ILogger<LoggingOnlyMailSender> logger) : IMailSender
{
    private readonly ILogger Logger = logger;

    public Task<int> SendMailMessageAsync(MailMessage message)
    {
        message.From = new MailAddress("development@none.com", "Ingen mottagare");
#pragma warning disable CA2254 // Template should be a static expression
        Logger.LogInformation(MessageSummary(message));
#pragma warning restore CA2254 // Template should be a static expression
        return Task.FromResult(1);

        static string MessageSummary(MailMessage message) =>
            $"{message.Subject} to {string.Join(';', message.To.AsEnumerable().Select(m => m.Address))}\n{message.Body}";
    }

}
