using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Meetings;

/// <summary>
/// A participant's registration for a layout (Migration step 3.6): whether modules come after agreement with the organiser, and a
/// note to the organiser. The ids come from the page's address.
/// </summary>
[ValidatableType]
public class LayoutParticipantFormData
{
    public const int MaxNoteLength = 1000;

    [Display(Name = "BringsModulesAfterAgreement")]
    public bool BringsModulesAfterAgreement { get; set; }

    [Display(Name = "Comment")]
    [StringLength(MaxNoteLength)]
    public string? NoteToOrganiserMarkdown { get; set; }
}

public static class LayoutParticipantFormExtensions
{
    extension(LayoutParticipant entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public LayoutParticipantFormData AsFormData() => new()
        {
            BringsModulesAfterAgreement = entity.BringsModulesAfterAgreement,
            NoteToOrganiserMarkdown = entity.NoteToOrganiserMarkdown,
        };
    }

    extension(LayoutParticipantFormData form)
    {
        /// <summary>Form data → the loaded layout registration: only the form's fields change. Call it only with validated form data.</summary>
        public LayoutParticipant AsEntity(LayoutParticipant existing)
        {
            existing.BringsModulesAfterAgreement = form.BringsModulesAfterAgreement;
            existing.NoteToOrganiserMarkdown = string.IsNullOrWhiteSpace(form.NoteToOrganiserMarkdown) ? null : form.NoteToOrganiserMarkdown.Trim();
            return existing;
        }
    }
}