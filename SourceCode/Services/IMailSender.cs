using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace ModulesRegistry.Services
{
    public interface IMailSender
    {
        Task<int> SendMailMessageAsync(MailMessage message);
    }
}
