using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class Group
{
    public Group()
    {
        GroupMembers = new HashSet<GroupMember>();
        ModuleOwnerships = new HashSet<ModuleOwnership>();
    }

    public int Id { get; set; }
    public int CountryId { get; set; }
    public int? GroupDomainId { get; set; }
    public string FullName { get; set; }
    public string ShortName { get; set; }
    public string Category { get; set; }
    public string CityName { get; set; }

    public virtual Country Country { get; set; }
    public virtual GroupDomain GroupDomain { get; set; }
    public virtual ICollection<GroupMember> GroupMembers { get; set; }
    public virtual ICollection<ModuleOwnership> ModuleOwnerships { get; set; }
    public override string ToString() => FullName;
}

#nullable enable

internal static class GroupMapper
{
    public static void MapGroup(this ModelBuilder builder) =>
        builder.Entity<Group>(entity =>
        {
            entity.ToTable("Group");

            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.CityName).HasMaxLength(50);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ShortName).HasMaxLength(10);

            entity.HasOne(d => d.Country)
                .WithMany(p => p.Groups)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Group_Country");

            entity.HasOne(d => d.GroupDomain)
                .WithMany(p => p.Groups)
                .HasForeignKey(d => d.GroupDomainId)
                .OnDelete(DeleteBehavior.Restrict);
        });
}
