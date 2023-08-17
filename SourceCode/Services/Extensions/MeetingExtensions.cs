namespace ModulesRegistry.Services.Extensions;
public static class MeetingExtensions
{
    public static bool MayDelete(this Meeting? meeting, ClaimsPrincipal principal) =>
        meeting is not null &&
        principal.IsCountryOrGlobalAdministrator() &&
        meeting.Layouts.Sum(l => l.LayoutParticipants.Count) == 0;


 
    public static string Status(this Meeting? meeting, DateTime at) =>
        meeting is null ? string.Empty :
        Resources.Strings.ResourceManager.GetString(meeting.StatusResourceName(at)) ?? string.Empty;

    internal static string StatusResourceName(this Meeting? meeting, DateTime at) =>
        meeting is null ? string.Empty :
        meeting.IsCancelled() ? "Canceled" :
        meeting.IsOpenForRegistration(at) ? "RegistrationOpen" :
        meeting.IsClosedForRegistration(at) ? "RegistrationClosed" :
        meeting.IsOrganiserInternal ? "Internal" :
        ((MeetingStatus)meeting.Status).ToString();

    public static string MeetingStatusCssClass(this Meeting meeting, DateTime at) =>
        $"meeting {meeting.StatusResourceName(at).ToLowerInvariant()}";


}

