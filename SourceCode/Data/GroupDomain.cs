using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public class GroupDomain
{
    public GroupDomain()
    {
        Groups = new HashSet<Group>();
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Group> Groups { get; set; }
}

#nullable enable

internal static class GroupDomainMapper
{
    public static void MapGroupDomain(this ModelBuilder builder) =>
        builder.Entity<GroupDomain>(entity =>
        {
            entity.ToTable("GroupDomain");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });
}
