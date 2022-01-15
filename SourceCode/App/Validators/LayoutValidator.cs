using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class LayoutValidator : AbstractValidator<Layout>
{
    public LayoutValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.MeetingId)
            .MustBeSelected(localizer)
            .WithName(n => localizer["Meeting"]);
        RuleFor(m => m.ResponsibleGroupId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.ResponsibleGroup)]);
        RuleFor(m => m.PrimaryModuleStandardId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.PrimaryModuleStandard)]);
        RuleFor(m => m.RegistrationOpeningDate)
            .NotEmpty()
            .WithName(n => localizer["RegistrationOpens"]);
        RuleFor(m => m.RegistrationClosingDate)
            .NotEmpty()
            .WithName(n => localizer["RegistrationCloses"]);
        RuleFor(m => m.Theme)
            .MaximumLength(100)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Theme)]);
        RuleFor(m => m.Note)
            .MaximumLength(100)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.Note)]);
        RuleFor(m => m.FirstYear)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.FirstYear)]);
        RuleFor(m => m.LastYear)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.LastYear)]);
        RuleFor(m => m.StartHour)
            .MustBeValidHour(localizer)
            .WithName(n => localizer[nameof(n.StartHour)]);
        RuleFor(m => m.EndHour)
            .MustBeValidHour(localizer)
            .WithName(n => localizer[nameof(n.EndHour)]);
    }
}
