using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using ModulesRegistry.Resources;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Meetings;

/// <summary>
/// The fields of the meeting form (Migration step 3.6). The meeting's id comes from the page's address. The display names and the
/// messages are resource keys; the attributes are also checked in the browser. Only the server checks that a module meeting has the
/// arrival and departure times, and that a changed start date is not more than a week ago (<see cref="StoredStartDate"/>). An end
/// date before the start date is corrected when saved.
/// </summary>
[ValidatableType]
public class MeetingFormData : IValidatableObject
{
    public const int DetailsMaxLength = 2000;
    public const int AccomodationMaxLength = 1000;
    public const int FoodMaxLength = 1000;
    public const int DaysBackForStartDate = 7;

    /// <summary>The stored start date, set by the page before validation, never posted; null for a new meeting.</summary>
    public DateTime? StoredStartDate { get; set; }

    /// <summary>Today, set by the page before validation (the app's time provider).</summary>
    public DateTime Today { get; set; } = DateTime.Today;

    [Display(Name = "MeetingType")]
    public int MeetingType { get; set; }

    [Display(Name = "Status")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int Status { get; set; }

    [Display(Name = "Name")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Organiser")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int OrganiserGroupId { get; set; }

    [Display(Name = "IsOrganisingGroupInternal")]
    public bool IsOrganiserInternal { get; set; }

    [Display(Name = "City")]
    [Required]
    [StringLength(50)]
    [OrdinaryText]
    public string CityName { get; set; } = string.Empty;

    [Display(Name = "Venue")]
    [StringLength(50)]
    [OrdinaryText]
    public string? VenueName { get; set; }

    [Display(Name = "InPartnershipWith")]
    public int? GroupDomainId { get; set; }

    [Display(Name = "StartDate")]
    public DateTime StartDate { get; set; }

    [Display(Name = "EndDate")]
    public DateTime EndDate { get; set; }

    [Display(Name = "LatestArrivalTimeWithModules")]
    public TimeOnly? LatestArrivalTimeWithModules { get; set; }

    [Display(Name = "EarliestDepartureTimeWithModules")]
    public TimeOnly? EarliestDepartureTimeWithModules { get; set; }

    [Display(Name = "ExternalLink")]
    [StringLength(1000)]
    public string? ExternalLink { get; set; }

    [Display(Name = "Details")]
    [StringLength(DetailsMaxLength)]
    public string? Details { get; set; }

    [Display(Name = "Accomodation")]
    [StringLength(AccomodationMaxLength)]
    public string? Accomodation { get; set; }

    [Display(Name = "Food")]
    [StringLength(FoodMaxLength)]
    public string? Food { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.Date != StoredStartDate?.Date && StartDate.Date < Today.AddDays(-DaysBackForStartDate))
            yield return new ValidationResult(Strings.StartDateTooEarly_Error, [nameof(StartDate)]);
        if (MeetingType == (int)Data.MeetingType.ModuleMeeting)
        {
            if (LatestArrivalTimeWithModules is null)
                yield return new ValidationResult(string.Format(Strings.MustBeSelected_Error, Strings.LatestArrivalTimeWithModules), [nameof(LatestArrivalTimeWithModules)]);
            if (EarliestDepartureTimeWithModules is null)
                yield return new ValidationResult(string.Format(Strings.MustBeSelected_Error, Strings.EarliestDepartureTimeWithModules), [nameof(EarliestDepartureTimeWithModules)]);
        }
    }
}

public static class MeetingFormExtensions
{
    extension(Meeting entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public MeetingFormData AsFormData() => new()
        {
            MeetingType = entity.MeetingType,
            Status = entity.Status,
            Name = entity.Name ?? string.Empty,
            OrganiserGroupId = entity.OrganiserGroupId,
            IsOrganiserInternal = entity.IsOrganiserInternal,
            CityName = entity.CityName ?? string.Empty,
            VenueName = entity.VenueName,
            GroupDomainId = entity.GroupDomainId,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            LatestArrivalTimeWithModules = entity.LatestArrivalTimeWithModules,
            EarliestDepartureTimeWithModules = entity.EarliestDepartureTimeWithModules,
            ExternalLink = entity.ExternalLink,
            Details = entity.Details,
            Accomodation = entity.Accomodation,
            Food = entity.Food,
        };
    }

    extension(MeetingFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the access key, layouts and
        /// participants stay. Create: <c>form.AsEntity()</c> starts from a new entity. Call it only with validated form data.
        /// </summary>
        public Meeting AsEntity(Meeting? existing = null)
        {
            var entity = existing ?? new Meeting();
            entity.MeetingType = form.MeetingType;
            entity.Status = form.Status;
            entity.Name = form.Name;
            entity.OrganiserGroupId = form.OrganiserGroupId;
            entity.IsOrganiserInternal = form.IsOrganiserInternal;
            entity.CityName = form.CityName;
            entity.VenueName = form.VenueName;
            entity.GroupDomainId = form.GroupDomainId is null or 0 ? null : form.GroupDomainId;
            entity.StartDate = form.StartDate;
            entity.EndDate = form.EndDate;
            entity.LatestArrivalTimeWithModules = form.LatestArrivalTimeWithModules;
            entity.EarliestDepartureTimeWithModules = form.EarliestDepartureTimeWithModules;
            entity.ExternalLink = form.ExternalLink;
            entity.Details = form.Details;
            entity.Accomodation = form.Accomodation;
            entity.Food = form.Food;
            return entity;
        }
    }
}