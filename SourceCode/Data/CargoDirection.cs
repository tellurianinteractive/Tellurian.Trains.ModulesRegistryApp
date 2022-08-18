using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class CargoDirection
{
    public CargoDirection()
    {
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public string ShortName { get; set; }
    public bool IsSupply { get; set; }

}

#nullable enable

internal static class CargoDirectionMapper
{
    public static void MapCargoDirection(this ModelBuilder builder) =>
        builder.Entity<CargoDirection>(entity =>
        {
            entity.ToTable("CargoDirection");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.ShortName)
                .IsRequired()
                .HasMaxLength(4);
        });
}
