using System.Net.Mail;

namespace ModulesRegistry.Services
{
    public interface IMailSender
    {
        Task<int> SendMailMessageAsync(MailMessage message);
    }
}
