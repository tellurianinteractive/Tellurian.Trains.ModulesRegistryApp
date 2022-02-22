namespace ModulesRegistry.Data;
public class LayoutParticipation
{
    public Layout? Layout { get; set; }
    public MeetingParticipant? Participant { get; set; }
    public bool IsParticipating { get; set; }

    public LayoutParticipant? AsLayoutParticipant =>
        Layout is null || Participant is null || !IsParticipating ? null :
        new LayoutParticipant { Layout = Layout, MeetingParticipant = Participant };
}
