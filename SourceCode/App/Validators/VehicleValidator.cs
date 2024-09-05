using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Extensions;

namespace ModulesRegistry.Validators;

public class VehicleValidator : AbstractValidator<Vehicle>
{
    public VehicleValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(x => x.InventoryNumber).InclusiveBetween(1, 9999).WithName(x => localizer[nameof(x.InventoryNumber)]);
        RuleFor(x => x.PrototypeManufacturerName).Length(1, 20).MustBeOrdinaryText(localizer).WithName(localizer["Manufacturer"]);
        RuleFor(x => x.PrototypeManufactureYear).IsEmptyOrInclusiveBetween(1850, DateTime.Now.Year, localizer);
        RuleFor(x => x.PrototypeImageHref).Length(0, 256);
        RuleFor(x => x.KeeperSignature).Length(1, 10).WithName(x => localizer[nameof(x.KeeperSignature)]);
        RuleFor(x => x.VehicleClass).NotEmpty().Length(1, 10).WithName(x => localizer[nameof(x.VehicleClass)]);
        RuleFor(x => x.VehicleNumber).NotEmpty().Length(1, 20).WithName(x => localizer[nameof(x.VehicleNumber)]);
        RuleFor(x => x.Theme).NotEmpty().Length(1, 10).WithName(x => localizer[nameof(x.Theme)]);
        RuleFor(x => x.ModelManufacturerName).NotEmpty().Length(1, 20).MustBeOrdinaryText(localizer).WithName(localizer.FromParts("Model-Manufacturer"));
        RuleFor(x => x.ModelNumber).Length(0, 16).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.ModelNumber)]);
        RuleFor(x => x.ThisEmbodiementFromYear).MustBeValidYear(localizer).WithName(localizer.FromParts("PeriodInThisVersion-FromYear"));
        RuleFor(x => x.ThisEmbodiementUptoYear).MustBeValidYear(localizer).WithName(localizer.FromParts("PeriodInThisVersion-UptoYear"));
        RuleFor(x => x.ScaleId).MustBeSelected(localizer).WithName(x => localizer[nameof(x.Scale)]);
        RuleFor(x => x.DecoderType).Length(0, 20).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.DecoderType)]);

        RuleFor(x => x.OwningPersonId).MustBeSelected(localizer).WithName(localizer.FromParts("Owner"));
        RuleFor(x => x.Note).Length(0, 200).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.Note)]);
        RuleFor(x => x.EnginePower).IsEmptyOrInclusiveBetween(0, 10000, localizer).WithName(x => localizer[nameof(x.EnginePower)]);
        RuleFor(x => x.PrototypeLength).IsEmptyOrInclusiveBetween(0, 100, localizer).WithName(localizer["Length"]);
        RuleFor(x => x.PrototypeWeight).IsEmptyOrInclusiveBetween(0, 1000, localizer).WithName(localizer["Weight"]);

        RuleFor(x => x.F0).Length(0, 12);
        RuleFor(x => x.F1).Length(0, 12);
        RuleFor(x => x.F2).Length(0, 12);
        RuleFor(x => x.F3).Length(0, 12);
        RuleFor(x => x.F4).Length(0, 12);
        RuleFor(x => x.F5).Length(0, 12);
        RuleFor(x => x.F6).Length(0, 12);
        RuleFor(x => x.F7).Length(0, 12);
        RuleFor(x => x.F8).Length(0, 12);
        RuleFor(x => x.F9).Length(0, 12);
        RuleFor(x => x.F10).Length(0, 12);
        RuleFor(x => x.F11).Length(0, 12);
        RuleFor(x => x.F12).Length(0, 12);
        RuleFor(x => x.F13).Length(0, 12);
        RuleFor(x => x.F14).Length(0, 12);
        RuleFor(x => x.F15).Length(0, 12);
        RuleFor(x => x.F16).Length(0, 12);
        RuleFor(x => x.F17).Length(0, 12);
        RuleFor(x => x.F18).Length(0, 12);
        RuleFor(x => x.F19).Length(0, 12);
        RuleFor(x => x.F20).Length(0, 12);
        RuleFor(x => x.F21).Length(0, 12);
        RuleFor(x => x.F22).Length(0, 12);
        RuleFor(x => x.F23).Length(0, 12);
        RuleFor(x => x.F24).Length(0, 12);
        RuleFor(x => x.F25).Length(0, 12);
        RuleFor(x => x.F26).Length(0, 12);
        RuleFor(x => x.F27).Length(0, 12);
        RuleFor(x => x.F28).Length(0, 12);
        RuleFor(x => x.F29).Length(0, 12);

    }
}