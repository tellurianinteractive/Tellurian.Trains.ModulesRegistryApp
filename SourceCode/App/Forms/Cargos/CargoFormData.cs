using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Cargos;

/// <summary>
/// The fields of the cargo form (Migration step 3.6): the NHM code, the default wagon classes, the period of years and the name in
/// each language. The display names and the messages are resource keys; the built-in attributes are also checked in the browser.
/// The cargo's id is not a field: the page takes it from its address.
/// </summary>
[ValidatableType]
public class CargoFormData
{
    private const int TranslatedLength = 30;

    [Display(Name = "NHMCode")]
    [Range(0, int.MaxValue)]
    public int NhmCode { get; set; }

    [Display(Name = "DefaultClasses")]
    [StringLength(10)]
    [OrdinaryText]
    public string? DefaultClasses { get; set; }

    // int?, as AppInputNumber binds it; the entity's short? is cast in the mapping.
    [Display(Name = "FromYear")]
    [YearOrEmpty]
    public int? FromYear { get; set; }

    [Display(Name = "UptoYear")]
    [YearOrEmpty]
    public int? UptoYear { get; set; }

    [Display(Name = "IsExpress")]
    public bool IsExpress { get; set; }

    [Display(Name = "IsCoolingRequired")]
    public bool IsCoolingRequired { get; set; }

    [Display(Name = "English")]
    [Required]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string EN { get; set; } = string.Empty;

    [Display(Name = "German")]
    [Required]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string DE { get; set; } = string.Empty;

    [Display(Name = "Dutch")]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string? NL { get; set; }

    [Display(Name = "French")]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string? FR { get; set; }

    [Display(Name = "Italian")]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string? IT { get; set; }

    [Display(Name = "Polish")]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string? PL { get; set; }

    [Display(Name = "Danish")]
    [Required]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string DA { get; set; } = string.Empty;

    [Display(Name = "Swedish")]
    [Required]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string SV { get; set; } = string.Empty;

    [Display(Name = "Norwegian")]
    [Required]
    [StringLength(TranslatedLength)]
    [OrdinaryText]
    public string NB { get; set; } = string.Empty;
}