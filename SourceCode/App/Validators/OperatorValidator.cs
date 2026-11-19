using ModulesRegistry.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Validators;

public class OperatorValidator : AbstractValidator<Operator>
{
    public OperatorValidator(IStringLocalizer<Strings> localizer)
    {
        RuleFor(m => m.Signature)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(6)
            .WithName(n => localizer[nameof(n.Signature)]);

        RuleFor(m => m.FullName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.FullName)]);
        RuleFor(m => m.PrimaryOperatingCountryId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.PrimaryOperatingCountry)]);
        RuleFor(m => m.FirstYearInOperation)
            .MustBeYearOrEmpty(localizer)
            .WithName(n => localizer[nameof(n.FirstYearInOperation)]);
        RuleFor(m => m.FinalYearInOperation)
            .MustBeYearOrEmpty(localizer)
            .WithName(n => localizer[nameof(n.FinalYearInOperation)]);
    }
}
