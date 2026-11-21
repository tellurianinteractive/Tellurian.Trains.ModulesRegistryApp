using ModulesRegistry.Services.Implementations;
using System.Net.Mail;
using System.Text;

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
        if (body.IsEmpty) return null;
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
        stringBuilder.Append(LanguageExtensions.GetLocalizedString("Hello", Recipient.PreferredLanguage()));
        stringBuilder.Append(' ');
        stringBuilder.Append(Recipient.Person.FirstName);
        stringBuilder.AppendLine("</h1>");
    }
}

/// <summary>
/// An invitation to the app. <paramref name="Recipient"/> is the person's user, or, for a person without one, an unsaved user with the
/// invited address and the person (the account is created when the invitation is answered); <see cref="ConfirmationLink"/> is then the
/// link with the invitation's token.
/// </summary>
public record UserInvitation(User Recipient, Person Inviter, string Subject, TextContent Message, string BaseUri) : UserMessage(Recipient, Subject, Message)
{
    public string? PersonalMessage { get; init; }
    /// <summary>The link to answer the invitation; when null, the existing user's confirmation link.</summary>
    public string? ConfirmationLink { get; init; }
    public bool HasPersonalMessage => !string.IsNullOrWhiteSpace(PersonalMessage);
    public override string MessageHtml => GetMessageHtml(this);

    static string GetMessageHtml(UserInvitation invitation)
    {
        if (!invitation.IsValid) return string.Empty;
        var preferredLanguage = invitation.Recipient.PreferredLanguage();
        var text = new StringBuilder(1000);
        invitation.AppendHelloPhrase(text);
        if (invitation.HasPersonalMessage) text.Append($"<p>{System.Net.WebUtility.HtmlEncode(invitation.PersonalMessage)}</p>"); // Typed by the inviter: text, not HTML.
        text.AppendLine(invitation.Message.AsHtml);
        text.AppendLine(invitation.ConfirmationLink is null ? invitation.Recipient.ConfirmationLinkTag(invitation.BaseUri) :
            $"<p><a href=\"{invitation.ConfirmationLink}\">{LanguageExtensions.GetLocalizedString("CreatePassword", preferredLanguage)}</a></p>");
        text.Append($"<p>{LanguageExtensions.GetLocalizedString("BestRegards", preferredLanguage)}</p>");
        text.Append($"<p>{invitation.Inviter.FirstName} {invitation.Inviter.LastName}</p>");
        return text.ToString();
    }

}

public record SecurityNotification(User Recipient, string Subject, TextContent Message) : UserMessage(Recipient, Subject, Message)
{
    public override string MessageHtml => GetMessageHtml(this);

    static string GetMessageHtml(SecurityNotification notification)
    {
        if (!notification.IsValid) return string.Empty;
        var preferredLanguage = notification.Recipient.PreferredLanguage();
        var text = new StringBuilder(1000);
        notification.AppendHelloPhrase(text);
        text.AppendLine(notification.Message.AsHtml);
        text.Append($"<p>{LanguageExtensions.GetLocalizedString("BestRegards", preferredLanguage)}</p>");
        text.Append($"<p>{LanguageExtensions.GetLocalizedString("AppName", preferredLanguage)}</p>");
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
        text.Append($"<p>{LanguageExtensions.GetLocalizedString("BestRegards", preferredLanguage)}</p>");
        text.Append($"<p>{LanguageExtensions.GetLocalizedString("AppName", preferredLanguage)}</p>");
        return text.ToString();
    }

}