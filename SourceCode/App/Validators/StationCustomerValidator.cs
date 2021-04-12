using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class StationCustomerValidator : AbstractValidator<StationCustomer>
    {
        public StationCustomerValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.CustomerName).NotEmpty().MaximumLength(50).MustBeOrdinaryText(localizer).MustBeCapitalizedCorrectly(localizer).WithName(n => localizer["Name"]);
            RuleFor(m => m.Comment).MaximumLength(50).MustBeCapitalizedCorrectly(localizer).WithName(n => localizer[nameof(n.Comment)]);
            RuleFor(m => m.OpenedYear).MustBeValidYear(localizer).WithName(n => localizer[nameof(n.OpenedYear)]);
            RuleFor(m => m.ClosedYear).MustBeValidYear(localizer).WithName(n => localizer[nameof(n.ClosedYear)]);
            RuleFor(m => m.TrackOrArea).MustBeOrdinaryText(localizer).WithName(n => localizer[nameof(n.TrackOrArea)]);
            RuleFor(m => m.TrackOrAreaColor).MustBeColor(localizer).WithName(n => localizer[nameof(n.TrackOrAreaColor)]);
        }
    }
}
