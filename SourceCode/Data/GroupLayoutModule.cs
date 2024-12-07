using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModulesRegistry.Data;

#nullable disable
public class GroupLayoutModule
{
    public int Id { get; set; }
    public int GroupId { get; set; }
    public int? GroupMemberId { get; set; }
    public int ModuleId { get; set; }

    public virtual Group Group { get; set; }
    public virtual GroupMember GroupMember { get; set; }
    public virtual Module Module { get; set; }
   
}
# nullable enable

internal static class GroupLayoutModuleMapper
{
    public static void MapGroupLayoutModule(this ModelBuilder builder) =>
        builder.Entity<GroupLayoutModule>(entity =>
        {
            entity.ToTable("GroupLayoutModule");

            //entity.HasOne<Module>()
            //    .WithMany()
            //    .HasForeignKey(e => e.ModuleId);
            //entity.HasOne<Group>()
            //   .WithMany()
            //   .HasForeignKey(e => e.GroupId);
            //entity.HasOne<GroupMember>()
            //   .WithMany()
            //   .HasForeignKey(e => e.GroupMemberId);
        });
}

