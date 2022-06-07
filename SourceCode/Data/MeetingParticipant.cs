#nullable disable

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
    public DateTime? ArrivalTime { get; set; }

    public DateTimeOffset RegistrationTime { get; set; }
    public DateTimeOffset? CancellationTime { get; set; }

    public virtual Person Person { get; set; }
    public virtual Meeting Meeting { get; set; }
    public virtual ICollection<LayoutParticipant> LayoutParticipations { get; set; }
}
