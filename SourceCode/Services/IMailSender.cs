using System.Net.Mail;
using System.Threading.Tasks;

namespace ModulesRegistry.Services
{
    public interface IMailSender
    {
        Task<int> SendMailMessageAsync(MailMessage message);
    }
}
