using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class CargoQuantityUnit
{
    public CargoQuantityUnit()
    {
    }

    public int Id { get; set; }
    public string FullName { get; set; }
    public string Designation { get; set; }
    public string SingularResourceCode { get; set; }
    public string PluralResourceCode { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsBearer { get; set; }
}

#nullable enable

public static class CargoQuantityUnitExtensions
{
    public static bool IsContainer(this CargoQuantityUnit? it) => it is not null && it.FullName == "Containers";
}

internal static class QuantityUnitMapper
{
    public static void MapCargoQuantityUnit(this ModelBuilder builder) =>
        builder.Entity<CargoQuantityUnit>(entity =>
        {
            entity.ToTable("CargoUnit");

            entity.Property(e => e.Designation)
                .IsRequired()
                .HasMaxLength(6);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(12);

            entity.Property(e => e.SingularResourceCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.PluralResourceCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.DisplayOrder)
                .IsRequired();
        });

}
