using Microsoft.Extensions.Validation;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.ExternalStationCustomers;

/// <summary>
/// The fields of the external station customer form (Migration step 3.6): the name and the years. The cargo flows have their own
/// form (<see cref="ExternalStationCustomerCargoFormData"/>). The ids come from the page's address. The display names and the
/// messages are resource keys; the attributes are also checked in the browser.
/// </summary>
[ValidatableType]
public class ExternalStationCustomerFormData
{
    [Display(Name = "Name")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string CustomerName { get; set; } = string.Empty;

    // int?, as AppInputNumber binds it; the entity's short? is cast in the mapping.
    [Display(Name = "OpenedYear")]
    [YearOrEmpty]
    public int? OpenedYear { get; set; }

    [Display(Name = "ClosedYear")]
    [YearOrEmpty]
    public int? ClosedYear { get; set; }
}
