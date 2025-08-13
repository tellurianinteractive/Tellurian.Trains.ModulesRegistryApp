#nullable disable

using Microsoft.EntityFrameworkCore;
using System.Security;

namespace ModulesRegistry.Data;

public class MeetingParticipant
{
    public MeetingParticipant()
    {
        LayoutParticipations = new HashSet<LayoutParticipant>();
    }
    public int Id { get; set; }
    public int PersonId { get; set; }
    public int MeetingId { get; set; }
    public bool ParticipateDay1 { get; set; }
    public bool ParticipateDay2 { get; set; }
    public bool ParticipateDay3 { get; set; }
    public bool ParticipateDay4 { get; set; }
    public bool ParticipateDay5 { get; set; }
    public bool ParticipateDay6 { get; set; }
    public TimeOnly? LatestArrivalTime { get; set; }
    public TimeOnly? EarliestDepartureTime { get; set; }

    public DateTimeOffset RegistrationTime { get; set; }
    public DateTimeOffset? CancellationTime { get; set; }

    public virtual Person Person { get; set; }
    public virtual Meeting Meeting { get; set; }
    public virtual ICollection<LayoutParticipant> LayoutParticipations { get; set; }
}

public static class MeetingParticipantMapping
{
    public static void MapMeetingParticipant(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<MeetingParticipant>(entity =>
        {
            entity.ToTable("MeetingParticipant", tb => tb.HasTrigger("DeleteMeetingParticipant"));

            entity.HasOne(d => d.Meeting)
                .WithMany(d => d.Participants)
                .HasForeignKey(d => d.MeetingId);

            entity.HasOne(d => d.Person)
                .WithMany()
                .HasForeignKey(d => d.PersonId);

            entity.HasMany(d => d.LayoutParticipations)
                .WithOne(d => d.MeetingParticipant)
                .HasForeignKey(d => d.MeetingParticipantId);
        });
}

#nullable enable

public static class MeetingParticipantExtensions
{
    public static bool IsCancelled(this MeetingParticipant participant) => participant.CancellationTime.HasValue;

    public static bool IsParticipating(this MeetingParticipant participant) => !participant.IsCancelled();
    public static DateTime FirstParticpationDate(this MeetingParticipant participant) =>
        participant.ParticipateDay1 ? participant.Meeting.StartDate :
        participant.ParticipateDay2 ? participant.Meeting.StartDate.AddDays(1) :
        participant.ParticipateDay3 ? participant.Meeting.StartDate.AddDays(2) :
        participant.ParticipateDay4 ? participant.Meeting.StartDate.AddDays(3) :
        participant.ParticipateDay5 ? participant.Meeting.StartDate.AddDays(4) :
        participant.Meeting.StartDate.AddDays(5);

    public static DateTime LastParticpationDate(this MeetingParticipant participant) =>
        participant.ParticipateDay6 ? participant.Meeting.StartDate.AddDays(5) :
        participant.ParticipateDay5 ? participant.Meeting.StartDate.AddDays(4) :
        participant.ParticipateDay4 ? participant.Meeting.StartDate.AddDays(3) :
        participant.ParticipateDay3 ? participant.Meeting.StartDate.AddDays(2) :
        participant.ParticipateDay2 ? participant.Meeting.StartDate.AddDays(1) :
        participant.Meeting.StartDate;

    public static bool MayRegisterModules(this MeetingParticipant? participant) =>
        participant is not null && 
        participant.Meeting is not null && 
        participant.ArrivalDateTime() <=  participant.Meeting.LatestArrivalDateTimeWithModules() &&
        participant.DepartureDateTime() >= participant.Meeting.EarliestDepartureDateTimeWithModules();

    public static DateTime ArrivalDateTime(this MeetingParticipant participant) =>
        participant.LatestArrivalTime.HasValue ? participant.FirstParticpationDate().Add(participant.LatestArrivalTime.Value.ToTimeSpan()) :
        participant.FirstParticpationDate();

    public static DateTime DepartureDateTime(this MeetingParticipant participant) =>
        participant.EarliestDepartureTime.HasValue ? participant.LastParticpationDate().Add(participant.EarliestDepartureTime.Value.ToTimeSpan()) :
        participant.LastParticpationDate();

    public static string NameWithCity(this MeetingParticipant participant) =>
        participant.Person is not null ? participant.Person.NameWithCity() : string.Empty;

    public static MeetingParticipant DefaultParticipant(this Meeting meeting, int personId)
    {
        var participant = new MeetingParticipant
        {
            MeetingId = meeting.Id,
            PersonId = personId,
            LatestArrivalTime = meeting.LatestArrivalTimeWithModules,
            EarliestDepartureTime = meeting.EarliestDepartureTimeWithModules,
        };
        participant.SetDefaultParticipationDays(meeting);
        if (meeting.Layouts.Count == 1)
        {
            participant.LayoutParticipations.Add(new LayoutParticipant() { LayoutId = meeting.Layouts.First().Id, PersonId = personId });
        }
        return participant;
    }

    private static void SetDefaultParticipationDays(this MeetingParticipant participant, Meeting meeting)
    {
        var days = meeting.DurationDays();
        if (days > 0) participant.ParticipateDay1 = true;
        if (days > 1) participant.ParticipateDay2 = true;
        if (days > 2) participant.ParticipateDay3 = true;
        if (days > 3) participant.ParticipateDay4 = true;
        if (days > 4) participant.ParticipateDay5 = true;
        if (days > 5) participant.ParticipateDay6 = true;
    }

}


