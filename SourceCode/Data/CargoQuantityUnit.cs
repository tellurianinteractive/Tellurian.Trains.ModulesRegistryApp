using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class CargoQuantityUnit
{
    public CargoQuantityUnit()
    {
    }

    public int Id { get; set; }
    [Obsolete("Use the resoutce codes.")]
    public string FullName { get; set; }
    public string Designation { get; set; }
    public string SingularResourceCode { get; set; }
    public string PluralResourceCode { get; set; }
    public int DisplayOrder { get; set; }
}

#nullable enable

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
