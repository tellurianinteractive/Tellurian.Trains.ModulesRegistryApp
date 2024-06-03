using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class GroupMember
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int PersonId { get; set; }
    public bool IsGroupAdministrator { get; set; }
    public bool IsDataAdministrator { get; set; }
    public bool MayBorrowGroupsModules { get; set; }
    public bool MemberMayBorrowMyModules { get; set; }

    public virtual Group Group { get; set; }
    public virtual Person Person { get; set; }
    public override string ToString() => $"{Person?.Name()} in {Group?.ShortName}";
}

public static class GroupMemberExtensions
{
    public static bool IsAnyAdministrator(this GroupMember groupMember) => groupMember.IsGroupAdministrator || groupMember.IsDataAdministrator;

}

#nullable enable

internal static class GroupMemberMapper
{
    public static void MapGroupMember(this ModelBuilder builder) =>
        builder.Entity<GroupMember>(entity =>
        {
            entity.ToTable("GroupMember");

            entity.HasOne(d => d.Group)
                .WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.GroupId);

            entity.HasOne(d => d.Person)
                .WithMany(p => p.GroupMembers)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.NoAction);
        });
}
