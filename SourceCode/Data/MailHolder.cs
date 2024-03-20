using ModulesRegistry.Data.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace ModulesRegistry.Data;

public record MailHolder(string Name, string EmailAddresses);

public static class MailHolderExtensions
{
    public static string MailtoHref(this IEnumerable<MailHolder> receipients, MailHolder sender, string subject)
    {
        var href = new StringBuilder(1000);
        href.Append("mailto:");
        href.Append(sender.FormattedEmail());
        href.Append("?bcc=");
        href.Append(string.Join(",", receipients.Where(r => r.EmailAddresses.HasValue() && !r.EmailAddresses.Contains(sender.EmailAddresses)).Select(r => r.FormattedEmail())));
        href.Append("&subject=");
        href.Append(subject);
        return href.ToString();
    }

    private static string FormattedEmail(this MailHolder holder) => $"<{HttpUtility.UrlEncode(holder.Name)}>{holder.EmailAddresses}";

    public static MailHolder Sender (this ClaimsPrincipal principal) => 
       new (principal.GivenName() ?? principal.EmailAddess(), principal.EmailAddess());

    public static IEnumerable<MailHolder> ParticipantsMails(this Meeting? meeting) => 
        meeting?.Participants.Select(p => new MailHolder(p.Person.FirstName, p.Person.EmailAddresses)) ?? Array.Empty<MailHolder>();

    public static IEnumerable<MailHolder> ParticipantsMails(this IEnumerable<LayoutParticipant>? participants) =>
        participants is not null ? participants.Select(lp => new MailHolder(lp.Person.FirstName, lp.Person.EmailAddresses) ): Array.Empty<MailHolder>();
}