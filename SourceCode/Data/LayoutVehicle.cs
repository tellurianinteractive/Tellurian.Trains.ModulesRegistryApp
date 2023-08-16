using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;
public class LayoutVehicle
{
    public int Id { get; set; }
    public short? DccAddress { get; set; }
    public int LayoutId { get; set; }
    public string? OperatorSignature { get; set; }
    public string? VehicleClass { get; set; }
    public string? VehicleNumber { get; set; }
    public string? VehicleProviderName { get; set; }
    public string? ThrottleIdentity { get; set; }
    public bool IsTractionUnit { get; set; }
}

internal static class LayoutVehicleMapper
{
    public static void MapLayoutVehicle(this ModelBuilder builder) =>
    builder
        .Entity<LayoutVehicle>(
            lv =>
            {
                lv.HasNoKey();
                lv.ToView("LayoutVehicles");
            });
}

