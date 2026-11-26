#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public partial class ModuleStandard
{
    public int Id { get; set; }
    public string ShortName { get; set; }
    public int ScaleId { get; set; }
    public string TrackSystem { get; set; }
    public string PreferredTheme { get; set; }
    public int MainThemeId { get; set; }
    /// <summary>The standard this standard is an extension of, if any.</summary>
    public int? ExtendsStandardId { get; set; }
    /// <summary>The country of a national standard; empty for an international standard.</summary>
    public int? OnlyValidInCountryId { get; set; }
    public virtual Scale Scale { get; set; }
    public virtual ModuleStandard ExtendsStandard { get; set; }
    public virtual Country OnlyValidInCountry { get; set; }
    /// <summary>The wheel profiles and couplings the standard accepts.</summary>
    public virtual ICollection<ModuleFeature> Features { get; set; } = [];
}

public static class ModuleStandardMapping
{
    internal static void MapModuleStandard(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ModuleStandard>(entity =>
        {
            entity.ToTable("ModuleStandard");

            entity.Property(e => e.PreferredTheme).HasMaxLength(50);

            entity.Property(e => e.ShortName).HasMaxLength(12);

            entity.Property(e => e.TrackSystem)
                .HasMaxLength(20);

            entity.HasOne(d => d.Scale)
                .WithMany(p => p.ModuleStandards)
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ExtendsStandard)
                .WithMany()
                .HasForeignKey(d => d.ExtendsStandardId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OnlyValidInCountry)
                .WithMany()
                .HasForeignKey(d => d.OnlyValidInCountryId)
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.Property(e => e.MainThemeId).HasDatabaseDefault("0");
        });
}

#nullable enable

public static class ModuleStandardExtensions
{
    extension(ModuleStandard standard)
    {
        /// <summary>
        /// The standard's own value, or when it has none, the value of the standard it extends, directly or through others, with
        /// the standard the value comes from (the standard itself for its own value). <paramref name="standardsById"/> must hold
        /// the extended standards; a loop ends the search.
        /// </summary>
        public (string? Value, ModuleStandard? From) InheritedValue(IReadOnlyDictionary<int, ModuleStandard> standardsById, Func<ModuleStandard, string?> value)
        {
            var visited = new HashSet<int>();
            ModuleStandard? current = standard;
            while (current is not null && visited.Add(current.Id))
            {
                var text = value(current);
                if (text.HasValue) return (text, current);
                current = current.ExtendsStandardId is int id ? standardsById.GetValueOrDefault(id) : null;
            }
            return (null, null);
        }
    }
}