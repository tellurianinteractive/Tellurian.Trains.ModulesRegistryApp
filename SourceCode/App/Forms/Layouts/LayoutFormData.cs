using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Validation;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Layouts;

/// <summary>
/// The fields of a meeting's layout form (Migration step 3.6). The meeting's and the layout's ids come from the page's address. The
/// display names and the messages are resource keys; the attributes are also checked in the browser, except the range of the
/// optional maximum number of participants, which only the server checks.
/// </summary>
[ValidatableType]
public class LayoutFormData
{
    public const int MaxNoteLength = 2000;

    [Display(Name = "ResponsibleGroup")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int OrganisingGroupId { get; set; }

    [Display(Name = "PrimaryModuleStandard")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int PrimaryModuleStandardId { get; set; }

    [Display(Name = "ContactPerson")]
    public int? ContactPersonId { get; set; }

    [Display(Name = "Name")]
    [StringLength(10)]
    [OrdinaryText]
    public string? ShortName { get; set; }

    [Display(Name = "Theme")]
    [StringLength(100)]
    [OrdinaryText]
    public string? Theme { get; set; }

    [Display(Name = "RegistrationOpens")]
    public DateTime RegistrationOpeningDate { get; set; }

    [Display(Name = "RegistrationCloses")]
    public DateTime RegistrationClosingDate { get; set; }

    [Display(Name = "ModuleRegistrationCloses")]
    public DateTime? ModuleRegistrationClosingDate { get; set; }

    [Display(Name = "IsRegistrationPermitted")]
    public bool IsRegistrationPermitted { get; set; }

    [Display(Name = "MaxNumberOfParticipants")]
    [Range(1, 200)]
    public int? MaxNumberOfParticipants { get; set; }

    [Display(Name = "FirstYear")]
    [YearOrEmpty]
    public int? FirstYear { get; set; }

    [Display(Name = "LastYear")]
    [YearOrEmpty]
    public int? LastYear { get; set; }

    [Display(Name = "Note")]
    [StringLength(MaxNoteLength)]
    public string? Details { get; set; }
}

/// <summary>
/// The fields of a station's settings in a layout (Migration step 3.6). The dependent settings are made consistent when saved
/// (<c>WithDataRulesApplied</c>), so no field is disabled in the form.
/// </summary>
[ValidatableType]
public class LayoutStationFormData
{
    [Display(Name = "IsManned")]
    public bool IsManned { get; set; }

    [Display(Name = "HidePassingTrains")]
    public bool HidePassingTrains { get; set; }

    [Display(Name = "HideMeetingTrains")]
    public bool HideMeetingTrains { get; set; }

    [Display(Name = "PrintTrainStartLabels")]
    public bool PrintTrainStartLabels { get; set; }

    [Display(Name = "PrintOnlyFirstTrainStartLabel")]
    public bool PrintOnlyFirstTrainStartLabel { get; set; }

    // Any value of DifficultyLevel; the list offers only those.
    [Display(Name = "DifficultyLevel")]
    public int Difficulty { get; set; }

    [Display(Name = "OtherName")]
    [StringLength(50)]
    [OrdinaryText]
    public string? OtherName { get; set; }

    [Display(Name = "OtherSignature")]
    [StringLength(5)]
    [OrdinaryText]
    public string? OtherSignature { get; set; }

    [Display(Name = "OtherCountry")]
    public int? OtherCountryId { get; set; }
}

public static class LayoutFormExtensions
{
    extension(Layout entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public LayoutFormData AsFormData() => new()
        {
            OrganisingGroupId = entity.OrganisingGroupId,
            PrimaryModuleStandardId = entity.PrimaryModuleStandardId,
            ContactPersonId = entity.ContactPersonId,
            ShortName = entity.ShortName,
            Theme = entity.Theme,
            RegistrationOpeningDate = entity.RegistrationOpeningDate,
            RegistrationClosingDate = entity.RegistrationClosingDate,
            ModuleRegistrationClosingDate = entity.ModuleRegistrationClosingDate,
            IsRegistrationPermitted = entity.IsRegistrationPermitted,
            MaxNumberOfParticipants = entity.MaxNumberOfParticipants,
            FirstYear = entity.FirstYear,
            LastYear = entity.LastYear,
            Details = entity.Details,
        };
    }

    extension(LayoutFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the meeting and the layout's
        /// other settings stay. Create: <c>form.AsEntity(meetingId: …)</c> starts a new layout of the meeting. Call it only with validated form data.
        /// </summary>
        public Layout AsEntity(Layout? existing = null, int meetingId = 0)
        {
            var entity = existing ?? new Layout { MeetingId = meetingId };
            entity.OrganisingGroupId = form.OrganisingGroupId;
            entity.PrimaryModuleStandardId = form.PrimaryModuleStandardId;
            entity.ContactPersonId = form.ContactPersonId is null or 0 ? null : form.ContactPersonId;
            entity.ShortName = form.ShortName;
            entity.Theme = form.Theme;
            entity.RegistrationOpeningDate = form.RegistrationOpeningDate;
            entity.RegistrationClosingDate = form.RegistrationClosingDate;
            entity.ModuleRegistrationClosingDate = form.ModuleRegistrationClosingDate;
            entity.IsRegistrationPermitted = form.IsRegistrationPermitted;
            entity.MaxNumberOfParticipants = form.MaxNumberOfParticipants;
            entity.FirstYear = (short?)form.FirstYear;
            entity.LastYear = (short?)form.LastYear;
            entity.Details = form.Details;
            return entity;
        }
    }

    extension(LayoutStation entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public LayoutStationFormData AsFormData() => new()
        {
            IsManned = entity.IsManned,
            HidePassingTrains = entity.HidePassingTrains,
            HideMeetingTrains = entity.HideMeetingTrains,
            PrintTrainStartLabels = entity.PrintTrainStartLabels,
            PrintOnlyFirstTrainStartLabel = entity.PrintOnlyFirstTrainStartLabel,
            Difficulty = entity.Difficulty,
            OtherName = entity.OtherName,
            OtherSignature = entity.OtherSignature,
            OtherCountryId = entity.OtherCountryId,
        };
    }

    extension(LayoutStationFormData form)
    {
        /// <summary>Form data → the loaded layout station: only the form's fields change. Call it only with validated form data.</summary>
        public LayoutStation AsEntity(LayoutStation existing)
        {
            existing.IsManned = form.IsManned;
            existing.HidePassingTrains = form.HidePassingTrains;
            existing.HideMeetingTrains = form.HideMeetingTrains;
            existing.PrintTrainStartLabels = form.PrintTrainStartLabels;
            existing.PrintOnlyFirstTrainStartLabel = form.PrintOnlyFirstTrainStartLabel;
            existing.Difficulty = form.Difficulty;
            existing.OtherName = form.OtherName;
            existing.OtherSignature = form.OtherSignature;
            existing.OtherCountryId = form.OtherCountryId is null or 0 ? null : form.OtherCountryId;
            return existing;
        }
    }
}