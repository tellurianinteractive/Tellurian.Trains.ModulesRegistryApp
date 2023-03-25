using ModulesRegistry.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Models;
public abstract record UserMessage(User Recipient, string Subject, TextContent Message)
{
    public bool IsValid => Recipient.Person is not null && !IsInvalid;
    public virtual string MessageHtml => string.Empty;
    public MailMessage? MailMessage => AsMailMessage(this);
    protected virtual bool IsInvalid => false;

    static MailMessage? AsMailMessage(UserMessage message)
    {
        if (!message.IsValid) return null;
        var body = message.MessageHtml;
        if (body.HasNoValue()) return null;
        var mail = new MailMessage();
        mail.To.Add(new MailAddress(message.Recipient.EmailAddress, $"{message.Recipient.Person.FirstName} {message.Recipient.Person.LastName}"));
        mail.Subject = message.Subject;
        mail.Body = body;
        mail.IsBodyHtml = true;
        return mail;
    }
    protected void AppendHelloPhrase(StringBuilder stringBuilder)
    {
        stringBuilder.Append("<h1>");
        stringBuilder.Append(LanguageUtility.GetLocalizedString("Hello", Recipient.PreferredLanguage()));
        stringBuilder.Append(' ');
        stringBuilder.Append(Recipient.Person.FirstName);
        stringBuilder.AppendLine("</h1>");
    }
}

public record UserInvitation(User Recipient, Person Inviter, string Subject, TextContent Message, string BaseUri) : UserMessage(Recipient, Subject, Message)
{
    public string? PersonalMessage { get; init; }
    public bool HasPersonalMessage => !string.IsNullOrWhiteSpace(PersonalMessage);
    public override string MessageHtml => GetMessageHtml(this);

    static string GetMessageHtml(UserInvitation invitation)
    {
        if (!invitation.IsValid) return string.Empty;
        var preferredLanguage = invitation.Recipient.PreferredLanguage();
        var text = new StringBuilder(1000);
        invitation.AppendHelloPhrase(text);
        if (invitation.HasPersonalMessage) text.Append($"<p>{invitation.PersonalMessage}</p>");
        text.AppendLine(invitation.Message.AsHtml);
        text.AppendLine(invitation.Recipient.ConfirmationLinkTag(invitation.BaseUri));
        text.Append($"<p>{LanguageUtility.GetLocalizedString("BestRegards", preferredLanguage)}</p>");
        text.Append($"<p>{invitation.Inviter.FirstName} {invitation.Inviter.LastName}</p>");
        return text.ToString();
    }

}

public record PasswordResetRequest(User Recipient, string Subject, TextContent Message, string BaseUri) : UserMessage(Recipient, Subject, Message)
{
    public override string MessageHtml => GetMessageHtml(this);
    protected override bool IsInvalid => Recipient.PasswordResetAttempts > MaxRequests;
    public const int MaxRequests = 3;

    static string GetMessageHtml(PasswordResetRequest passwordResetRequest)
    {
        if (!passwordResetRequest.IsValid) return string.Empty;
        var preferredLanguage = passwordResetRequest.Recipient.PreferredLanguage();
        var text = new StringBuilder(1000);
        passwordResetRequest.AppendHelloPhrase(text);
        text.AppendLine(passwordResetRequest.Message.AsHtml);
        text.AppendLine(passwordResetRequest.Recipient.ConfirmationLinkTag(passwordResetRequest.BaseUri));
        text.Append($"<p>{LanguageUtility.GetLocalizedString("BestRegards", preferredLanguage)}</p>");
        text.Append($"<p>{LanguageUtility.GetLocalizedString("AppName", preferredLanguage)}</p>");
        return text.ToString();
    }

}
