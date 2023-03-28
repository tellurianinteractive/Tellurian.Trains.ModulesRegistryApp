using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class OperatorValidator : AbstractValidator<Operator>
{
    public OperatorValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.Signature)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(6)
            .MustBeCapitalizedCorrectly(localizer, true)
            .WithName(n => localizer[nameof(n.Signature)]);

        RuleFor(m => m.FullName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .MustBeCapitalizedCorrectly(localizer, false)
            .WithName(n => localizer[nameof(n.FullName)]);
        RuleFor(m => m.PrimaryOperatingCountryId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.PrimaryOperatingCountry)]);
        RuleFor(m => m.FirstYearInOperation)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.FirstYearInOperation)]);
        RuleFor(m => m.FinalYearInOperation)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.FinalYearInOperation)]);
    }
}
