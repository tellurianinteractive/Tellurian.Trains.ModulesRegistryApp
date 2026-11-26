using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Persons;

/// <summary>
/// The fields of the person form (Migration step 3.6), with the user's rights when the person is a user. The id comes from the
/// page's address. The display names and the messages are resource keys; the attributes are also checked in the browser.
/// Names, addresses and the FREMO fields are corrected when saving (<c>CorrectInput</c>), so they aren't checked for capitals.
/// </summary>
[ValidatableType]
public class PersonFormData
{
    [Display(Name = "FirstName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string FirstName { get; set; } = string.Empty;

    [Display(Name = "NickName")]
    [StringLength(50)]
    [OrdinaryText]
    public string? NickName { get; set; }

    [Display(Name = "MiddleName")]
    [StringLength(50)]
    [OrdinaryText]
    public string? MiddleName { get; set; }

    [Display(Name = "LastName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "CityName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string CityName { get; set; } = string.Empty;

    [Display(Name = "Country")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int CountryId { get; set; }

    [Display(Name = "EmailAddresses")]
    [StringLength(50)]
    [EmailAddresses]
    public string? EmailAddresses { get; set; }

    [Display(Name = "FremoNumber")]
    [FremoMemberNumberOrEmpty]
    public int? FremoMemberNumber { get; set; }

    [Display(Name = "FremoPrefix")]
    [StringLength(10)]
    [OrdinaryText]
    public string? FremoOwnerSignature { get; set; }

    [Display(Name = "FremoReservedAdresses")]
    [StringLength(200)]
    [LocoAddresses]
    public string? FremoReservedAdresses { get; set; }

    // The user's rights: the page and the service keep the stored value of each right the signed-in user may not change.

    [Display(Name = "IsGlobalAdministrator")]
    public bool IsGlobalAdministrator { get; set; }

    [Display(Name = "IsCountryAdministrator")]
    public bool IsCountryAdministrator { get; set; }

    [Display(Name = "IsApiAccessPermitted")]
    public bool IsApiAccessPermitted { get; set; }

    [Display(Name = "MayUploadSkpDrawing")]
    public bool MayUploadSkpDrawing { get; set; }

    [Display(Name = "MayVerifyWiFreds")]
    public bool MayManageWiFreds { get; set; }

    [Display(Name = "MayManageEndProfiles")]
    public bool MayManageEndProfiles { get; set; }

    /// <summary>The area of responsibility is edited only for a global or country administrator.</summary>
    public bool IsAdministrator => IsGlobalAdministrator || IsCountryAdministrator;

    [Display(Name = "AdministratorAreaOfResposibility")]
    [StringLength(50)]
    [OrdinaryText]
    public string? AdministratorAreaOfResposibility { get; set; }
}

public static class PersonFormExtensions
{
    extension(Person entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public PersonFormData AsFormData() => new()
        {
            FirstName = entity.FirstName.OrEmpty,
            NickName = entity.NickName,
            MiddleName = entity.MiddleName,
            LastName = entity.LastName.OrEmpty,
            CityName = entity.CityName.OrEmpty,
            CountryId = entity.CountryId,
            EmailAddresses = entity.EmailAddresses,
            FremoMemberNumber = entity.FremoMemberNumber,
            FremoOwnerSignature = entity.FremoOwnerSignature,
            FremoReservedAdresses = entity.FremoReservedAdresses,
            IsGlobalAdministrator = entity.User?.IsGlobalAdministrator == true,
            IsCountryAdministrator = entity.User?.IsCountryAdministrator == true,
            IsApiAccessPermitted = entity.User?.IsApiAccessPermitted == true,
            MayUploadSkpDrawing = entity.User?.MayUploadSkpDrawing == true,
            MayManageWiFreds = entity.User?.MayManageWiFreds == true,
            MayManageEndProfiles = entity.User?.MayManageEndProfiles == true,
            AdministratorAreaOfResposibility = entity.User?.AdministratorAreaOfResposibility,
        };
    }

    extension(PersonFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the rest of the person and
        /// its user stay. Create: <c>form.AsEntity()</c>, a person without a user. The user's rights are set only when the person is a
        /// user; the service decides which of them the signed-in user may change. Call it only with validated form data.
        /// </summary>
        public Person AsEntity(Person? existing = null)
        {
            var entity = existing ?? new Person();
            entity.FirstName = form.FirstName;
            entity.NickName = form.NickName;
            entity.MiddleName = form.MiddleName;
            entity.LastName = form.LastName;
            entity.CityName = form.CityName;
            entity.CountryId = form.CountryId;
            entity.EmailAddresses = form.EmailAddresses;
            entity.FremoMemberNumber = form.FremoMemberNumber;
            entity.FremoOwnerSignature = form.FremoOwnerSignature;
            entity.FremoReservedAdresses = form.FremoReservedAdresses;
            if (entity.User is { } user)
            {
                user.IsGlobalAdministrator = form.IsGlobalAdministrator;
                user.IsCountryAdministrator = form.IsCountryAdministrator;
                user.IsApiAccessPermitted = form.IsApiAccessPermitted;
                user.MayUploadSkpDrawing = form.MayUploadSkpDrawing;
                user.MayManageWiFreds = form.MayManageWiFreds;
                user.MayManageEndProfiles = form.MayManageEndProfiles;
                user.AdministratorAreaOfResposibility = form.AdministratorAreaOfResposibility;
            }
            return entity;
        }
    }
}