using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

/// <summary>
/// A feature of modules (2026-11-26). Today a wheel profile or a coupling that a module standard accepts: a <see cref="Data.Feature"/>
/// of the category WHEELS or COUPLINGS, for the standard's scale or any scale. A standard may accept several of each; one without
/// any of a category has those of the standard it extends. It has an id of its own, so its changes are in the change log.
/// </summary>
public sealed class ModuleFeature
{
    public int Id { get; set; }
    public int ModuleStandardId { get; set; }
    public int FeatureId { get; set; }

    public ModuleStandard ModuleStandard { get; set; } = null!;
    public Feature Feature { get; set; } = null!;
}

public static class ModuleFeatureMapping
{
    extension(ModelBuilder modelBuilder)
    {
        internal void MapModuleFeature() =>
            modelBuilder.Entity<ModuleFeature>(entity =>
            {
                entity.ToTable("ModuleFeature");

                entity.HasOne(d => d.ModuleStandard)
                    .WithMany(p => p.Features)
                    .HasForeignKey(d => d.ModuleStandardId)
                    .HasConstraintName("FK_ModuleFeature_ModuleStandard")
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Feature)
                    .WithMany()
                    .HasForeignKey(d => d.FeatureId)
                    .HasConstraintName("FK_ModuleFeature_Feature")
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => new { e.ModuleStandardId, e.FeatureId }, "IX_ModuleFeature")
                    .IsUnique();
            });
    }
}