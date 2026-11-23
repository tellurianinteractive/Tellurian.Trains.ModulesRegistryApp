using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

#nullable disable

public class CargoPackagingUnit
{
    public int Id { get; set; }
    public string SingularResourceCode { get; set; }
    public string PluralResourceCode { get; set; }
    public int DisplayOrder { get; set; }
    public string PrepositionResourceCode { get; set; }
    public bool IsContainer { get; set; }
}

#nullable enable

internal static class CargoPackagingUnitMapper
{
    public static void MapCargoPackagingUnit(this ModelBuilder builder) =>
        builder.Entity<CargoPackagingUnit>(entity =>
        {
            entity.ToTable("CargoPackagingUnit");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.SingularResourceCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PluralResourceCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.PrepositionResourceCode).HasMaxLength(4);
            entity.Property(e => e.IsContainer).HasDatabaseDefault("0");
        });
}
