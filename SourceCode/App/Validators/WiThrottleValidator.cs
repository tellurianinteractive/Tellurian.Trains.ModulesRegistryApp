using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class WiThrottleValidator : AbstractValidator<WiFredThrottle>

{
    public WiThrottleValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(throttle => throttle.OwningPersonId)
            .MustBeSelected(localizer)
            .WithName(throttle => localizer["Owner"]);
        RuleFor(throttle => throttle.InventoryNumber)
            .NotEmpty()
            .GreaterThanOrEqualTo(1)
            .WithName(throttle => localizer[nameof(throttle.InventoryNumber)]);
        RuleFor(throttle => throttle.Name)
            .NotEmpty()
            .MustBeOrdinaryText(localizer)
            .WithName(throttle => localizer[nameof(throttle.Name)]);
        RuleFor(throttle => throttle.MacAddress)
            .NotEmpty()
            .MustBeMacAddress(localizer)
            .WithName(throttle => localizer[nameof(throttle.MacAddress)]);
        RuleFor(throttle => throttle.LocoAddress1)
            .MustBeDccAddressOrEmpty(localizer)
            .WithName(throttle => localizer["DccAddress"]);
        RuleFor(throttle => throttle.LocoAddress2)
            .MustBeDccAddressOrEmpty(localizer)
            .WithName(throttle => localizer["DccAddress"]);
        RuleFor(throttle => throttle.LocoAddress3)
            .MustBeDccAddressOrEmpty(localizer)
            .WithName(throttle => localizer["DccAddress"]);
        RuleFor(throttle => throttle.LocoAddress4)
            .MustBeDccAddressOrEmpty(localizer)
            .WithName(throttle => localizer["DccAddress"]);
    }
}
