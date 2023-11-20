using Microsoft.EntityFrameworkCore;

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
        });
}
