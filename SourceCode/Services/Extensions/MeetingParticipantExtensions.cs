using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Services.Extensions;
public static class MeetingParticipantExtensions
{
    public static string FirstParticipationDay(this MeetingParticipant? participant) =>
        participant is null || participant.Meeting is null ? string.Empty :
        participant.ParticipateDay1 ? participant.Meeting.StartDate.DayOfWeek.ToString().Localized() :
        participant.ParticipateDay2 ? participant.Meeting.StartDate.AddDays(1).DayOfWeek.ToString().Localized() :
        participant.ParticipateDay3 ? participant.Meeting.StartDate.AddDays(2).DayOfWeek.ToString().Localized() :
        participant.ParticipateDay4 ? participant.Meeting.StartDate.AddDays(3).DayOfWeek.ToString().Localized() :
        participant.ParticipateDay5 ? participant.Meeting.StartDate.AddDays(4).DayOfWeek.ToString().Localized() :
        string.Empty;


    public static string[] Days(this MeetingParticipant mp, Meeting m)
    {
        List<string> dayNames = [];
        if (mp.ParticipateDay1) dayNames.Add(m.Day(1).AsLocalized());
        if (mp.ParticipateDay2) dayNames.Add(m.Day(2).AsLocalized());
        if (mp.ParticipateDay3) dayNames.Add(m.Day(3).AsLocalized());
        if (mp.ParticipateDay4) dayNames.Add(m.Day(4).AsLocalized());
        if (mp.ParticipateDay5) dayNames.Add(m.Day(5).AsLocalized());
        return [.. dayNames];
    }


}
