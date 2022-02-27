using ModulesRegistry.Data;

namespace ModulesRegistry.Models;

public class LayoutParticipation
{
    public LayoutParticipation(Layout layout, MeetingParticipant participant)
    {
        Layout = layout;
        Participant = participant;
    }
    public Layout Layout { get;  }
    public MeetingParticipant Participant { get;  }
    public bool IsParticipating { get; set; }

    public LayoutParticipant? AsLayoutParticipant =>
        Layout is null || Participant is null || !IsParticipating ? null :
        new LayoutParticipant { Layout = Layout, MeetingParticipant = Participant };
}
