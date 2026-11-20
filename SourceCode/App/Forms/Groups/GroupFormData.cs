using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Groups;

/// <summary>
/// The fields of the group form (Migration step 3.6). The id comes from the page's address. The display names and the messages
/// are resource keys; the attributes are also checked in the browser.
/// </summary>
[ValidatableType]
public class GroupFormData
{
    [Display(Name = "ShortName")]
    [Required]
    [StringLength(10)]
    [OrdinaryText]
    public string ShortName { get; set; } = string.Empty;

    [Display(Name = "FullName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "CityName")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string CityName { get; set; } = string.Empty;

    [Display(Name = "Country")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int CountryId { get; set; }

    // A resource key: "Community" or "NonProfitAssociation".
    [Display(Name = "Category")]
    [Required(ErrorMessage = "MustBeSelected_Error")]
    [StringLength(20)]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Domain")]
    public int? GroupDomainId { get; set; }
}

public static class GroupFormExtensions
{
    extension(Group entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public GroupFormData AsFormData() => new()
        {
            ShortName = entity.ShortName ?? string.Empty,
            FullName = entity.FullName ?? string.Empty,
            CityName = entity.CityName ?? string.Empty,
            CountryId = entity.CountryId,
            Category = entity.Category ?? string.Empty,
            GroupDomainId = entity.GroupDomainId,
        };
    }

    extension(GroupFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the members stay. Create:
        /// <c>form.AsEntity()</c>. The service corrects the values and decides who may change the country and the domain. Call it
        /// only with validated form data.
        /// </summary>
        public Group AsEntity(Group? existing = null)
        {
            var entity = existing ?? new Group();
            entity.ShortName = form.ShortName;
            entity.FullName = form.FullName;
            entity.CityName = form.CityName;
            entity.CountryId = form.CountryId;
            entity.Category = form.Category;
            entity.GroupDomainId = form.GroupDomainId;
            return entity;
        }
    }
}