#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;
public class VehicleFeature
{
    public int Id { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public bool IsResourceCode { get; set; }
    public int? OnlyScaleId { get; set; }

    public Scale OnlyScale { get; set; }
}

public static class VehicleFeatureMapper
{
    public static void MapVehicleFeature(this ModelBuilder builder)
    {
        builder.Entity<VehicleFeature>(entity =>
        {
            entity.ToTable("VehicleFeature");
            entity.HasOne(e => e.OnlyScale)
            .WithMany()
                .HasForeignKey(e => e.OnlyScaleId);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Category).HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(20);
            entity.Property(e => e.IsResourceCode).HasDatabaseDefault("0");
            // UNIQUE constraints in the database. HasFilter(null): EF would otherwise leave out rows with NULL, as SQL Server does not.
            entity.HasIndex(e => new { e.Category, e.Description, e.OnlyScaleId }, "UX_VehicleFeatyre").IsUnique().HasFilter(null);
        });
    }
}
