#nullable disable

using Microsoft.EntityFrameworkCore;

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
        });
    }
}
