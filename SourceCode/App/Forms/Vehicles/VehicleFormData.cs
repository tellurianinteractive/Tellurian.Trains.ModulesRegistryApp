using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Vehicles;

/// <summary>
/// The fields of the vehicle form (Migration step 3.6): prototype data, the period of this version, model data and the names of the
/// function keys. The display names and the messages are resource keys; the attributes are also checked in the browser, except the
/// ranges of the optional decimal and power fields, which only the server checks. The vehicle's id and owner are not fields: the
/// page takes the id from its address, and the owner is the stored one, or the user for a new vehicle.
/// </summary>
[ValidatableType]
public class VehicleFormData
{
    private const int FunctionLength = 12; // The function keys F0 to F29 have no display name: the message names the key.

    [Display(Name = "Country")]
    public int? KeeperCountryId { get; set; }

    [Display(Name = "KeeperSignature")]
    [Required]
    [StringLength(10)]
    [OrdinaryText]
    public string KeeperSignature { get; set; } = string.Empty;

    [Display(Name = "Class")]
    [Required]
    [StringLength(10)]
    [OrdinaryText]
    public string VehicleClass { get; set; } = string.Empty;

    [Display(Name = "VehicleNumber")]
    [Required]
    [StringLength(20)]
    [OrdinaryText]
    public string VehicleNumber { get; set; } = string.Empty;

    [Display(Name = "Theme")]
    [Required]
    [StringLength(10)]
    public string Theme { get; set; } = string.Empty;

    [Display(Name = "Manufacturer")]
    [StringLength(20)]
    [OrdinaryText]
    public string? PrototypeManufacturerName { get; set; }

    [Display(Name = "ManufacturedYear")]
    [YearOrEmpty(YearPeriod.MinVehicleYear)]
    public int? PrototypeManufactureYear { get; set; }

    [Display(Name = "Traction")]
    public int? TractionFeatureId { get; set; }

    [Display(Name = "Length")]
    [Range(0.0, 100.0)]
    public decimal? PrototypeLength { get; set; }

    [Display(Name = "Weight")]
    [Range(0.0, 1000.0)]
    public decimal? PrototypeWeight { get; set; }

    [Display(Name = "EnginePower")]
    [Range(0, 10000)]
    public int? EnginePower { get; set; }

    [Display(Name = "ImageLink")]
    [StringLength(256)]
    public string? PrototypeImageHref { get; set; }

    [Display(Name = "FromYear")]
    [YearOrEmpty]
    public int? ThisEmbodiementFromYear { get; set; }

    [Display(Name = "UptoYear")]
    [YearOrEmpty]
    public int? ThisEmbodiementUptoYear { get; set; }

    [Display(Name = "InventoryNumber")]
    [Range(1, 9999)]
    public int InventoryNumber { get; set; }

    [Display(Name = "Scale")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int ScaleId { get; set; }

    [Display(Name = "ModelManufacturer")]
    [Required]
    [StringLength(20)]
    [OrdinaryText]
    public string ModelManufacturerName { get; set; } = string.Empty;

    [Display(Name = "ArticleNumber")]
    [StringLength(16)]
    [OrdinaryText]
    public string? ModelNumber { get; set; }

    [Display(Name = "Couplings")]
    public int? CouplingFeatureId { get; set; }

    [Display(Name = "Wheels")]
    public int? WheelsFeatureId { get; set; }

    [Display(Name = "DecoderType")]
    [StringLength(20)]
    [OrdinaryText]
    public string? DecoderType { get; set; }

    [Display(Name = "DccAddress")]
    [DccAddressOrEmpty]
    public int? DccAddress { get; set; }

    [Display(Name = "HasSound")]
    public bool HasSound { get; set; }

    [Display(Name = "HasRemoteCouplings")]
    public bool HasRemoteCouplings { get; set; }

    [Display(Name = "IsWeathered")]
    public bool IsWeathered { get; set; }

    [Display(Name = "Note")]
    [StringLength(200)]
    [OrdinaryText]
    public string? Note { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F0 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F1 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F2 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F3 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F4 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F5 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F6 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F7 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F8 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F9 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F10 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F11 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F12 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F13 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F14 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F15 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F16 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F17 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F18 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F19 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F20 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F21 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F22 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F23 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F24 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F25 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F26 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F27 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F28 { get; set; }

    [StringLength(FunctionLength)]
    [OrdinaryText]
    public string? F29 { get; set; }
}