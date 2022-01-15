using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class RegionValidator : AbstractValidator<Region>
{
    public RegionValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.CountryId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Country)]);
        RuleFor(m => m.Description)
            .MustBeOrdinaryTextOrNull(localizer)
            .WithName(n => localizer[nameof(n.Description)]);
        RuleFor(m => m.LocalName)
            .NotEmpty()
            .MustBeCapitalizedCorrectly(localizer)
            .WithName(n => localizer["Name"]);
        RuleFor(m => m.BackColor)
            .MustBeColor(localizer)
            .WithName(n => localizer[nameof(n.BackColor)]);
    }
}
