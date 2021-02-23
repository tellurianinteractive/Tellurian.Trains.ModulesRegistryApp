using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class ModuleValidator : AbstractValidator<Module>
    {
        public ModuleValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.FullName)
                .NotEmpty()
                .MinimumLength(3)
                .MaximumLength(50)
                .NameIsCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.FullName)]);
            RuleFor(m => m.ScaleId)
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.Scale)]);
            RuleFor(m => m.FREMONumber)
                .InclusiveBetween(1,9999)
                .When(m => m.FREMONumber.HasValue)
                .WithName(n => localizer[nameof(n.FREMONumber)]);
            RuleFor(m => m.StandardId)
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.Standard)]);
            RuleFor(m => m.RepresentsFromYear)
                .MustBeValidYear(localizer)
                .WithName(n => localizer["FromYear"]);
            RuleFor(m => m.RepresentsUptoYear)
               .MustBeValidYear(localizer)
               .WithName(n => localizer["UptoYear"]);
            RuleFor(m => m.Radius)
                .InclusiveBetween(0.0, 10.0)
                .When(m => m.Radius is not null)
                .WithName(n => localizer[nameof(n.Radius)]);
            RuleFor(m => m.Angle)
                .InclusiveBetween(0, 360)
                .When(m => m.Angle is not null)
                .WithName(n => localizer[nameof(n.Angle)]);
            RuleFor(m => m.Length)
                .InclusiveBetween(0, 50)
                .WithName(n => localizer[nameof(n.Length)]);
            RuleFor(m => (int)m.NumberOfThroughTracks)
                .InclusiveBetween(1, 4)
                .WithName(n => localizer[nameof(n.NumberOfThroughTracks)]);
            RuleFor(m => m.FunctionalState)
                .IsInEnum()
                .WithName(n => localizer[nameof(n.FunctionalState)]);
            RuleFor(m => m.LandscapeState)
                .IsInEnum()
                .WithName(n => localizer[nameof(n.LandscapeState)]);
        }
    }
}
