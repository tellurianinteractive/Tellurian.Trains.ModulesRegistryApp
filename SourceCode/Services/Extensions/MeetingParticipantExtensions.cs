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
}
