using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class GroupValidator : AbstractValidator<Group>
    {
        public GroupValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(group => group.ShortName)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(10)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.ShortName)]);

            RuleFor(group => group.FullName)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(50)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.FullName)]);

            RuleFor(group => group.CityName)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(50)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer)
                .WithName(n => localizer[nameof(n.CityName)]);

            RuleFor(group => group.Category)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(20)
                .WithName(n => localizer[nameof(n.Category)]);

            RuleFor(group => group.CountryId)
                .MustBeSelected(localizer)
                .WithName(n => localizer[nameof(n.Country)]);

        }
    }
}
