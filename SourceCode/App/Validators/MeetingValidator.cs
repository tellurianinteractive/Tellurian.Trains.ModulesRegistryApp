using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class MeetingValidator : AbstractValidator<Meeting>
{
    public const int DetailsMaxLength = 2000;
    public const int AccomodationMaxLenght = 1000;
    public const int FoodMaxLength = 1000;
    public MeetingValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.OrganiserGroupId)
            .MustBeSelected(localizer)
            .WithName(n => localizer["Organiser"]);
        RuleFor(m => m.CityName)
            .NotEmpty()
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.CityName)]);
        RuleFor(m => m.VenueName)
           .MaximumLength(50)
           .MustBeOrdinaryText(localizer)
           .WithName(n => localizer[nameof(n.VenueName)]);
        RuleFor(m => m.Name)
            .NotEmpty()
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Name)]);
        RuleFor(m => m.Details)
            .MaximumLength(DetailsMaxLength)
            .WithName(n => localizer[nameof(n.Details)]);
        RuleFor(m => m.Accomodation)
            .MaximumLength(AccomodationMaxLenght)
            .WithName(n => localizer[nameof(n.Accomodation)]);
        RuleFor(m => m.Food)
           .MaximumLength(FoodMaxLength)
           .WithName(n => localizer[nameof(n.Food)]);
        RuleFor(m => m.StartDate)
            .NotEmpty()
            .GreaterThan(DateTime.Now.AddDays(-7))
            .WithName(n => localizer[nameof(n.StartDate)]);
        RuleFor(m => m.EndDate)
            .NotEmpty()
            .GreaterThanOrEqualTo(m => m.StartDate)
            .WithName(n => localizer[nameof(n.EndDate)]);
        RuleFor(m => m.LatestArrivalTimeWithModules)
            .NotNull()
            .WithName(n => localizer[nameof(n.LatestArrivalTimeWithModules)]);
        RuleFor(m => m.EarliestDepartureTimeWithModules)
            .NotNull()
            .WithName(n => localizer[nameof(n.EarliestDepartureTimeWithModules)]);
        RuleFor(m => m.Status)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Status)]);
    }
}
