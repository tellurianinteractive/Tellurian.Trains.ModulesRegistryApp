using System.Net;
using System.Net.Mail;

namespace ModulesRegistry.Services.Implementations;

public sealed class CloudMailSender(IOptions<CloudMailSenderSettings> options) : IMailSender
{
    private readonly CloudMailSenderSettings Settings = options.Value;

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
        new(Settings.SmtpServerName, Settings.SmtpServerPortNumber) { Credentials = NetworkCredential };
    // TODO: Remove ApiKey after Brevo version is deployed.
    private NetworkCredential NetworkCredential =>
        new(Settings.UserName ?? "api-key", Settings.Password ?? Settings.ApiKey);
}

/// <summary>
/// TODO: When change to Brevo SMTP mail service is deployed and tested,
/// The <see cref="ApiKey"/> field can be removed.
/// </summary>
public class CloudMailSenderSettings
{
    public required string SenderMailAddress { get; set; }
    public required string SmtpServerName { get; set; }
    public int SmtpServerPortNumber { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public  string? ApiKey { get; set; }
}


