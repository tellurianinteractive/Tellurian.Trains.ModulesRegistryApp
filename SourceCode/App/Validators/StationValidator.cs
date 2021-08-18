using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class StationValidator : AbstractValidator<Station>
    {
        public StationValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.FullName)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(50)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer, false)
                .WithName(n => localizer[nameof(n.FullName)]);
            RuleFor(m => m.Signature)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(5)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.Signature)]);
        }
    }
}
