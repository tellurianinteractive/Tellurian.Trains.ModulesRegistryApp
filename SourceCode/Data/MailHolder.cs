using ModulesRegistry.Data.Extensions;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace ModulesRegistry.Data;

public record MailHolder(string Name, string EmailAddresses);

public record MailGroup(string Href, string Subset);

public static class MailHolderExtensions
{
    public static string MailtoHref(this IEnumerable<MailHolder> receipients, MailHolder sender, string subject)
    {
        var href = new StringBuilder(1000);
        href.Append("mailto:");
        href.Append(sender.EmailWithName());
        href.Append("?bcc=");
        href.Append(string.Join(",", receipients.Where(r => r.EmailAddresses.HasValue() && !r.EmailAddresses.Contains(sender.EmailAddresses)).Select(r => r.EmailWithName())));
        href.Append("&subject=");
        href.Append(subject);
        return href.ToString();
    }

    private static string EmailWithName(this MailHolder holder) => $"{HttpUtility.UrlEncode(holder.Name)}%20<{holder.EmailAddresses}>";
    public static MailHolder Sender(this ClaimsPrincipal principal) =>
       new(principal.GivenName() ?? principal.EmailAddess(), principal.EmailAddess());

    public static IEnumerable<MailHolder> ParticipantsMails(this Meeting? meeting) =>
        meeting?.Participants.Select(p => new MailHolder(p.Person.FirstName, p.Person.EmailAddresses)) ?? Array.Empty<MailHolder>();

    public static IEnumerable<MailHolder> ParticipantsMails(this IEnumerable<LayoutParticipant>? participants) =>
        participants is not null ? participants.Select(lp => new MailHolder(lp.Person.FirstName, lp.Person.EmailAddresses)) : [];

    public static IEnumerable<MailHolder> MailHolders(this IEnumerable<GroupMember>? groupMembers) =>
        groupMembers is not null ? groupMembers.Select(gm => new MailHolder(gm.Person.FirstName, gm.Person.EmailAddresses)) : [];

    public static MailHolder MailHolder(this ClaimsPrincipal principal) => new(principal.GivenName() ?? "Admin", principal.EmailAddess());

    /// <summary>
    /// This function handle the fact that mailto hrefs cannot exceed a certain length.
    /// </summary>
    /// <param name="mailHolders"></param>
    /// <returns></returns>
    public static IEnumerable<MailGroup> MailHrefGroups(this IEnumerable<MailHolder> mailHolders, MailHolder sender, string subject, int maxHrefLenght = 1800)
    {
        var result = new List<MailGroup>();
        var href = CreateHref(sender, subject, maxHrefLenght);
        var mails = new List<string>(100);
        string? FirstName = null;
        string? LastName = null;
        foreach (var mailHolder in mailHolders.Where(mh => !mh.Equals(sender)).OrderBy(mh=> mh.Name))
        {
            FirstName ??= mailHolder.Name;
            var mail = mailHolder.EmailWithName();
            mails.Add(mail);
            LastName = mailHolder.Name;
            if (href.Length + mails.Sum(m => m.Length+1) > maxHrefLenght)
            {
                href.Append(string.Join(',', mails));
                result.Add(new(href.ToString(), $"{FirstName}>{LastName}"));
                href = CreateHref(sender, subject, maxHrefLenght);
                mails.Clear();
                FirstName = null;
            };
         
        }
        href.Append(string.Join(',', mails));
        result.Add(new(href.ToString(), $"{FirstName}>{LastName}"));
        return result;

        static StringBuilder CreateHref(MailHolder sender, string subject, int length)
        {
            var href = new StringBuilder(length);
            href.Append("mailto:");
            href.Append(sender.EmailWithName());
            href.Append("?subject=");
            href.Append(subject);
            href.Append("&bcc=");
            return href;
        }
    }

}