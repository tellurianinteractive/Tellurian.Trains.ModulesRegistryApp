using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class MeetingParticipantValidator : AbstractValidator<MeetingParticipant>
    {
        public MeetingParticipantValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.PersonId)
                .MustBeSelected(localizer)
                .WithName(n => localizer["Participant"]);
            RuleFor(m => m.MeetingId)
                .MustBeSelected(localizer)
                .WithName(n => localizer["Meeting"]);
            RuleFor(m => m.LatestArrivalTime)
                .NotEmpty()
                .WithName(n => localizer["LatestArrivalTime"]); ;
            RuleFor(m => m.EarliestDepartureTime)
               .NotEmpty()
               .WithName(n => localizer["EarliestDepartureTime"]); ;
        }
    }
}
