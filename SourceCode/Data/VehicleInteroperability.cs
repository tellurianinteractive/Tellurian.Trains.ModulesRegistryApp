#nullable disable

using Microsoft.EntityFrameworkCore;

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

        });
    }
}
