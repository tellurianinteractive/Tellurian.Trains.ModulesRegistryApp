using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators
{
    public class StationCustomerCargoValidator : AbstractValidator<StationCustomerCargo>
    {
        public StationCustomerCargoValidator(IStringLocalizer<App> localizer)
        {
            RuleFor(m => m.StationCustomerId).MustBeSelected(localizer).WithName(n => localizer["Customer"]);
            RuleFor(m => m.CargoId).MustBeSelected(localizer).WithName(n => localizer["Cargo"]);
            RuleFor(m => m.DirectionId).MustBeSelected(localizer).WithName(n => localizer["Direction"]);
            RuleFor(m => m.OperatingDayId).MustBeSelected(localizer).WithName(n => localizer["OperatingDays"]);
            RuleFor(m => m.QuantityUnitId).MustBeSelected(localizer).WithName(n => localizer["QuantityUnit"]);
            RuleFor(m => m.Quantity).InclusiveBetween(1,1000).WithName(n => localizer["Quantity"]);
            RuleFor(m => m.SpecialCargoName).MaximumLength(20).MustBeOrdinaryTextOrNull(localizer).WithName(n => localizer["SpecialName"]);
            RuleFor(m => m.ReadyTimeId).MustBeSelected(localizer).WithName(n => localizer[ReadyTimeLabel(n)]);
            RuleFor(m => m.FromYear).MustBeValidYear(localizer).WithName(n => localizer[nameof(n.FromYear)]);
            RuleFor(m => m.UptoYear).MustBeValidYear(localizer).WithName(n => localizer[nameof(n.UptoYear)]);
            RuleFor(m => m.TrackOrArea).MaximumLength(10).MustBeOrdinaryTextOrNull(localizer).WithName(n => localizer[nameof(n.TrackOrArea)]);
            RuleFor(m => m.TrackOrAreaColor).MustBeColor(localizer).WithName(n => localizer[nameof(n.TrackOrAreaColor)]);
        }

        static string ReadyTimeLabel(StationCustomerCargo cargo) => cargo is null ? "ReadyTime" : cargo.DirectionId == 1 || cargo.DirectionId == 4 ? "UnloadingReady" : "LoadingReady";
    }
}
