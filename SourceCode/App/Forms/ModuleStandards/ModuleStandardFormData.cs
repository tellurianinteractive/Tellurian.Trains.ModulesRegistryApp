using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.ModuleStandards;

/// <summary>
/// The fields of the module standard form (Migration step 3.6). The id comes from the page's address. The display names and the
/// messages are resource keys; the attributes are also checked in the browser.
/// </summary>
[ValidatableType]
public class ModuleStandardFormData
{
    [Display(Name = "ShortName")]
    [Required]
    [StringLength(12)]
    [OrdinaryText]
    public string ShortName { get; set; } = string.Empty;

    [Display(Name = "Scale")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int ScaleId { get; set; }

    // Any (0), Europe or US. The modules and layouts have their own theme; this one is what they get when none is chosen.
    [Display(Name = "MainTheme")]
    public int MainThemeId { get; set; }

    // Empty is "not an extension".
    [Display(Name = "ExtendsStandard")]
    public int? ExtendsStandardId { get; set; }

    // Empty is an international standard. An extension of a national standard gets its country when saved.
    [Display(Name = "OnlyValidInCountry")]
    public int? OnlyValidInCountryId { get; set; }

    [Display(Name = "PreferredTheme")]
    [StringLength(50)]
    [OrdinaryText]
    public string? PreferredTheme { get; set; }

    [Display(Name = "TrackSystem")]
    [StringLength(20)]
    [OrdinaryText]
    public string? TrackSystem { get; set; }

    // The accepted wheel profiles and couplings, checkboxes posted as FeatureIds (read by the page). Another scale's are not saved.
    public List<int> FeatureIds { get; set; } = [];
}

/// <summary>The fields of the module end profile form (Migration step 3.6).</summary>
[ValidatableType]
public class ModuleEndProfileFormData
{
    [Display(Name = "Scale")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int ScaleId { get; set; }

    [Display(Name = "Designation")]
    [Required]
    [StringLength(20)]
    [OrdinaryText]
    public string Designation { get; set; } = string.Empty;

    [Display(Name = "Width")]
    [EndPlateMillimetresOrEmpty]
    public int? WidthMillimeter { get; set; }

    // Empty is "no standard"; with a standard, the scale is the standard's when saved.
    [Display(Name = "ModuleStandard")]
    public int? ModuleStandardId { get; set; }
}

public static class ModuleStandardFormExtensions
{
    extension(ModuleStandard entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ModuleStandardFormData AsFormData() => new()
        {
            ShortName = entity.ShortName ?? string.Empty,
            ScaleId = entity.ScaleId,
            MainThemeId = entity.MainThemeId,
            ExtendsStandardId = entity.ExtendsStandardId,
            OnlyValidInCountryId = entity.OnlyValidInCountryId,
            PreferredTheme = entity.PreferredTheme,
            TrackSystem = entity.TrackSystem,
            FeatureIds = [.. (entity.Features ?? []).Select(f => f.FeatureId)],
        };
    }

    extension(ModuleStandardFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns. Create:
        /// <c>form.AsEntity()</c>. The service corrects the values when saving. Call it only with
        /// validated form data.
        /// </summary>
        public ModuleStandard AsEntity(ModuleStandard? existing = null)
        {
            var entity = existing ?? new ModuleStandard();
            entity.ShortName = form.ShortName;
            entity.ScaleId = form.ScaleId;
            entity.MainThemeId = form.MainThemeId;
            entity.ExtendsStandardId = form.ExtendsStandardId;
            entity.OnlyValidInCountryId = form.OnlyValidInCountryId;
            entity.PreferredTheme = form.PreferredTheme;
            entity.TrackSystem = form.TrackSystem;
            entity.Features = [.. form.FeatureIds.Distinct().Select(id => new ModuleFeature { ModuleStandardId = entity.Id, FeatureId = id })];
            return entity;
        }
    }

    extension(ModuleEndProfile entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ModuleEndProfileFormData AsFormData() => new() { ScaleId = entity.ScaleId, Designation = entity.Designation ?? string.Empty, WidthMillimeter = entity.WidthMillimeter, ModuleStandardId = entity.ModuleStandardId };
    }

    extension(ModuleEndProfileFormData form)
    {
        /// <summary>Form data → entity; the PDF document and the drawing stay. Call it only with validated form data.</summary>
        public ModuleEndProfile AsEntity(ModuleEndProfile? existing = null)
        {
            var entity = existing ?? new ModuleEndProfile();
            entity.ScaleId = form.ScaleId;
            entity.Designation = form.Designation;
            entity.WidthMillimeter = form.WidthMillimeter;
            entity.ModuleStandardId = form.ModuleStandardId;
            return entity;
        }
    }
}