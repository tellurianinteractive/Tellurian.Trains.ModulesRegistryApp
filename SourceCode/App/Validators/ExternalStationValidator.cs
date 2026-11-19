using ModulesRegistry.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Validators;

public class ExternalStationValidator : AbstractValidator<ExternalStation>
{
    public ExternalStationValidator(IStringLocalizer<Strings> localizer)
    {
        RuleFor(m => m.RegionId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Region)]);
        RuleFor(m => m.FullName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.FullName)]);
        RuleFor(m => m.InternationalName)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.FullName)]);
        RuleFor(m => m.Signature)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(5)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Signature)]);
        RuleFor(m => m.OpenedYear)
            .MustBeYearOrEmpty(localizer)
            .WithName(n => localizer[nameof(n.OpenedYear)]);
        RuleFor(m => m.ClosedYear)
            .MustBeYearOrEmpty(localizer)
            .WithName(n => localizer[nameof(n.ClosedYear)]);
    }
}
