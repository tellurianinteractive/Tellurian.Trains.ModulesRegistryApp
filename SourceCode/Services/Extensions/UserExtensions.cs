using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using System.Text;

namespace ModulesRegistry.Services.Extensions
{
    public static class UserExtensions
    {
        public static string? PreferredLanguage(this User me) =>
            me.Person is null || me.Person.Country is null ? null :
            me.Person.Country.Languages.Items()[0];

        public static string ConfirmationLink(this User? me, string baseUri) =>
            me is null || me.Person is null ? string.Empty :
                $"{baseUri}users/confirm?email={me.PrimaryEmail()}&objectid={me.ObjectId}";

        public static string ConfirmationLinkTag(this User? me, string baseUri) =>
            me is null ? string.Empty :
            $"<p><a href=\"{me.ConfirmationLink(baseUri)}\">{LanguageService.GetString("CreatePassword", me.PreferredLanguage())}</a></p>";

        public static string PrimaryEmail(this User? me) =>
            me is null ? string.Empty :
            me.Person is null ? me.EmailAddress :
            me.Person.PrimaryEmail();

        public static string PrimaryEmail(this Person? me) =>
            me is null ? string.Empty :
            me.EmailAddresses.Items()[0];

        public static bool HasEmail([NotNullWhen(true)] this User? me) =>
            !string.IsNullOrWhiteSpace(me.PrimaryEmail());

        public static bool HasEmail([NotNullWhen(true)] this Person? me) =>
            !string.IsNullOrWhiteSpace(me.PrimaryEmail());

        public static string Name(this Person? me) =>
            me is not null ? $"{me.FirstName} {me.MiddleName} {me.LastName}";

        public static bool IsNeverLoggedIn(this Person? me) =>
            me is null || me.User is null || me.User.LastSignInTime is null;

        public static bool IsInvited([NotNullWhen(true)] this Person? me) =>
            me is not null && me.User is not null && me.User.LastSignInTime is null;

        public static bool IsPasswordResetPermitted([NotNullWhen(true)] this User? me) =>
            me is not null && me.PasswordResetAttempts <= PasswordResetRequest.MaxRequests;

        public static string GetMessageHtml(this UserInvitation me)
        {
            if (!me.IsValid) return string.Empty;
            var preferredLanguage = me.Recipient.PreferredLanguage();
            var text = new StringBuilder(1000);
            text.AppendHelloPhrase(me);
            if (me.HasPersonalMessage) text.Append($"<p>{me.PersonalMessage}</p>");
            text.AppendLine(me.Message.AsHtml);
            text.AppendLine(me.Recipient.ConfirmationLinkTag(me.BaseUri));
            text.Append($"<p>{LanguageService.GetString("BestRegards", preferredLanguage)}</p>");
            text.Append($"<p>{me.Inviter.FirstName} {me.Inviter.LastName}</p>");
            return text.ToString();
        }

        public static MailMessage? AsMailMessage(this UserMessage me)
        {
            if (!me.IsValid) return null;
            var body = me.MessageHtml;
            if (body.HasNoValue()) return null;
            var message = new MailMessage();
            message.To.Add(new MailAddress(me.Recipient.EmailAddress, $"{me.Recipient.Person.FirstName} {me.Recipient.Person.LastName}"));
            message.Subject = me.Subject;
            message.Body = body;
            message.IsBodyHtml = true;
            return message;
        }

        public static string GetMessageHtml(this PasswordResetRequest me)
        {
            if (!me.IsValid) return string.Empty;
            var preferredLanguage = me.Recipient.PreferredLanguage();
            var text = new StringBuilder(1000);
            text.AppendHelloPhrase(me);
            text.AppendLine(me.Message.AsHtml);
            text.AppendLine(me.Recipient.ConfirmationLinkTag(me.BaseUri));
            text.Append($"<p>{LanguageService.GetString("BestRegards", preferredLanguage)}</p>");
            text.Append($"<p>{LanguageService.GetString("AppName", preferredLanguage)}</p>");
            return text.ToString();
        }

        private static void AppendHelloPhrase(this StringBuilder me, UserMessage message)
        {
            me.Append("<h1>");
            me.Append(LanguageService.GetString("Hello", message.Recipient.PreferredLanguage()));
            me.Append(' ');
            me.Append(message.Recipient.Person.FirstName);
            me.AppendLine("</h1>");
        }
    }

    public abstract record UserMessage(User Recipient, string Subject, TextContent Message)
    {
        public bool IsValid => Recipient.Person is not null && !IsInvalid;
        public virtual string MessageHtml => string.Empty;
        protected virtual bool IsInvalid => false;
    }

    public record UserInvitation(User Recipient, Person Inviter, string Subject, TextContent Message, string BaseUri) : UserMessage(Recipient, Subject, Message)
    {
        public string? PersonalMessage { get; init; }
        public bool HasPersonalMessage => !string.IsNullOrWhiteSpace(PersonalMessage);
        public override string MessageHtml => this.GetMessageHtml();
        
    }

    public record PasswordResetRequest(User Recipient, string Subject, TextContent Message, string BaseUri) : UserMessage(Recipient, Subject, Message)
    {
        public override string MessageHtml => this.GetMessageHtml();
        protected override bool IsInvalid => Recipient.PasswordResetAttempts > MaxRequests;
        public const int MaxRequests = 3;
    }
}


