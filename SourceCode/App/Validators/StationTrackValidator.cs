using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class StationTrackValidator : AbstractValidator<StationTrack>
{
    public StationTrackValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.StationId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Station)]);
        RuleFor(m => m.Designation)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(5)
            .MustBeOrdinaryText(localizer)
           .WithName(n => localizer["TrackNumber"]);
        RuleFor(m => (int)m.DisplayOrder)
            .InclusiveBetween(1, 30)
            .WithName(n => localizer[nameof(n.DisplayOrder)]);
        RuleFor(m => m.MaxTrainLength)
            .NotEmpty()
            .InclusiveBetween(0.2, 10.0)
            .WithName(n => localizer[nameof(n.MaxTrainLength)]);
        RuleFor(m => m.PlatformLength)
            .InclusiveBetween(0.2, 10.0)
            .WithName(n => localizer[nameof(n.PlatformLength)]);
        RuleFor(m => (int?)m.SpeedLimit)
            .InclusiveBetween(5, 350)
            .WithName(n => localizer[nameof(n.SpeedLimit)]);
        RuleFor(m => m.UsageNote)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer["TrackNote"]);
        RuleFor(m => m.DirectionId).
            MustBeEnumValue(localizer, typeof(StationTrackDirection)).
            WithName(n => localizer["Direction"]);
    }
}
