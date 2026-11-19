using ModulesRegistry.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class ExternalStationCustomerValidator : AbstractValidator<ExternalStationCustomer>
{
    public ExternalStationCustomerValidator(IStringLocalizer<Strings> localizer)
    {
        RuleFor(m => m.CustomerName)
            .NotEmpty()
            .MinimumLength(1)
            .MaximumLength(50)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer["Name"]);
        RuleFor(m => m.OpenedYear)
            .MustBeYearOrEmpty(localizer)
            .WithName(n => localizer[nameof(n.OpenedYear)]);
        RuleFor(m => m.ClosedYear)
            .MustBeYearOrEmpty(localizer)
            .WithName(n => localizer[nameof(n.ClosedYear)]);
    }
}
