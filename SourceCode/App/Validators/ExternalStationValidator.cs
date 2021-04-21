using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class ExternalStationValidator : AbstractValidator<ExternalStation>
    {
        public ExternalStationValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.FullName)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(50)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.FullName)]);
            RuleFor(m => m.Signature)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(5)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.Signature)]);
            RuleFor(m => m.OpenedYear)
                .MustBeValidYear(localizer)
                .WithName(n => localizer[nameof(n.OpenedYear)]);
            RuleFor(m => m.ClosedYear)
                .MustBeValidYear(localizer)
                .WithName(n => localizer[nameof(n.ClosedYear)]);
        }
    }
}
