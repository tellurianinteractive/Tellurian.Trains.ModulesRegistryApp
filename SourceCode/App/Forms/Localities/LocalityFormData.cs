using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using ModulesRegistry.Resources;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Localities;

/// <summary>
/// The fields of the locality form. The id comes from the page's address. The coordinates are one field, in the form map services
/// copy them (<see cref="GeoCoordinates.Parse"/>). The display names and the messages are resource keys.
/// </summary>
[ValidatableType]
public class LocalityFormData : IValidatableObject
{
    [Display(Name = "Country")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int CountryId { get; set; }

    [Display(Name = "Name")]
    [Required]
    [StringLength(100)]
    [OrdinaryText]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "IsModuleStorage")]
    public bool IsModuleStorage { get; set; }

    [Display(Name = "IsMeetingVenue")]
    public bool IsMeetingVenue { get; set; }

    [Display(Name = "StreetAddress")]
    [StringLength(100)]
    [OrdinaryText]
    public string? StreetAddress { get; set; }

    [Display(Name = "PostalCode")]
    [StringLength(10)]
    [OrdinaryText]
    public string? PostalCode { get; set; }

    [Display(Name = "CityName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string CityName { get; set; } = string.Empty;

    [Display(Name = "Coordinates")]
    [StringLength(50)]
    public string? Coordinates { get; set; }

    [Display(Name = "Description")]
    [StringLength(2000)]
    [OrdinaryText]
    public string? Description { get; set; }

    [Display(Name = "UsedByGroup")]
    public int? UsedByGroupId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Coordinates) && GeoCoordinates.Parse(Coordinates) is null)
            yield return new ValidationResult(Strings.Coordinates_Error, [nameof(Coordinates)]);
    }
}

public static class LocalityFormExtensions
{
    extension(Locality entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public LocalityFormData AsFormData() => new()
        {
            CountryId = entity.CountryId,
            Name = entity.Name,
            IsModuleStorage = entity.IsModuleStorage,
            IsMeetingVenue = entity.IsMeetingVenue,
            StreetAddress = entity.StreetAddress,
            PostalCode = entity.PostalCode,
            CityName = entity.CityName,
            Coordinates = GeoCoordinates.From(entity.Latitude, entity.Longitude)?.ToString(),
            Description = entity.Description,
            UsedByGroupId = entity.UsedByGroupId,
        };
    }

    extension(LocalityFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the hall drawing stays.
        /// Create: <c>form.AsEntity()</c>. Call it only with validated form data; the service corrects the texts.
        /// </summary>
        public Locality AsEntity(Locality? existing = null)
        {
            var entity = existing ?? new Locality();
            var coordinates = GeoCoordinates.Parse(form.Coordinates);
            entity.CountryId = form.CountryId;
            entity.Name = form.Name;
            entity.IsModuleStorage = form.IsModuleStorage;
            entity.IsMeetingVenue = form.IsMeetingVenue;
            entity.StreetAddress = form.StreetAddress;
            entity.PostalCode = form.PostalCode;
            entity.CityName = form.CityName;
            entity.Latitude = coordinates?.Latitude;
            entity.Longitude = coordinates?.Longitude;
            entity.Description = form.Description;
            entity.UsedByGroupId = form.UsedByGroupId;
            return entity;
        }
    }
}