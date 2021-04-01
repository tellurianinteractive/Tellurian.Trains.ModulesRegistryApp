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
                .IsOrdinaryText(localizer)
                .NameIsCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.FullName)]);
            RuleFor(m => m.Signature)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(5)
                .IsOrdinaryText(localizer)
                .NameIsCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.Signature)]);
        }
    }
}
