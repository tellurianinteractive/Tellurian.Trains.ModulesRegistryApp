using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using System;

namespace ModulesRegistry.Validators
{
    public class CargoValidator : AbstractValidator<Cargo>
    {
        const int TranslatedLength = 25;
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

            RuleFor(m => m.EN).MaximumLength(TranslatedLength).WithName(n => localizer["English"]);
            RuleFor(m => m.DE).MaximumLength(TranslatedLength).WithName(n => localizer["German"]);
            RuleFor(m => m.DA).MaximumLength(TranslatedLength).WithName(n => localizer["Danish"]);
            RuleFor(m => m.NL).MaximumLength(TranslatedLength).WithName(n => localizer["Dutch"]);
            RuleFor(m => m.NO).MaximumLength(TranslatedLength).WithName(n => localizer["Norwegian"]);
            RuleFor(m => m.PL).MaximumLength(TranslatedLength).WithName(n => localizer["Polish"]);
            RuleFor(m => m.SV).MaximumLength(TranslatedLength).WithName(n => localizer["Swedish"]);
        }
    }
}
