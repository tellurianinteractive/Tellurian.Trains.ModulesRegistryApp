#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;
public class Vehicle
{
    public int Id { get; set; }
    public string KeeperSignature { get; set; }
    public string VehicleClass { get; set; }
    public string VehicleNumber { get; set; }
    public int? VehicleInteroperabilityCode { get; set; }
    public virtual VehicleInteroperability VehicleInteroperability { get; set; }
    public string PrototypeManufacturerName { get; set; }
    public short? PrototypeManufactureYear { get; set; }
    public short? EnginePower { get; set; }
    public decimal? PrototypeLength { get; set; }
    public decimal? PrototypeWeight { get; set; }
    public string PrototypeImageHref { get; set; }
    public string Theme { get; set; }
    public short? ThisEmbodiementFromYear { get; set; }
    public short? ThisEmbodiementUptoYear { get; set; }
    public string ModelManufacturerName { get; set; }
    public string ModelNumber { get; set; }
    public string ModelImageHref { get; set; }
    public int InventoryNumber { get; set; }
    public int? CouplingFeatureId { get; set; }
    public virtual VehicleFeature CouplingFeature { get; set; }
    public int? WheelsFeatureId { get; set; }
    public virtual VehicleFeature WheelsFeature { get; set; }
    public int? TractionFeatureId { get; set; }
    public virtual VehicleFeature TractionFeature { get; set; }
    public bool IsWeathered { get; set; }
    public short? DccAddress { get; set; }
    public string DecoderType { get; set; }
    public bool HasSound { get; set; }
    public bool HasRemoteCouplings { get; set; }
    public string Note { get; set; }
    public int? VehicleOperatorId { get; set; }
    public virtual VehicleOperator VehicleOperator { get; set; }
    public int? KeeperCountryId { get; set; }
    public virtual Country KeeperCountry { get; set; }
    public int ScaleId { get; set; }
    public virtual Scale Scale { get; set; }
    public int OwningPersonId { get; set; }
    public virtual Person OwningPerson { get; set; }

    public string F0 { get; set; }
    public string F1 { get; set; }
    public string F2 { get; set; }
    public string F3 { get; set; }
    public string F4 { get; set; }
    public string F5 { get; set; }
    public string F6 { get; set; }
    public string F7 { get; set; }
    public string F8 { get; set; }
    public string F9 { get; set; }
    public string F10 { get; set; }
    public string F11 { get; set; }
    public string F12 { get; set; }
    public string F13 { get; set; }
    public string F14 { get; set; }
    public string F15 { get; set; }
    public string F16 { get; set; }
    public string F17 { get; set; }
    public string F18 { get; set; }
    public string F19 { get; set; }
    public string F20 { get; set; }
    public string F21 { get; set; }
    public string F22 { get; set; }
    public string F23 { get; set; }
    public string F24 { get; set; }
    public string F25 { get; set; }
    public string F26 { get; set; }
    public string F27 { get; set; }
    public string F28 { get; set; }
    public string F29 { get; set; }
}


internal static class VehicleMapper
{
    public static void MapVehicle(this ModelBuilder builder)
    {
        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicle");
            entity.Property(e => e.PrototypeLength).HasPrecision(9, 3);
            entity.Property(e => e.PrototypeWeight).HasPrecision(9, 3);
            entity.HasOne(e => e.KeeperCountry)
                .WithMany()
                .HasForeignKey(e => e.KeeperCountryId);
            entity.HasOne(e => e.VehicleOperator)
                .WithMany(vo => vo.Vehicles)
                .HasForeignKey(e => e.VehicleOperatorId);
            entity.HasOne(e => e.VehicleInteroperability)
                .WithMany()
                .HasForeignKey(e => e.VehicleInteroperabilityCode);
            entity.HasOne(e => e.Scale)
                .WithMany(s => s.Vehicles)
                .HasForeignKey(e => e.ScaleId)
                .OnDelete(DeleteBehavior.ClientCascade);
            entity.HasOne(e => e.OwningPerson)
                .WithMany(p => p.Vehicles)
                .HasForeignKey(e => e.OwningPersonId)
                .OnDelete(DeleteBehavior.ClientCascade);
            entity.HasOne(e => e.CouplingFeature)
                .WithMany()
                .HasForeignKey(e => e.CouplingFeatureId);
            entity.HasOne(e => e.WheelsFeature)
                .WithMany()
                .HasForeignKey(e => e.WheelsFeatureId);
            entity.HasOne(e => e.TractionFeature)
                .WithMany()
                .HasForeignKey(e => e.TractionFeatureId);
            entity.Property(e => e.KeeperSignature).HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property<int?>("VehicleType");
            entity.Property(e => e.VehicleClass).HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property(e => e.VehicleNumber).HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.PrototypeManufacturerName).HasMaxLength(20);
            entity.Property<int?>("EnginePowerUnit").HasDefaultValueSql("0");
            entity.Property(e => e.PrototypeImageHref).HasMaxLength(512).IsUnicode(false);
            entity.Property(e => e.ModelManufacturerName).HasMaxLength(20);
            entity.Property<byte[]>("ModelImage");
            entity.Property(e => e.ModelImageHref).HasMaxLength(512).IsUnicode(false);
            entity.Property(e => e.ModelNumber).HasMaxLength(16);
            entity.Property(e => e.IsWeathered).HasDatabaseDefault("0");
            entity.Property(e => e.Theme).HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property(e => e.DecoderType).HasMaxLength(20);
            entity.Property(e => e.HasSound).HasDatabaseDefault("0");
            entity.Property(e => e.HasRemoteCouplings).HasDatabaseDefault("0");
            entity.Property(e => e.F0).HasMaxLength(12);
            entity.Property(e => e.F1).HasMaxLength(12);
            entity.Property(e => e.F2).HasMaxLength(12);
            entity.Property(e => e.F3).HasMaxLength(12);
            entity.Property(e => e.F4).HasMaxLength(12);
            entity.Property(e => e.F5).HasMaxLength(12);
            entity.Property(e => e.F6).HasMaxLength(12);
            entity.Property(e => e.F7).HasMaxLength(12);
            entity.Property(e => e.F8).HasMaxLength(12);
            entity.Property(e => e.F9).HasMaxLength(12);
            entity.Property(e => e.F10).HasMaxLength(12);
            entity.Property(e => e.F11).HasMaxLength(12);
            entity.Property(e => e.F12).HasMaxLength(12);
            entity.Property(e => e.F13).HasMaxLength(12);
            entity.Property(e => e.F14).HasMaxLength(12);
            entity.Property(e => e.F15).HasMaxLength(12);
            entity.Property(e => e.F16).HasMaxLength(12);
            entity.Property(e => e.F17).HasMaxLength(12);
            entity.Property(e => e.F18).HasMaxLength(12);
            entity.Property(e => e.F19).HasMaxLength(12);
            entity.Property(e => e.F20).HasMaxLength(12);
            entity.Property(e => e.F21).HasMaxLength(12);
            entity.Property(e => e.F22).HasMaxLength(12);
            entity.Property(e => e.F23).HasMaxLength(12);
            entity.Property(e => e.F24).HasMaxLength(12);
            entity.Property(e => e.F25).HasMaxLength(12);
            entity.Property(e => e.F26).HasMaxLength(12);
            entity.Property(e => e.F27).HasMaxLength(12);
            entity.Property(e => e.F28).HasMaxLength(12);
            entity.Property(e => e.F29).HasMaxLength(12);
            entity.Property<decimal?>("SellingPrice").HasPrecision(9, 2);
            // UNIQUE constraints in the database. HasFilter(null): EF would otherwise leave out rows with NULL, as SQL Server does not.
            entity.HasIndex(e => new { e.OwningPersonId, e.InventoryNumber }, "UX_Vehicle_InventoryNumber").IsUnique().HasFilter(null);
        });
    }
}

#nullable enable
public static class VehicleExtensions
{
    public static bool HasImage(this Vehicle? vehicle) => vehicle?.PrototypeImageHref?.Length > 10 == true;
}