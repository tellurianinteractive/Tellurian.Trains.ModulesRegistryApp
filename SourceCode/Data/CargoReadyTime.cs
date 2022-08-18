#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class CargoReadyTime
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string ShortName { get; set; }
    public bool IsSpecifiedInLayout { get; set; }
}

#nullable enable

internal static class CargoReadyTimeMapper
{
    public static void MapCargoReadyTime(this ModelBuilder builder) =>
        builder.Entity<CargoReadyTime>(entity =>
        {
            entity.ToTable("CargoReadyTime");

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(10);
        });
}
