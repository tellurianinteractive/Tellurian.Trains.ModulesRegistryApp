using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Resources;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Meetings;

/// <summary>
/// A meeting participant's days and times (Migration step 3.6). The meeting and the person come from the page's address. At least one
/// day must be checked (server only); the days between the first and last checked day are filled in when saved, as the interactive
/// form did when a box was checked. The days have no display names: their labels are the meeting's days.
/// </summary>
[ValidatableType]
public class MeetingParticipantFormData : IValidatableObject
{
    public bool Day1 { get; set; }
    public bool Day2 { get; set; }
    public bool Day3 { get; set; }
    public bool Day4 { get; set; }
    public bool Day5 { get; set; }
    public bool Day6 { get; set; }

    [Display(Name = "LatestArrivalTime")]
    [Required]
    public TimeOnly? LatestArrivalTime { get; set; }

    [Display(Name = "EarliestDepartureTime")]
    [Required]
    public TimeOnly? EarliestDepartureTime { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!(Day1 || Day2 || Day3 || Day4 || Day5 || Day6))
            yield return new ValidationResult(Strings.SelectAtLeastOneDay, [nameof(Day1)]);
    }
}

public static class MeetingParticipantFormExtensions
{
    extension(MeetingParticipant entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public MeetingParticipantFormData AsFormData() => new()
        {
            Day1 = entity.ParticipateDay1,
            Day2 = entity.ParticipateDay2,
            Day3 = entity.ParticipateDay3,
            Day4 = entity.ParticipateDay4,
            Day5 = entity.ParticipateDay5,
            Day6 = entity.ParticipateDay6,
            LatestArrivalTime = entity.LatestArrivalTime,
            EarliestDepartureTime = entity.EarliestDepartureTime,
        };
    }

    extension(MeetingParticipantFormData form)
    {
        /// <summary>
        /// Form data → the loaded or new participant: only the days and times change, and the days between the first and last checked
        /// day are filled in. Call it only with validated form data.
        /// </summary>
        public MeetingParticipant AsEntity(MeetingParticipant participant)
        {
            participant.ParticipateDay1 = form.Day1;
            participant.ParticipateDay2 = form.Day2;
            participant.ParticipateDay3 = form.Day3;
            participant.ParticipateDay4 = form.Day4;
            participant.ParticipateDay5 = form.Day5;
            participant.ParticipateDay6 = form.Day6;
            participant.LatestArrivalTime = form.LatestArrivalTime;
            participant.EarliestDepartureTime = form.EarliestDepartureTime;
            participant.FillParticipationDaysBetween();
            return participant;
        }
    }
}