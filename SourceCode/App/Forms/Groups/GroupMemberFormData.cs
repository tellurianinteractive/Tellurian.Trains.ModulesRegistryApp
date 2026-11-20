using Microsoft.Extensions.Validation;
using ModulesRegistry.Data;
using System.ComponentModel.DataAnnotations;

namespace ModulesRegistry.Forms.Groups;

/// <summary>
/// A group member's rights (Migration step 3.6), on the add member form with the person chosen by search, and on the edit member
/// form, where the person is fixed. The group and member ids come from the page's address. The display names and the messages are
/// resource keys.
/// </summary>
[ValidatableType]
public class GroupMemberFormData
{
    // Chosen by the person search on the add member form; not used on the edit member form.
    [Display(Name = "Person")]
    [Range(1, int.MaxValue, ErrorMessage = "MustBeSelected_Error")]
    public int PersonId { get; set; }

    [Display(Name = "IsGroupAdministrator")]
    public bool IsGroupAdministrator { get; set; }

    [Display(Name = "IsDataAdministrator")]
    public bool IsDataAdministrator { get; set; }

    [Display(Name = "IsMeetingAdministrator")]
    public bool IsMeetingAdministrator { get; set; }

    [Display(Name = "MayBorrowGroupsModules")]
    public bool MayBorrowGroupsModules { get; set; }

    [Display(Name = "MemberMayBorrowMyModules")]
    public bool MemberMayBorrowMyModules { get; set; }
}

public static class GroupMemberFormExtensions
{
    extension(GroupMember entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public GroupMemberFormData AsFormData() => new()
        {
            PersonId = entity.PersonId,
            IsGroupAdministrator = entity.IsGroupAdministrator,
            IsDataAdministrator = entity.IsDataAdministrator,
            IsMeetingAdministrator = entity.IsMeetingAdministrator,
            MayBorrowGroupsModules = entity.MayBorrowGroupsModules,
            MemberMayBorrowMyModules = entity.MemberMayBorrowMyModules,
        };
    }

    extension(GroupMemberFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> changes only the rights; the group and person stay. Add:
        /// <c>form.AsEntity(groupId: ...)</c> makes the chosen person a member of the group. Call it only with validated form data.
        /// </summary>
        public GroupMember AsEntity(GroupMember? existing = null, int groupId = 0)
        {
            var entity = existing ?? new GroupMember { GroupId = groupId, PersonId = form.PersonId };
            entity.IsGroupAdministrator = form.IsGroupAdministrator;
            entity.IsDataAdministrator = form.IsDataAdministrator;
            entity.IsMeetingAdministrator = form.IsMeetingAdministrator;
            entity.MayBorrowGroupsModules = form.MayBorrowGroupsModules;
            entity.MemberMayBorrowMyModules = form.MemberMayBorrowMyModules;
            return entity;
        }
    }
}