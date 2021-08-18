using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class ExternalStationCustomerValidator : AbstractValidator<ExternalStationCustomer>
    {
        public ExternalStationCustomerValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.CustomerName)
                .NotEmpty()
                .MinimumLength(1)
                .MaximumLength(50)
                .MustBeOrdinaryText(localizer)
                .MustBeCapitalizedCorrectly(localizer, false)
                .WithName(n => localizer["Name"]);
            RuleFor(m => m.OpenedYear)
                .MustBeValidYear(localizer)
                .WithName(n => localizer[nameof(n.OpenedYear)]);
            RuleFor(m => m.ClosedYear)
                .MustBeValidYear(localizer)
                .WithName(n => localizer[nameof(n.ClosedYear)]);
        }
    }
}
