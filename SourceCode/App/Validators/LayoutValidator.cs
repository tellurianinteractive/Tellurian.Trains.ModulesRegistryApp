using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class LayoutValidator : AbstractValidator<Layout>
{
    public const int MaxNoteLength = 2000;
    public LayoutValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.MeetingId)
            .MustBeSelected(localizer)
            .WithName(n => localizer["Meeting"]);
        RuleFor(m => m.ShortName)
            .MaximumLength(10)
            .MustBeOrdinaryText(localizer);
        RuleFor(m => m.OrganisingGroupId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.OrganisingGroup)]);
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
        RuleFor(m => m.Details)
            .MaximumLength(MaxNoteLength)
            .WithName(n => localizer[nameof(n.Details)]);
        RuleFor(m => m.FirstYear)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.FirstYear)]);
        RuleFor(m => m.LastYear)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.LastYear)]);
        RuleFor(m => m.MaxNumberOfParticipants)
            .Empty().When(m => m.MaxNumberOfParticipants is null)
            .InclusiveBetween(1, 200);
    }
}
