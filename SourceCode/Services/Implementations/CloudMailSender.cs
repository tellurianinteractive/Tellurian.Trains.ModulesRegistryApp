using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace ModulesRegistry.Services.Implementations;

public sealed class CloudMailSender : IMailSender
{
    public CloudMailSender(IOptions<CloudMailSenderSettings> options)
    {
        Settings = options.Value;
    }
    private readonly CloudMailSenderSettings Settings;

    public async Task<int> SendMailMessageAsync(MailMessage message)
    {
        if (message.To.Count < 1) throw new SmtpException("No receiver(s).");
        if (message.Subject is null) throw new SmtpException("No message subject.");
        if (string.IsNullOrWhiteSpace(message.Body)) throw new SmtpException("No message body.");
        message.From = Sender;
        using var client = Client;
        try
        {
            await client.SendMailAsync(message).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return 0;
        }
        return message.To.Count + message.CC.Count + message.Bcc.Count;
    }

    private MailAddress Sender =>
        new(Settings.SenderMailAddress, Resources.Strings.AppName);

    private SmtpClient Client =>
        new("smtp.sendgrid.net", 587) { Credentials = NetworkCredential };

    private NetworkCredential NetworkCredential =>
        new("apikey", Settings.ApiKey);
}

public class CloudMailSenderSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SenderMailAddress { get; set; } = string.Empty;
}
