using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public partial class CargoDirection
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string ShortName { get; set; }
    public bool IsSupply { get; set; }

}


internal static class CargoDirectionMapper
{
    public static void MapCargoDirection(this ModelBuilder builder) =>
        builder.Entity<CargoDirection>(entity =>
        {
            entity.ToTable("CargoDirection");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.FullName).HasMaxLength(10);
            entity.Property(e => e.ShortName).HasMaxLength(4);
            entity.Property<bool>("IsInternational").HasDatabaseDefault("0");
        });
}
