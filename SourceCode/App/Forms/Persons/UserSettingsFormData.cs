using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Persons;

/// <summary>
/// What users change about themselves on the user settings page (Migration step 3.6): email addresses, city and, until it is set,
/// the FREMO prefix. The names and the country are shown but changed by an administrator. The first email address is the sign-in
/// address, so it is required.
/// </summary>
[ValidatableType]
public class UserSettingsFormData
{
    [Display(Name = "EmailAddresses")]
    [Required]
    [StringLength(50)]
    [EmailAddresses]
    public string EmailAddresses { get; set; } = string.Empty;

    [Display(Name = "CityName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string CityName { get; set; } = string.Empty;

    [Display(Name = "FremoPrefix")]
    [StringLength(10)]
    [OrdinaryText]
    public string? FremoOwnerSignature { get; set; }
}

public static class UserSettingsFormExtensions
{
    extension(Person entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public UserSettingsFormData AsUserSettingsFormData() => new()
        {
            EmailAddresses = entity.EmailAddresses.OrEmpty,
            CityName = entity.CityName.OrEmpty,
            FremoOwnerSignature = entity.FremoOwnerSignature,
        };
    }

    extension(UserSettingsFormData form)
    {
        /// <summary>
        /// Form data → the stored person: only the settings change. A FREMO prefix once set stays (the page shows it disabled).
        /// Call it only with validated form data.
        /// </summary>
        public Person AsEntity(Person existing)
        {
            existing.EmailAddresses = form.EmailAddresses;
            existing.CityName = form.CityName;
            if (existing.FremoOwnerSignature.IsEmpty) existing.FremoOwnerSignature = form.FremoOwnerSignature;
            return existing;
        }
    }
}