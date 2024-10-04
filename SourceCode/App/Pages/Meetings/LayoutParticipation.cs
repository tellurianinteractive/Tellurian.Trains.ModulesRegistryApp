using ModulesRegistry.Data;

namespace ModulesRegistry.Pages.Meetings;

public class LayoutParticipation(Layout layout, MeetingParticipant participant)
{
    public Layout Layout { get; } = layout;
    public MeetingParticipant Participant { get; } = participant;
    public bool IsParticipating => LayoutParticipant is not null;
    public LayoutParticipant? LayoutParticipant => Participant.LayoutParticipations.SingleOrDefault(lp => lp.LayoutId == Layout.Id);
       
}
