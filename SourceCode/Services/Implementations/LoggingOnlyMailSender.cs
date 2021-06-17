using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public sealed class LoggingOnlyMailSender : IMailSender
    {
        public LoggingOnlyMailSender(ILogger<LoggingOnlyMailSender> logger)
        {
            Logger = logger;
        }
        private readonly ILogger Logger;

        public Task<int> SendMailMessageAsync(MailMessage message)
        {
            message.From = new MailAddress("development@none.com", "Ingen mottagare");
            Logger.LogInformation(MessageSummary(message));
            return Task.FromResult(1);

            static string MessageSummary(MailMessage message) =>
                $"{message.Subject} to {string.Join(';', message.To.AsEnumerable().Select(m => m.Address))}\n{message.Body}";
        }

    }
}
