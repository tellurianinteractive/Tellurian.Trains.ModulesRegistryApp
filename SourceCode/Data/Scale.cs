#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class Scale
{
    public Scale()
    {
        ModuleStandards = new HashSet<ModuleStandard>();
        Modules = new HashSet<Module>();
        Vehicles = new HashSet<Vehicle>();
    }

    public int Id { get; set; }
    public string ShortName { get; set; }
    public int Denominator { get; set; }

    public virtual ICollection<ModuleStandard> ModuleStandards { get; set; }
    public virtual ICollection<Module> Modules { get; set; }
    public virtual ICollection<Vehicle> Vehicles { get; set; }
}

# nullable enable
public static class ScaleExtensions
{
    extension(ModelBuilder builder)
    {
        internal void MapScale() =>
            builder.Entity<Scale>(entity =>
            {
                entity.ToTable("Scale");

                entity.Property(e => e.ShortName)
                    .IsRequired()
                    .HasMaxLength(10);
            });
    }

    public static string Display(this Scale? me) => me is null ? string.Empty : $"{me.ShortName} 1:{me.Denominator}";
}
