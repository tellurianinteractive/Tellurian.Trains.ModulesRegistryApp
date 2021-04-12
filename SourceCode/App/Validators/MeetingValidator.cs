using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using System;

namespace ModulesRegistry.Validators
{
    public class MeetingValidator : AbstractValidator<Meeting>
    {
        public MeetingValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.OrganiserGroupId).MustBeSelected(localizer).WithName(n => localizer["Organiser"]);
            RuleFor(m => m.PlaceName).NotEmpty().MaximumLength(50).MustBeOrdinaryText(localizer).WithName(n => localizer["Venue"]);
            RuleFor(m => m.Description).NotEmpty().MaximumLength(50).MustBeOrdinaryText(localizer).WithName(n => localizer[nameof(n.Description)]);
            RuleFor(m => m.StartDate).NotEmpty().GreaterThan(DateTime.Now).WithName(n => localizer[nameof(n.StartDate)]);
            RuleFor(m => m.EndDate).NotEmpty().GreaterThan(m => m.StartDate).WithName(n => localizer[nameof(n.EndDate)]);
            RuleFor(m => m.Status).MustBeSelected(localizer).WithName(n => localizer[nameof(n.Status)]);
        }
    }
}
