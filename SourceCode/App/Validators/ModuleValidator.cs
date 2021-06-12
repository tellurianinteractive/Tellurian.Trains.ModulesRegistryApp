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
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.FullName)]);
            RuleFor(m => m.ScaleId)
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.Scale)]);
            RuleFor(m => m.ConfigurationLabel)
                .MaximumLength(10)
                .MustBeOrdinaryText(localizer)
                .WithName(n => localizer[nameof(n.ConfigurationLabel)]);
            RuleFor(m => m.PackageLabel)
                .MaximumLength(10)
                .MustBeOrdinaryText(localizer)
                .WithName(n => localizer[nameof(n.PackageLabel)]);
            RuleFor(m => m.FremoNumber)
                .InclusiveBetween(1, 9999).When(m => m.FremoNumber.HasValue)
                .WithName(n => localizer[nameof(n.FremoNumber)]);
            RuleFor(m => m.StandardId)
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.Standard)]);
            RuleFor(m => m.Theme)
                .MaximumLength(50)
                .MustBeOrdinaryText(localizer)
                .WithName(n => localizer[nameof(n.Theme)]);
            RuleFor(m => m.RepresentsFromYear)
                .MustBeValidYear(localizer)
                .WithName(n => localizer["FromYear"]);
            RuleFor(m => m.RepresentsUptoYear)
               .MustBeValidYear(localizer)
               .WithName(n => localizer["UptoYear"]);
            RuleFor(m => m.Radius)
                .InclusiveBetween(300.0, 10000.0).When(m => m.Radius is not null)
                .WithName(n => localizer[nameof(n.Radius)]);
            RuleFor(m => m.Angle)
                .InclusiveBetween(1, 360).When(m => m.Angle is not null)
                .WithName(n => localizer[nameof(n.Angle)]);
            RuleFor(m => m.Straight)
                .InclusiveBetween(0, 50000).When(m => m.Straight is not null)
                .WithName(n => localizer[nameof(n.Straight)]);
            RuleFor(m => m.Length)
                .InclusiveBetween(0, 50000)
                .WithName(n => localizer[nameof(n.Length)]);
            RuleFor(m => (int)m.NumberOfThroughTracks)
                .InclusiveBetween(1, 4)
                .WithName(n => localizer[nameof(n.NumberOfThroughTracks)]);
            RuleFor(m => (int?)m.SpeedLimit)
               .InclusiveBetween(10, 200).When(m => m.SpeedLimit.HasValue)
               .WithName(n => localizer[nameof(n.SpeedLimit)]);
            RuleFor(m => m.FunctionalState)
                .InclusiveBetween((int)ModuleFunctionalState.Unknown, (int)ModuleFunctionalState.Approved)
                .WithName(n => localizer[nameof(n.FunctionalState)]);
            RuleFor(m => m.LandscapeState)
                .InclusiveBetween((int)ModuleLandscapeState.Unknown, (int)ModuleLandscapeState.FullyAppliedDetailed)
                .WithName(n => localizer[nameof(n.LandscapeState)]);
            RuleFor(m => m.OverheadLineFeature)
               .InclusiveBetween((int)OverheadLineFeature.No, (int)OverheadLineFeature.OnlyPosts)
               .WithName(n => localizer[nameof(n.OverheadLineFeature)]);
            RuleFor(m => m.SignalFeature)
              .InclusiveBetween((int)SignalFeature.No, (int)SignalFeature.Fixed)
              .WithName(n => localizer[nameof(n.SignalFeature)]);
            RuleFor(m => m.Note)
                .MaximumLength(50)
                .MustBeOrdinaryText(localizer)
                .WithName(n => localizer[nameof(n.Note)]);
        }
    }
}
