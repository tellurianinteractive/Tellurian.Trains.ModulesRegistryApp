#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;
public class Vehicle
{
    public int Id { get; set; }
    public string KeeperSignature { get; set; }
    public string VehicleClass {  get; set; }
    public string VehicleNumber { get; set; }
    public int InteroperabilityId { get; set; }
    public string PrototypeManufacturerName { get; set; }
    public string Theme { get; set; }
    public short? ThisEmbodiementFromYear { get; set; }
    public short? ThisEmbodiementUptoYear { get; set; }
    public string ModelManufacturerName { get; set; }
    public string ModelNumber { get; set; }
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
    public int ScaleId { get; set; }
    public virtual Scale Scale { get; set; }
    public int OwningPersonId { get; set; }
    public virtual Person OwningPerson { get; set; }

    public void FormatData()
    {
        KeeperSignature = KeeperSignature.ToUpperInvariant();
    }
}

internal static class VehicleMapper
{
    public static void MapVehicle(this ModelBuilder builder)
    {
        builder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicle");
            entity.HasOne(e => e.Scale)
                .WithMany(s => s.Vehicles)
                .HasForeignKey(e => e.ScaleId);
            entity.HasOne(e => e.OwningPerson)
                .WithMany(p => p.Vehicles)
                .HasForeignKey(e => e.OwningPersonId);
            entity.HasOne(e => e.CouplingFeature)
                .WithMany()
                .HasForeignKey(e => e.CouplingFeatureId);
            entity.HasOne(e => e.WheelsFeature)
                .WithMany()
                .HasForeignKey(e => e.WheelsFeatureId);
            entity.HasOne(e => e.TractionFeature)
                .WithMany()
                .HasForeignKey(e => e.TractionFeatureId);
        });
    }
}
