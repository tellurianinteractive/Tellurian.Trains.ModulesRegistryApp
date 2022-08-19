using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public class StationCustomerWaybill
{
    public static StationCustomerWaybill Default(int stationCustomerId) =>
        new()
        {
            StationCustomerId = stationCustomerId,
            OperatingDayId = OperatingDay.Daily,
            IsManuallyCreated = true,
        };
    public int Id { get; set; }
    public int StationCustomerId { get; set; }
    public int StationCustomerCargoId { get; set; }
    public int? OtherCustomerCargoId { get; set; }
    public int? OtherRegionId { get; set; }
    public int OperatingDayId { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsManuallyCreated { get; set; }
    public bool HasEmptyReturn { get; set; }
    public bool IsExpressCargo { get; set; }
    public bool IsCoolingTransport { get; set; }
    public bool HideLoadingTimes { get; set; }
    public bool HideUnloadingTimes { get; set; }
    public bool PrintPerOperatingDay { get; set; }
    public int PrintCount { get; set; }

    public virtual StationCustomer StationCustomer { get; set; }
    public virtual StationCustomerCargo StationCustomerCargo { get; set; }
    public virtual StationCustomerCargo OtherCustomerCargo { get; set; }
    public virtual Region OtherRegion { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }
}

#nullable enable

internal static class StationCustomerWaybillMapper
{
    public static void MapStationCustomerWaybill(this ModelBuilder builder) =>
        builder.Entity<StationCustomerWaybill>(entity =>
        {
            entity.ToTable("StationCustomerWaybill");

            entity.HasOne(e => e.StationCustomer)
                .WithMany(e => e.Waybills)
                .HasForeignKey(e => e.StationCustomerId);

            entity.HasOne(e => e.StationCustomerCargo)
               .WithMany()
               .HasForeignKey(e => e.StationCustomerCargoId);

            entity.HasOne(e => e.OtherCustomerCargo)
                .WithMany()
                .HasForeignKey(e => e.OtherCustomerCargoId);

            entity.HasOne(e => e.OtherRegion)
                .WithMany()
                .HasForeignKey(e => e.OtherRegionId);

            entity.HasOne(e => e.OperatingDay)
                .WithMany()
                .HasForeignKey(e => e.OperatingDayId);

        });
}
