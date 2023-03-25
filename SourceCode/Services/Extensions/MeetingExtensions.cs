using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;

public static class LayoutExtensions
{
    public static string RegistrationOpensDate(this Layout layout) => layout.RegistrationOpeningDate.ToShortDateString();
    public static string RegistrationClosesDate(this Layout layout) => layout.RegistrationClosingDate.ToShortDateString();
    public static string RegistrationOfModulesClosesDate(this Layout layout) => (layout.ModuleRegistrationClosingDate ?? layout.RegistrationClosingDate).ToShortDateString();
    internal static bool IsOpenForRegistration(this Layout layout, DateTime at) =>
        layout.IsRegistrationPermitted &&
        layout.RegistrationOpeningDate <= at &&
        layout.RegistrationClosingDate >= at;

    internal static bool IsNotYetOpenForRegistration(this Layout layout, DateTime at) =>
        layout.IsRegistrationPermitted &&
        layout.RegistrationOpeningDate > at;

}
public static class MeetingExtensions
{
    public static int DaysCount(this Meeting meeting) =>
         (meeting.EndDate - meeting.StartDate).Days + 1;

    public static string Day(this Meeting meeting, int day) =>
        meeting.StartDate.AddDays(day - 1).DayOfWeek.ToString();

    public static bool IsCancelled(this Meeting? meeting) =>
        meeting is null || meeting.Status == (int)MeetingStatus.Canceled;

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
        meeting.Layouts.Any(l => l.IsNotYetOpenForRegistration(at));

    public static bool IsOpenForRegistration([NotNullWhen(true)] this Meeting? meeting, DateTime at, ClaimsPrincipal? principal = null)
    {       
        return principal is not null && meeting is not null && meeting.IsAnyLayoutRegistrationPermitted() && 
        (
            principal.IsMeetingOrganiserOrAdministrator(meeting) 
        ) ||
        (   meeting is not null && !meeting.IsCancelled() &&
            meeting.Layouts.Any(l => l.IsOpenForRegistration(at)));
    }

    public static bool IsClosedForRegistration([NotNullWhen(false)] this Meeting? meeting, DateTime at) =>
        meeting is not null && meeting.IsAnyLayoutRegistrationPermitted() &&
        meeting.Layouts.Any() && meeting.Layouts.All(l => l.RegistrationClosingDate <= at);

    private static bool IsAnyLayoutRegistrationPermitted(this Meeting meeting) =>
        meeting.Layouts.Any(l => l.IsRegistrationPermitted);

    public static bool MayRegister(this Meeting? meeting, DateTime at, ClaimsPrincipal? principal) =>
       principal is not null && principal.IsAnyAdministrator() ||
        (principal.IsAuthenticated() && meeting.IsOpenForRegistration(at, principal));


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

