using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

/// <summary>
/// A feature of a category, such as a wheel profile or a coupling, for one scale or all: what a vehicle has and what a module
/// standard accepts. It replaced the table VehicleFeature (2026-11-26), with the same rows and ids.
/// </summary>
public sealed class Feature
{
    public int Id { get; set; }
    /// <summary>One of <see cref="FeatureCategories"/>.</summary>
    public required string Category { get; set; }
    /// <summary>The scale the feature is for; empty for all scales.</summary>
    public int? OnlyScaleId { get; set; }
    /// <summary>The feature's name, or the resource key of its name when <see cref="IsResourceCode"/>.</summary>
    public string? Description { get; set; }
    public bool IsResourceCode { get; set; }

    public Scale? OnlyScale { get; set; }
}

public static class FeatureCategories
{
    public const string Wheels = "WHEELS";
    public const string Couplings = "COUPLINGS";
    public const string Traction = "TRACTION";
}

public static class FeatureMapping
{
    extension(ModelBuilder modelBuilder)
    {
        internal void MapFeature() =>
            modelBuilder.Entity<Feature>(entity =>
            {
                entity.ToTable("Feature");

                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.Category).HasMaxLength(10).IsUnicode(false).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(20);
                entity.Property(e => e.IsResourceCode).HasDatabaseDefault("0");

                entity.HasOne(e => e.OnlyScale)
                    .WithMany()
                    .HasForeignKey(e => e.OnlyScaleId)
                    .HasConstraintName("FK_Feature_Scale")
                    .OnDelete(DeleteBehavior.NoAction);

                // A UNIQUE constraint in the database. HasFilter(null): EF would otherwise leave out rows with NULL, as SQL Server does not.
                entity.HasIndex(e => new { e.Category, e.Description, e.OnlyScaleId }, "UX_Feature").IsUnique().HasFilter(null);
            });
    }
}