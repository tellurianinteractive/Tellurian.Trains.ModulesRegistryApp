namespace ModulesRegistry.Services.Extensions;
public static class MeetingParticipantExtensions
{
    public static string FirstParticipationDay(this MeetingParticipant? me) =>
        me is null || me.Meeting is null ? string.Empty :
        me.ParticipateDay1 ? me.Meeting.StartDate.DayOfWeek.ToString().Localized() :
        me.ParticipateDay2 ? me.Meeting.StartDate.AddDays(1).DayOfWeek.ToString().Localized() :
        me.ParticipateDay3 ? me.Meeting.StartDate.AddDays(2).DayOfWeek.ToString().Localized() :
        me.ParticipateDay4 ? me.Meeting.StartDate.AddDays(3).DayOfWeek.ToString().Localized() :
        me.ParticipateDay5 ? me.Meeting.StartDate.AddDays(4).DayOfWeek.ToString().Localized() :
        string.Empty;
}
