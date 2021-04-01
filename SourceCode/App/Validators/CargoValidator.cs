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
                .IsOrdinaryText(localizer)
                .WithName(n => localizer[nameof(n.DefaultClasses)]);

            // Temporary disabled to simplify entering of data.
            //RuleFor(m => m.NhmCode)
            //    .MustBeSelected(localizer)
            //    .WithName(n => localizer[nameof(n.NhmCode)]);

            RuleFor(m => m.FromYear)
                .InclusiveBetween((short)1900, (short)(DateTime.Now.Year) )
                .WithName(n => localizer[nameof(n.FromYear)]);

            RuleFor(m => m.UptoYear)
                .InclusiveBetween((short)1900, (short)(DateTime.Now.Year))
                .WithName(n => localizer[nameof(n.UptoYear)]);

            RuleFor(m => m.EN).MaximumLength(TranslatedLength).IsOrdinaryText(localizer).WithName(n => localizer["English"]);
            RuleFor(m => m.DE).MaximumLength(TranslatedLength).IsOrdinaryText(localizer).WithName(n => localizer["German"]);
            RuleFor(m => m.DA).MaximumLength(TranslatedLength).IsOrdinaryText(localizer).WithName(n => localizer["Danish"]);
            RuleFor(m => m.NL).MaximumLength(TranslatedLength).IsOrdinaryText(localizer).WithName(n => localizer["Dutch"]);
            RuleFor(m => m.NO).MaximumLength(TranslatedLength).IsOrdinaryText(localizer).WithName(n => localizer["Norwegian"]);
            RuleFor(m => m.PL).MaximumLength(TranslatedLength).IsOrdinaryText(localizer).WithName(n => localizer["Polish"]);
            RuleFor(m => m.SV).MaximumLength(TranslatedLength).IsOrdinaryText(localizer).WithName(n => localizer["Swedish"]);
        }
    }
}
