﻿using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class ModuleExitValidator : AbstractValidator<ModuleExit>
{
    public ModuleExitValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.Label)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(20)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer["Direction"]);
        RuleFor(m => m.EndProfileId)
            .MustBeSelected(localizer)
            .WithName(n => localizer["EndProfile"]);
    }
}
