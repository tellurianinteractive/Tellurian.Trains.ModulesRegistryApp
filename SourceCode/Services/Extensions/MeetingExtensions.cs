using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static class MeetingExtensions
{
    public static string Day(this Meeting meeting, int day) =>
        meeting.StartDate.AddDays(day - 1).DayOfWeek.ToString();

    public static int DaysCount(this Meeting meeting) =>
         (meeting.EndDate - meeting.StartDate).Days + 1;

    public static bool IsCancelled([NotNullWhen(true)] this Meeting? meeting) =>
        meeting is not null && meeting.Status == (int)MeetingStatus.Canceled;

    private static bool IsNotCancelled([NotNullWhen(true)] this Meeting? meeting) =>
        !meeting.IsCancelled();

    public static bool IsOpenForRegistration([NotNullWhen(true)] this Meeting? meeting, DateTime at) =>
        meeting is not null &&
        meeting.IsNotCancelled() &&
        meeting.IsAnyLayoutRegistrationPermitted() &&
        meeting.Layouts.Any(l => l.IsOpenForRegistration(at));

    private static bool IsClosedForRegistration([NotNullWhen(false)] this Meeting? meeting, DateTime at) =>
        meeting is not null && 
        meeting.IsAnyLayoutRegistrationPermitted() &&
        meeting.Layouts.Any() && meeting.Layouts.All(l => l.RegistrationClosingDate <= at);

    public static bool MayDelete(this Meeting? meeting, ClaimsPrincipal principal) =>
        meeting is not null &&
        principal.IsCountryOrGlobalAdministrator() && 
        meeting.Layouts.Sum(l => l.LayoutParticipants.Count) == 0;

    public static DateTime? RegistrationOpensDate(this Meeting? meeting) =>
        meeting is null || !meeting.IsAnyLayoutRegistrationPermitted() ? null :
        meeting.Layouts.Where(l => l.IsRegistrationPermitted).Min(l => l.RegistrationOpeningDate);

    public static DateTime? RegistrationClosingDate(this Meeting? meeting) =>
        meeting is null || !meeting.IsAnyLayoutRegistrationPermitted() ? null :
        meeting.Layouts.Where(l => l.IsRegistrationPermitted).Max(l => l.RegistrationClosingDate);

    public static DateTime? RegistrationOfModulesClosingDate(this Meeting? meeting) =>
        meeting is null || !meeting.IsAnyLayoutRegistrationPermitted() ? null :
        meeting.Layouts.Where(l => l.IsRegistrationPermitted).Max(l => l.ModuleRegistrationClosingDate ?? l.RegistrationClosingDate);

    public static bool IsNotYetOpenForRegistration([NotNullWhen(true)] this Meeting? meeting, DateTime at) =>
        meeting is not null &&
        meeting.Layouts.All(l => l.IsNotYetOpenForRegistration(at));

    private static bool IsAnyLayoutRegistrationPermitted(this Meeting meeting) =>
        meeting.Layouts.Any(l => l.IsRegistrationPermitted);

    public static string Organiser(this Meeting? meeting) =>
        meeting is null ? string.Empty :
        meeting.GroupDomainId.HasValue ? $"{meeting.OrganiserGroup.FullName}/{meeting.GroupDomain?.Name}" :
        $"{meeting.OrganiserGroup.FullName}";

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

