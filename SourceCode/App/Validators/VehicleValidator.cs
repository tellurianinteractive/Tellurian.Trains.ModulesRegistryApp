using FluentValidation;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;

namespace ModulesRegistry.Validators;

public class VehicleValidator : AbstractValidator<Vehicle>
{
    public VehicleValidator(IStringLocalizer<App> localizer)
    {
        RuleFor(x => x.InventoryNumber).InclusiveBetween(1, 9999).WithName(x => localizer[nameof(x.InventoryNumber)]);

        RuleFor(x => x.PrototypeManufacturerName).Length(1, 20).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.PrototypeManufacturerName)]);
        RuleFor(x => x.KeeperSignature).Length(1, 10).WithName(x => localizer[nameof(x.KeeperSignature)]);
        RuleFor(x => x.VehicleClass).NotEmpty().Length(1, 10).WithName(x => localizer[nameof(x.VehicleClass)]);
        RuleFor(x => x.VehicleNumber).NotEmpty().Length(1, 20).WithName(x => localizer[nameof(x.VehicleNumber)]);
        RuleFor(x => x.Theme).NotEmpty().Length(1, 10).WithName(x => localizer[nameof(x.Theme)]);

        RuleFor(x => x.ModelManufacturerName).NotEmpty().Length(1, 20).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.ModelManufacturerName)]);
        RuleFor(x => x.ModelNumber).Length(0, 16).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.ModelNumber)]);
        RuleFor(x => x.ThisEmbodiementFromYear).MustBeValidYear(localizer).WithName(x => localizer[nameof(x.ThisEmbodiementFromYear)]);
        RuleFor(x => x.ThisEmbodiementUptoYear).MustBeValidYear(localizer).WithName(x => localizer[nameof(x.ThisEmbodiementUptoYear)]);
        RuleFor(x => x.ScaleId).MustBeSelected(localizer).WithName(x => localizer[nameof(x.Scale)]);
        RuleFor(x => x.DecoderType).Length(0, 20).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.DecoderType)]);

        RuleFor(x => x.OwningPersonId).MustBeSelected(localizer).WithName(x => localizer["Owner"]);
        RuleFor(x => x.Note).Length(0, 200).MustBeOrdinaryText(localizer).WithName(x => localizer[nameof(x.Note)]);

    }
}