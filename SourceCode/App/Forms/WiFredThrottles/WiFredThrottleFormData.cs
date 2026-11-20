using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.WiFredThrottles;

/// <summary>
/// The fields of the WiFRED throttle form (Migration step 3.6). The display names and the messages are resource keys; the attributes
/// are also checked in the browser. The throttle's id is not a field: the page takes it from its address. The numbers are int, as the
/// number fields bind them; the entity's short is cast in the mapping.
/// </summary>
[ValidatableType]
public class WiFredThrottleFormData
{
    [Display(Name = "Owner")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int OwningPersonId { get; set; }

    [Display(Name = "MacAddress")]
    [Required]
    [MacAddress]
    public string MacAddress { get; set; } = string.Empty;

    [Display(Name = "Name")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "InventoryNumber")]
    [Range(1, short.MaxValue)]
    public int InventoryNumber { get; set; }

    [Display(Name = "DccAddress1")]
    [DccAddressOrEmpty]
    public int? LocoAddress1 { get; set; }

    [Display(Name = "DccAddress2")]
    [DccAddressOrEmpty]
    public int? LocoAddress2 { get; set; }

    [Display(Name = "DccAddress3")]
    [DccAddressOrEmpty]
    public int? LocoAddress3 { get; set; }

    [Display(Name = "DccAddress4")]
    [DccAddressOrEmpty]
    public int? LocoAddress4 { get; set; }
}