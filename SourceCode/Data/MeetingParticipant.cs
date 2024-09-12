#nullable disable

using Microsoft.EntityFrameworkCore;

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
    [Obsolete("Use LatestArrivalTime")]
    public DateTime? ArrivalTime { get; set; }
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

