#nullable disable

using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace ModulesRegistry.Data;
#pragma warning disable IDE0028 // Simplify collection initialization

public class Meeting
{
    public Meeting()
    {
        Layouts = new HashSet<Layout>();
        Participants = new HashSet<MeetingParticipant>();
    }

    public int Id { get; set; }
    public int OrganiserGroupId { get; set; }
    public bool IsOrganiserInternal { get; set; }
    public int? GroupDomainId { get; set; }
    public string CityName { get; set; }
    public string VenueName { get; set; }
    public string Name { get; set; }
    public DateTime StartDate { get; set; } = DateTime.Today.AddYears(1);
    public DateTime EndDate { get; set; } = DateTime.Today.AddYears(1).AddDays(4);
    public TimeOnly? LatestArrivalTimeWithModules { get; set; }
    public TimeOnly? EarliestDepartureTimeWithModules { get; set; }
    public int Status { get; set; }
    public int MeetingType { get; set; }
    public string Details { get; set; }
    public string Accomodation { get; set; }
    public string Food { get; set; }
    public string ExternalLink { get; set; }
    public Guid? AccessKey { get; set; } = Guid.NewGuid();
    public virtual Group OrganiserGroup { get; set; }
    public virtual GroupDomain GroupDomain { get; set; }
    public virtual ICollection<Layout> Layouts { get; set; }
    public virtual ICollection<MeetingParticipant> Participants { get; set; }
}

#nullable enable

public enum MeetingType
{
    ModuleMeeting = 0, // Default
    ClubEvent = 1,
    Market = 2,
}

public static class MeetingExtensions
{
    public static DateTime LatestArrivalDateTimeWithModules(this Meeting meeting) =>
        meeting.LatestArrivalTimeWithModules.HasValue ? meeting.StartDate.Add(meeting.LatestArrivalTimeWithModules.Value.ToTimeSpan()) :
        meeting.StartDate.AddHours(18);

    public static DateTime EarliestDepartureDateTimeWithModules(this Meeting meeting) =>
    meeting.EarliestDepartureTimeWithModules.HasValue ? meeting.EndDate.Add(meeting.EarliestDepartureTimeWithModules.Value.ToTimeSpan()) :
    meeting.EndDate.AddHours(12);

    public static bool ShowTime(this Meeting? meeting) =>
        meeting?.MeetingType != (int)MeetingType.ModuleMeeting;
    public static string Day(this Meeting meeting, int day) =>
    meeting.StartDate.AddDays(day - 1).DayOfWeek.ToString();

    public static int DaysCount(this Meeting meeting) =>
         (meeting.EndDate - meeting.StartDate).Days + 1;

    public static string Organiser(this Meeting? meeting) =>
        meeting is null ? string.Empty :
        meeting.GroupDomainId.HasValue ? $"{meeting.OrganiserGroup.FullName}/{meeting.GroupDomain?.Name}" :
        $"{meeting.OrganiserGroup.FullName}";

    public static bool HasParticipantsFromSeveralCountries(this Meeting? meeting) =>
        meeting is not null && meeting.Participants.Select(p => p.Person.CountryId).Distinct().Count() > 1;

    public static DateTime? RegistrationOpensDate(this Meeting? meeting) =>
     meeting is null || meeting.IsNotPermittingRegistrations() ? null :
     meeting.Layouts.Where(l => l.IsRegistrationPermitted).Min(l => l.RegistrationOpeningDate);

    public static DateTime? RegistrationClosingDate(this Meeting? meeting) =>
        meeting is null || meeting.IsNotPermittingRegistrations() ? null :
        meeting.Layouts.Where(l => l.IsRegistrationPermitted).Max(l => l.RegistrationClosingDate);

    public static DateTime? RegistrationOfModulesClosingDate(this Meeting? meeting) =>
        meeting is null || meeting.IsNotPermittingRegistrations() ? null :
        meeting.Layouts.Where(l => l.IsRegistrationPermitted).Max(l => l.ModuleRegistrationClosingDate ?? l.RegistrationClosingDate);

    public static bool IsNotYetOpenForRegistration([NotNullWhen(true)] this Meeting? meeting, DateTime at) =>
        meeting is not null &&
        meeting.Layouts.All(l => l.IsNotYetOpenForRegistration(at));


    public static bool IsCancelled([NotNullWhen(true)] this Meeting? meeting) =>
        meeting is not null &&
        meeting.Status == (int)MeetingStatus.Canceled;

    private static bool IsNotCancelled([NotNullWhen(true)] this Meeting? meeting) =>
        !meeting.IsCancelled();

    public static bool IsNotPermittingRegistrations([NotNullWhen(true)] this Meeting? meeting) =>
        meeting is null ||
        meeting.IsCancelled() ||
        meeting.Layouts.Count == 0 ||
        meeting.Layouts.All(l => !l.IsRegistrationPermitted);

    public static bool PermitsRegistrations([NotNullWhen(true)] this Meeting? meeting) =>
        meeting is not null && !meeting.IsCancelled() && meeting.Layouts.Any(l => l.IsRegistrationPermitted);

    public static bool IsOpenForRegistration([NotNullWhen(true)] this Meeting? meeting, DateTime at) =>
        meeting is not null &&
        meeting.IsNotCancelled() &&
        meeting.Layouts.Any(l => l.IsRegistrationPermitted && l.IsOpenForRegistration(at));

    public static bool IsClosedForRegistration([NotNullWhen(false)] this Meeting? meeting, DateTime at) =>
        meeting is not null &&
        meeting.Layouts.Count != 0 &&
        meeting.Layouts.All(l => l.IsRegistrationPermitted && l.RegistrationClosingDate <= at);

    public static bool MayDelete(this Meeting meeting, ClaimsPrincipal? principal) =>
        principal.IsCountryOrGlobalAdministrator() && meeting.Layouts.Sum(l => l.LayoutParticipants.Count) == 0;

    public static bool MayRegister(this Meeting meeting, ClaimsPrincipal? principal, DateTime at) =>
        meeting.PermitsRegistrations() &&
            (principal.IsGlobalAdministrator() ||
            principal.IsCountryAdministratorInCountry(meeting.OrganiserGroup.CountryId) ||
            meeting.HasMeetingAdministrator(principal.PersonId()) ||
            meeting.IsOpenForRegistration(at));


    public static string Scales(this Meeting meeting) =>
        string.Join(", ", meeting.Layouts.Select(l => l.PrimaryModuleStandard.Scale.ShortName).Distinct());

    public static MarkupString StartOrEventDate(this Meeting meeting) =>
        new(meeting.StartDate.ToString("d"));

    public static MarkupString EndDateOrTimes(this Meeting meeting) =>
        meeting.ShowTime() && meeting.EndDate.Date == meeting.StartDate.Date ? new($"""<i>{meeting.StartDate:t}-{meeting.EndDate:t}</i>""") :
        new(meeting.EndDate.ToString("d"));

    public static string Duration(this Meeting meeting) =>
        meeting.ShowTime() ? $"{meeting.StartOrEventDate()} {meeting.EndDateOrTimes()}" :
        $"{meeting.StartOrEventDate()} - {meeting.EndDateOrTimes()}";

    public static bool HasMeetingAdministrator(this Meeting meeting, int personId) =>
        meeting.OrganiserGroup is null || meeting.Layouts is null ? false :
        meeting.OrganiserGroup.GroupMembers.Any(gm => gm.IsMeetingAdministrator && gm.PersonId == personId) ||
        meeting.Layouts.Any(l => l.OrganisingGroup?.GroupMembers is not null &&  l.OrganisingGroup.GroupMembers.Any(gm => gm.IsMeetingAdministrator && gm.PersonId == personId));
}

public static class MeetingMapping
{
    internal static void MapMeeting(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<Meeting>(entity =>
        {
            entity.ToTable("Meeting", tb => tb.HasTrigger("DeleteMeeting"));

            entity.HasOne(d => d.OrganiserGroup)
                .WithMany()
                .HasForeignKey(d => d.OrganiserGroupId);

            entity.HasOne(d => d.GroupDomain)
                .WithMany()
                .HasForeignKey(d => d.GroupDomainId);

            entity.HasMany(d => d.Participants)
                .WithOne(d => d.Meeting)
                .HasForeignKey(d => d.MeetingId);
        });
}

