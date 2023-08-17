#nullable disable

using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data;

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
    public int Status { get; set; }
    public string Details { get; set; }
    public string Accomodation { get; set; }
    public string Food { get; set; }
    public virtual Group OrganiserGroup { get; set; }
    public virtual GroupDomain GroupDomain { get; set; }
    public virtual ICollection<Layout> Layouts { get; set; }
    public virtual ICollection<MeetingParticipant> Participants { get; set; }
}

#nullable enable

public static class MeetingExtensions
{
    public static string Day(this Meeting meeting, int day) =>
    meeting.StartDate.AddDays(day - 1).DayOfWeek.ToString();

    public static int DaysCount(this Meeting meeting) =>
         (meeting.EndDate - meeting.StartDate).Days + 1;

    public static string Organiser(this Meeting? meeting) =>
        meeting is null ? string.Empty :
        meeting.GroupDomainId.HasValue ? $"{meeting.OrganiserGroup.FullName}/{meeting.GroupDomain?.Name}" :
        $"{meeting.OrganiserGroup.FullName}";


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

    public static bool IsOpenForRegistration([NotNullWhen(true)] this Meeting? meeting, DateTime at) =>
        meeting is not null &&
        meeting.IsNotCancelled() &&
        meeting.Layouts.Any(l => l.IsRegistrationPermitted && l.IsOpenForRegistration(at));

    public static bool IsClosedForRegistration([NotNullWhen(false)] this Meeting? meeting, DateTime at) =>
        meeting is not null &&
        meeting.Layouts.Any() &&
        meeting.Layouts.All(l => l.IsRegistrationPermitted && l.RegistrationClosingDate <= at);
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

