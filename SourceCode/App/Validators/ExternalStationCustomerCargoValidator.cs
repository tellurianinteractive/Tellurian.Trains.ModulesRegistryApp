using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class ExternalStationCustomerCargoValidator : AbstractValidator<ExternalStationCustomerCargo>
{
    public ExternalStationCustomerCargoValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(m => m.CargoId)
            .MustBeSelected(localizer)
            .WithName(n => localizer["Cargo"]);
        RuleFor(m => m.DirectionId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.Direction)]);
        RuleFor(m => m.OperatingDayId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.OperatingDay)]);
        RuleFor(m => m.QuantityUnitId)
            .MustBeSelected(localizer)
            .WithName(n => localizer[nameof(n.QuantityUnit)]);
        RuleFor(m => m.Quantity)
            .InclusiveBetween(1, 1000)
            .WithName(n => localizer[nameof(n.Quantity)]);
        RuleFor(m => m.SpecificWagonClass)
            .MaximumLength(10)
            .MustBeOrdinaryTextOrNull(localizer)
            .WithName(n => localizer["OtherWagonClass"]);
        RuleFor(m => m.SpecialCargoName)
            .MaximumLength(20)
            .MustBeOrdinaryText(localizer)
            .WithName(n => localizer[nameof(n.SpecialCargoName)]);
        RuleFor(m => m.FromYear)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.FromYear)]);
        RuleFor(m => m.UptoYear)
            .MustBeValidYear(localizer)
            .WithName(n => localizer[nameof(n.UptoYear)]);
    }
}
