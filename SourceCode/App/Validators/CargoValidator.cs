using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using System;

namespace ModulesRegistry.Validators
{
    public class CargoValidator : AbstractValidator<Cargo>
    {
        private const int TranslatedLength = 30;
        public CargoValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.DefaultClasses)
                .MinimumLength(1)
                .MaximumLength(10)
                .MustBeOrdinaryText(localizer)
                .WithName(n => localizer[nameof(n.DefaultClasses)]);

            RuleFor(m => m.NhmCode)
               .GreaterThanOrEqualTo(0)
               .WithName(n => localizer[nameof(n.NhmCode)]);

            RuleFor(m => m.FromYear)
                .InclusiveBetween((short)1900, (short)(DateTime.Now.Year))
                .WithName(n => localizer[nameof(n.FromYear)]);

            RuleFor(m => m.UptoYear)
                .InclusiveBetween((short)1900, (short)(DateTime.Now.Year))
                .WithName(n => localizer[nameof(n.UptoYear)]);

            RuleFor(m => m.EN).NotEmpty().MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["English"]);
            RuleFor(m => m.DE).NotEmpty().MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["German"]);
            RuleFor(m => m.DA).NotEmpty().MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["Danish"]);
            RuleFor(m => m.FR).MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["French"]);
            RuleFor(m => m.NL).MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["Dutch"]);
            RuleFor(m => m.NO).NotEmpty().MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["Norwegian"]);
            RuleFor(m => m.PL).MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["Polish"]);
            RuleFor(m => m.SV).NotEmpty().MaximumLength(TranslatedLength).MustBeOrdinaryText(localizer).WithName(localizer["Swedish"]);
        }
    }
}
