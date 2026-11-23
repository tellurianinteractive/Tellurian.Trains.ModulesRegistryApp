#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public class VehicleInteroperability
{
    public int Code { get; set; }
    public string Description { get; set; }
    public bool AppliesToTractionUnits { get; set; }
    public bool AppliesToFreightWagons { get; set; }
    public bool AppliesToPassengerCars { get; set; }
    public bool AppliesToBogieWagons { get; set; }
    public bool AppliesToCarCarryingWagons { get; set; }
    public bool? IsForInternationalUse { get; set; }
    public bool? IsCompliantWithInternationalStandard {  get; set; }
    public bool? InternationalUseRequiresSpecialAgreement { get; set; }
    public bool IsSpecialVehicle { get; set; }
    public bool WithAirCondition { get; set; }
}
internal static class VehicleInteroperabilityMapper
{
    public static void MapVehicleInteroperability(this ModelBuilder builder)
    {
        builder.Entity<VehicleInteroperability>(entity =>
        {
            entity.ToTable("VehicleInteroperability");
            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).ValueGeneratedNever();
            entity.Property(e => e.Description).HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.AppliesToTractionUnits).HasDatabaseDefault("0");
            entity.Property(e => e.AppliesToFreightWagons).HasDatabaseDefault("0");
            entity.Property(e => e.AppliesToPassengerCars).HasDatabaseDefault("0");
            entity.Property(e => e.AppliesToBogieWagons).HasDatabaseDefault("0");
            entity.Property(e => e.AppliesToCarCarryingWagons).HasDatabaseDefault("0");
            entity.Property(e => e.IsForInternationalUse).HasDatabaseDefault("0");
            entity.Property(e => e.IsCompliantWithInternationalStandard).HasDatabaseDefault("0");
            entity.Property(e => e.InternationalUseRequiresSpecialAgreement).HasDatabaseDefault("0");
            entity.Property(e => e.IsSpecialVehicle).HasDatabaseDefault("0");
            entity.Property(e => e.WithAirCondition).HasDatabaseDefault("0");
        });
    }
}
