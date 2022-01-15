#nullable disable

namespace ModulesRegistry.Data;

public class LayoutParticipant
{
    public LayoutParticipant()
    {
        LayoutModules = new HashSet<LayoutModule>();
        LayoutStations = new HashSet<LayoutStation>();
    }
    public int Id { get; set; }
    public int MeetingParticipantId { get; set; }
    public int LayoutId { get; set; }
    public int PersonId { get; set; }
    public virtual MeetingParticipant MeetingParticipant { get; set; }
    public virtual Layout Layout { get; set; }
    public virtual Person Person { get; set; }

    public virtual ICollection<LayoutModule> LayoutModules { get; set; }
    public virtual ICollection<LayoutStation> LayoutStations { get; set; }

}

