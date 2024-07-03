﻿using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class ModuleStandardValidator : AbstractValidator<ModuleStandard>
{
    public ModuleStandardValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.ShortName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(12)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.ShortName)]);
        RuleFor(m => m.ScaleId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Scale)]);
        RuleFor(m => m.TrackSystem)
            .MaximumLength(20)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.TrackSystem)]);
        RuleFor(m => m.NormalGauge)
            .InclusiveBetween(0.0, 500.0)
            .When(m => m.NormalGauge is not null)
            .WithName(n => localizer[nameof(n.NormalGauge)]);
        RuleFor(m => m.NarrowGauge)
            .InclusiveBetween(0.0, 400.0)
            .When(m => m.NarrowGauge is not null)
            .WithName(n => localizer[nameof(n.NarrowGauge)]);
        RuleFor(m => m.Wheelset)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Wheelset)]);
        RuleFor(m => m.Couplings)
            .MaximumLength(20)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Couplings)]);
        RuleFor(m => m.Electricity)
            .MaximumLength(20)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Electricity)]);
        RuleFor(m => m.MainThemeId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.MainTheme)]);
        RuleFor(m => m.PreferredTheme)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.PreferredTheme)]);
        RuleFor(m => m.AcceptedNorm)
            .MaximumLength(255)
            .When(m => m.AcceptedNorm is not null)
            .WithName(n => localizer[nameof(n.AcceptedNorm)]);
    }
}
