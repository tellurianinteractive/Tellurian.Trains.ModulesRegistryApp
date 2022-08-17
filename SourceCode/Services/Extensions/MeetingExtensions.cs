using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static class MeetingExtensions
{
    public static int DaysCount(this Meeting it) =>
         (it.EndDate - it.StartDate).Days + 1;

    public static string Day(this Meeting it, int day) => 
        it.StartDate.AddDays(day - 1).DayOfWeek.ToString();

    public static bool IsCancelled(this Meeting? it) =>
        it is null || it.Status == (int)MeetingStatus.Canceled;

    public static bool IsOpenForRegistration([NotNullWhen(true)] this Meeting? it, DateTime at) =>
        it is not null && !it.IsCancelled() && it.Layouts.Any() && 
        it.Layouts.Any(l => l.RegistrationOpeningDate <= at) && 
        it.Layouts.Any(l => l.RegistrationClosingDate >= at);

    public static bool IsClosedForRegistration([NotNullWhen(true)] this Meeting? it, DateTime at) =>
        it is not null && it.Layouts.Any() && it.Layouts.Any(l => l.RegistrationClosingDate <= at);

    public static bool MayRegister(this Meeting? it, DateTime at, ClaimsPrincipal? principal) =>
       principal is not null && principal.IsAnyAdministrator() ||
        (principal.IsAuthenticated() && it.IsOpenForRegistration(at));

    public static string Organiser(this Meeting? me) =>
        me is null ? string.Empty :
        me.GroupDomainId.HasValue ? $"{me.OrganiserGroup.FullName}/{me.GroupDomain?.Name}" :
        $"{me.OrganiserGroup.FullName}";

    public static string StatusResourceName(this Meeting? me, DateTime at) =>
        me is null ? string.Empty :
        me.IsOrganiserInternal ? "Internal" :
        me.IsOpenForRegistration(at) ? "RegistrationOpen" :
        me.IsClosedForRegistration(at) ? "RegistrationClosed" :
        ((MeetingStatus)me.Status).ToString();

    public static string Status(this Meeting? it, DateTime at) =>
        it is null ? string.Empty :
        Resources.Strings.ResourceManager.GetString(it.StatusResourceName(at)) ?? string.Empty;

}

