using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using ModulesRegistry.Services.Implementations;
using System.Text;

namespace ModulesRegistry.Services.Extensions;
public static class VehicleExtensions
{
    public static string ModelInfo(this Vehicle? vehicle) =>
        vehicle is null ? string.Empty :
        $"{vehicle.ModelManufacturerName} {vehicle.ModelNumber}".Trim();

    public static string PrototypeInfo(this Vehicle? vehicle) =>
        vehicle is null ? string.Empty :
        $"{vehicle.KeeperSignature} {vehicle.VehicleClass} {vehicle.VehicleNumber} {LanguageExtensions.GetLocalizedString(vehicle.TractionFeature?.Description)}".Trim();

    public static string DisplayInfo(this Vehicle? vehicle) =>
        vehicle is null ? string.Empty :
        $"{vehicle.InventoryNumber} : {vehicle.KeeperSignature} {vehicle.VehicleClass} {vehicle.VehicleNumber}";

    public static MarkupString Features(this Vehicle vehicle, IStringLocalizer localizer)
    {
        var markup = new StringBuilder(200);
        if (vehicle.DccAddress.HasValue) markup.Append($""" <span>DCC {vehicle.DccAddress.Value}<span>""");
        if (vehicle.DecoderType.HasValue()) markup.Append($""" <span>{vehicle.DecoderType} <span>""");
        if (vehicle.HasSound) markup.Append(""" <i class="fa fa-volume-up"></span>""");
        return new MarkupString(markup.ToString());
    }
}


