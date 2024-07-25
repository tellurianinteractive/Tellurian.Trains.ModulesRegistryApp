#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class VehicleOperator
{
    public int Id { get; set; }
    public string Signature { get; set; }
    public string ShortName { get; set; }
    public string FullName { get; set; }
    public short? RICS { get; set; }
    public byte[] LogotypeImage { get; set; }
    public short? FirstYearInOperation { get; set; }
    public short? LastYearInOperation { get; set; }
    public bool IsPassengerOperator { get; set; }
    public bool IsFreightOperator { get; set; }
    public bool IsConstructionOperator { get; set; }
    public bool IsVeteranOperator { get; set; }
    public int PrimaryOperatingCountryId { get; set; }
    public virtual Country PrimaryOperatingCountry { get; set; }
    public int? OnlyInLayoutId { get; set; }
    public virtual Layout OnlyInLayout { get; set; }
    public virtual ICollection<Vehicle> Vehicles { get; set; }
}

internal static class VehicleOperatorMapper
{
    public static void MapVehicleOperator(this ModelBuilder builder)
    {
        builder.Entity<VehicleOperator>(entity =>
        {
            entity.ToTable("VehicleOperator");
            entity.HasOne(e => e.PrimaryOperatingCountry)
                .WithMany()
                .HasForeignKey(e => e.PrimaryOperatingCountryId);
            entity.HasOne(e => e.OnlyInLayout)
                .WithMany()
                .HasForeignKey(e => e.OnlyInLayoutId);
        });
    }
}

