using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.ExternalLinks;

/// <summary>
/// The fields of the external link form. The owner and the link's id come from the page's address; the service decides which
/// link is the default of its purpose and refuses a second link with the same purpose and language.
/// </summary>
[ValidatableType]
public class ExternalLinkFormData
{
    [Display(Name = "Purpose")]
    [Range(1, byte.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int Purpose { get; set; }

    [Display(Name = "WebAddress")]
    [Required]
    [StringLength(ExternalLinkExtensions.HrefMaxLength)]
    [WebAddress]
    public string Href { get; set; } = string.Empty;

    // One of ExternalLinkContentTypes.All.
    [Display(Name = "ContentType")]
    [Required(ErrorMessage = "MustBeSelected_Error")]
    public string ContentType { get; set; } = string.Empty;

    // Empty is "no language or several".
    [Display(Name = "Language")]
    public string? Language { get; set; }

    [Display(Name = "IsDefaultLink")]
    public bool IsDefault { get; set; }
}

public static class ExternalLinkFormExtensions
{
    extension(ExternalLink entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ExternalLinkFormData AsFormData() => new()
        {
            Purpose = (int)entity.Purpose,
            Href = entity.Href,
            ContentType = entity.ContentType,
            Language = entity.Language,
            IsDefault = entity.IsDefault,
        };
    }

    extension(ExternalLinkFormData form)
    {
        /// <summary>
        /// Form data → a link of the owner, with the id of the link it changes (0 for a new link). The service only takes the
        /// form's fields, and keeps an existing link's owner. Call it only with validated form data.
        /// </summary>
        public ExternalLink AsEntity(string ownerType, int ownerId, int id = 0) => new()
        {
            Id = id,
            OwnerType = ownerType,
            OwnerId = ownerId,
            Purpose = (ExternalLinkPurpose)form.Purpose,
            Href = form.Href,
            ContentType = form.ContentType,
            Language = form.Language,
            IsDefault = form.IsDefault,
        };
    }
}