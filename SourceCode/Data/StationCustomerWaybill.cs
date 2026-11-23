using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

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
    public int? OtherStationCustomerCargoId { get; set; }
    public int? OtherExternalCustomerCargoId { get; set; }
    public int? OtherRegionId { get; set; }
    public int OperatingDayId { get; set; }
    public int SequenceNumber { get; set; }
    public bool IsManuallyCreated { get; set; }
    public bool HasEmptyReturn { get; set; }
    public bool HasSameCargoReturn { get; set; }
    public bool HideLoadingTimes { get; set; }
    public bool HideUnloadingTimes { get; set; }
    public bool PrintPerOperatingDay { get; set; }
    public int PrintCount { get; set; }

    public virtual StationCustomer StationCustomer { get; set; }
    public virtual StationCustomerCargo StationCustomerCargo { get; set; }
    public virtual StationCustomerCargo OtherStationCustomerCargo { get; set; }
    public virtual ExternalStationCustomerCargo OtherExternalCustomerCargo { get; set; }
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
                .HasForeignKey(e => e.StationCustomerId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.StationCustomerCargo)
               .WithMany()
               .HasForeignKey(e => e.StationCustomerCargoId)
               .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.OtherStationCustomerCargo)
                .WithMany()
                .HasForeignKey(e => e.OtherStationCustomerCargoId);

            entity.HasOne(e => e.OtherExternalCustomerCargo)
               .WithMany()
               .HasForeignKey(e => e.OtherExternalCustomerCargoId);

            entity.HasOne(e => e.OtherRegion)
                .WithMany()
                .HasForeignKey(e => e.OtherRegionId);

            entity.HasOne(e => e.OperatingDay)
                .WithMany()
                .HasForeignKey(e => e.OperatingDayId)
                .OnDelete(DeleteBehavior.ClientCascade);
            entity.Property(e => e.OperatingDayId).HasDatabaseDefault("8");
            entity.Property(e => e.IsManuallyCreated).HasDatabaseDefault("0");
            entity.Property(e => e.HasEmptyReturn).HasDatabaseDefault("0");
            entity.Property(e => e.HasSameCargoReturn).HasDatabaseDefault("0");
            entity.Property(e => e.HideLoadingTimes).HasDatabaseDefault("0");
            entity.Property(e => e.HideUnloadingTimes).HasDatabaseDefault("0");
            entity.Property(e => e.PrintPerOperatingDay).HasDatabaseDefault("0");
            entity.Property(e => e.PrintCount).HasDatabaseDefault("1");
            entity.Property(e => e.SequenceNumber).HasDatabaseDefault("1");
            entity.HasIndex(e => e.OtherExternalCustomerCargoId, "IX_StationCustomerWaybill_OtherExternalCustomerCargoId");
            entity.HasIndex(e => new { e.StationCustomerId, e.StationCustomerCargoId }, "IX_StationCustomerWaybill_StationCustomerId").IncludeProperties(e => new { e.OtherStationCustomerCargoId, e.OtherExternalCustomerCargoId, e.OtherRegionId, e.OperatingDayId });
        });
}
