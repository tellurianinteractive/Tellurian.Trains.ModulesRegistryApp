namespace ModulesRegistry.Data;

/// <summary>
/// The participation days of a <see cref="MeetingParticipant"/> by day number (1 = the meeting's first day), without needing the meeting.
/// </summary>
public static class MeetingParticipantDaysExtensions
{
    public const int MaxDays = 6;

    extension(MeetingParticipant participant)
    {
        public bool ParticipatesDay(int day) => day switch
        {
            1 => participant.ParticipateDay1,
            2 => participant.ParticipateDay2,
            3 => participant.ParticipateDay3,
            4 => participant.ParticipateDay4,
            5 => participant.ParticipateDay5,
            6 => participant.ParticipateDay6,
            _ => false
        };

        public void SetParticipatesDay(int day, bool participates)
        {
            switch (day)
            {
                case 1: participant.ParticipateDay1 = participates; break;
                case 2: participant.ParticipateDay2 = participates; break;
                case 3: participant.ParticipateDay3 = participates; break;
                case 4: participant.ParticipateDay4 = participates; break;
                case 5: participant.ParticipateDay5 = participates; break;
                case 6: participant.ParticipateDay6 = participates; break;
            }
        }

        /// <summary>The first day number the participant participates, or 0 if none.</summary>
        public int FirstParticipationDayNumber() =>
            Enumerable.Range(1, MaxDays).FirstOrDefault(participant.ParticipatesDay);

        /// <summary>The last day number the participant participates, or 0 if none.</summary>
        public int LastParticipationDayNumber() =>
            Enumerable.Range(1, MaxDays).LastOrDefault(participant.ParticipatesDay);

        /// <summary>
        /// Checks every day between the first and the last checked day, since participation is continuous:
        /// checking Thursday and Sunday also checks Friday and Saturday.
        /// </summary>
        public void FillParticipationDaysBetween()
        {
            var first = participant.FirstParticipationDayNumber();
            var last = participant.LastParticipationDayNumber();
            if (first == 0) return;
            for (var day = first; day <= last; day++) participant.SetParticipatesDay(day, true);
        }

        /// <summary>Checks all days of a meeting with <paramref name="daysCount"/> days.</summary>
        public void ParticipateAllDays(int daysCount)
        {
            for (var day = 1; day <= Math.Min(daysCount, MaxDays); day++) participant.SetParticipatesDay(day, true);
        }
    }
}