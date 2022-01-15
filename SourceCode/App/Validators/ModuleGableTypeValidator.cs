using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class ModuleGableTypeValidator : AbstractValidator<ModuleGableType>
{
    public ModuleGableTypeValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.Designation)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(20)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Designation)]);
        RuleFor(m => m.ScaleId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Scale)]);
    }
}
