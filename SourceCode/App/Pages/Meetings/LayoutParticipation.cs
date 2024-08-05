using ModulesRegistry.Data;

namespace ModulesRegistry.Pages.Meetings;

public class LayoutParticipation(Layout layout, MeetingParticipant participant)
{
    public Layout Layout { get; } = layout;
    public MeetingParticipant Participant { get; } = participant;
    public bool IsParticipating { get; set; }

    public LayoutParticipant? AsLayoutParticipant =>
        Layout is null || Participant is null || !IsParticipating ? null :
        new LayoutParticipant { Layout = Layout, MeetingParticipant = Participant };
}
