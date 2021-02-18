using Microsoft.Extensions.Logging;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Implementations
{
    public class LoggingOnlyMailSender : IMailSender
    {
        public LoggingOnlyMailSender(ILogger<LoggingOnlyMailSender> logger)
        {
            Logger = logger;
        }
        private readonly ILogger Logger;

        public Task<int> SendMailMessageAsync(MailMessage message)
        {
            message.From = new MailAddress("development@none.com", "Ingen mottagare");
            Logger.LogInformation(message.ToString());
            return Task.FromResult(1);
        }
    }
}
