using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using System;

namespace ModulesRegistry.Validators
{
    public class CargoValidator : AbstractValidator<Cargo>
    {
        public CargoValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.DefaultClasses)
                .MinimumLength(1)
                .MaximumLength(10)
                .WithName(n => localizer[nameof(n.DefaultClasses)]);

            RuleFor(m => m.NhmCode)
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.NhmCode)]);

            RuleFor(m => m.FromYear)
                .InclusiveBetween((short)1900, (short)(DateTime.Now.Year) )
                .WithName(n => localizer[nameof(n.FromYear)]);

            RuleFor(m => m.UptoYear)
                .InclusiveBetween((short)1900, (short)(DateTime.Now.Year))
                .WithName(n => localizer[nameof(n.UptoYear)]);

            RuleFor(m => m.EN).MaximumLength(20).WithName(n => localizer["English"]);
            RuleFor(m => m.DE).MaximumLength(20).WithName(n => localizer["German"]);
            RuleFor(m => m.DA).MaximumLength(20).WithName(n => localizer["Danish"]);
            RuleFor(m => m.NL).MaximumLength(20).WithName(n => localizer["Dutch"]);
            RuleFor(m => m.NO).MaximumLength(20).WithName(n => localizer["Norwegian"]);
            RuleFor(m => m.PL).MaximumLength(20).WithName(n => localizer["Polish"]);
            RuleFor(m => m.SV).MaximumLength(20).WithName(n => localizer["Swedish"]);
        }
    }
}
