using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
public static class MeetingExtensions
{
    public static bool IsOpenForRegistration([NotNullWhen(true)] this Meeting? it, DateTime at) =>
        it is not null && !it.IsCancelled() && it.Layouts.Any() && it.Layouts.Min(l => l.RegistrationOpeningDate) <= at && it.Layouts.Max(l => l.RegistrationClosingDate) > at;

    public static bool MayRegister(this Meeting? it, DateTime at, ClaimsPrincipal? principal) =>
       principal is not null && principal.IsAnyAdministrator() ||
        (principal.IsAuthenticated() && it.IsOpenForRegistration(at));


    public static bool IsCancelled(this Meeting? it) => it is null || it.Status == (int)MeetingStatus.Canceled;
    public static int DaysCount(this Meeting it) => (it.EndDate - it.StartDate).Days + 1;
    public static string Day(this Meeting it, int day) => it.StartDate.AddDays(day - 1).DayOfWeek.ToString();

}

