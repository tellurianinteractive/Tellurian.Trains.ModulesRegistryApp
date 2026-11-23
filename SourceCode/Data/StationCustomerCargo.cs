#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;
using Microsoft.Extensions.Localization;

namespace ModulesRegistry.Data;

public partial class StationCustomerCargo
{
    public static StationCustomerCargo Default(int customerId) =>
        new()
        {
            StationCustomerId = customerId,
            DirectionId = 1,
            QuantityUnitId = 4,
            Quantity = 1,
            ReadyTimeId = 0,
            OperatingDayId = OperatingDay.Daily,
            TrackOrAreaColor = "#FFFFFF"
        };


    public StationCustomerCargo() { }
    public StationCustomerCargo(StationCustomerCargo other) => other.Clone();

    public int Id { get; set; }
    public int StationCustomerId { get; set; }
    public virtual StationCustomer StationCustomer { get; set; }
    public int CargoId { get; set; }
    public virtual Cargo Cargo { get; set; }
    public int DirectionId { get; set; }
    public virtual CargoDirection Direction { get; set; }
    public int QuantityUnitId { get; set; }
    public virtual CargoQuantityUnit QuantityUnit { get; set; }
    public int Quantity { get; set; }
    public int PackageUnitId { get; set; }
    public virtual CargoPackagingUnit PackageUnit { get; set; }
    public int OperatingDayId { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }
    public int ReadyTimeId { get; set; }
    public virtual CargoReadyTime ReadyTime { get; set; }

    public string TrackOrArea { get; set; }
    public string TrackOrAreaColor { get; set; }
    public string SpecificWagonClass { get; set; }
    public string SpecialCargoName { get; set; }
    public int? MaxTrainsetLength { get; set; }
    public short? FromYear { get; set; }
    public short? UptoYear { get; set; }

}

#nullable enable

public static class StationCustomerCargoExtensions
{
    public static StationCustomerCargo Clone(this StationCustomerCargo me) =>
       new()
       {
           Id = 0, // Id is not copied.
           StationCustomerId = me.StationCustomerId,
           CargoId = me.CargoId,
           DirectionId = me.DirectionId,
           QuantityUnitId = me.QuantityUnitId,
           PackageUnitId = me.PackageUnitId,
           OperatingDayId = me.OperatingDayId,
           ReadyTimeId = me.ReadyTimeId,

           Quantity = me.Quantity,
           TrackOrArea = me.TrackOrArea,
           TrackOrAreaColor = me.TrackOrAreaColor,
           SpecificWagonClass = me.SpecificWagonClass,
           SpecialCargoName = me.SpecialCargoName,
           MaxTrainsetLength = me.MaxTrainsetLength,
           FromYear = me.FromYear,
           UptoYear = me.UptoYear,
       };

    public static bool IsPackageUnitUnspecified(this StationCustomerCargo me) =>
        me.PackageUnitId == 0;

    public static bool IsReadyTimeUnspecified(this StationCustomerCargo me) =>
        me.ReadyTimeId == 0;

    public static bool IsImportOrExport(this StationCustomerCargo? me) =>
        me is not null && (me.DirectionId == 3 || me.DirectionId == 4);

    public static bool IsValid(this StationCustomerCargo? me) =>
        me is not null &&
        me.StationCustomerId > 0 &&
        me.CargoId > 0 &&
        me.DirectionId > 0 &&
        me.QuantityUnitId > 0 &&
        me.OperatingDayId > 0;

    

}

public static class StationCustomerCargoMapping
{
    public static void MapStationCustomerCargo(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<StationCustomerCargo>(entity =>
        {
            entity.ToTable("StationCustomerCargo",
                tb => tb.HasTrigger("DeleteStationCustomerCargo"));

            entity.HasOne(e => e.StationCustomer)
                .WithMany(p => p.Cargos)
                .HasForeignKey(e => e.StationCustomerId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.Cargo)
                .WithMany()
                .HasForeignKey(e => e.CargoId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.Direction)
                .WithMany()
                .HasForeignKey(e => e.DirectionId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.ReadyTime)
                .WithMany()
                .HasForeignKey(e => e.ReadyTimeId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.QuantityUnit)
                .WithMany()
                .HasForeignKey(e => e.QuantityUnitId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.PackageUnit)
                .WithMany()
                .HasForeignKey(e => e.PackageUnitId)
                .OnDelete(DeleteBehavior.ClientCascade);

            entity.HasOne(e => e.OperatingDay)
                .WithMany()
                .HasForeignKey(e => e.OperatingDayId)
                .OnDelete(DeleteBehavior.ClientCascade);
            entity.Property(e => e.PackageUnitId).HasDatabaseDefault("0");
            entity.Property(e => e.TrackOrArea).HasMaxLength(20);
            entity.Property(e => e.TrackOrAreaColor).HasMaxLength(7).IsFixedLength();
            entity.Property(e => e.SpecificWagonClass).HasMaxLength(10);
            entity.Property(e => e.SpecialCargoName).HasMaxLength(20);
            entity.Property(e => e.OperatingDayId).HasDatabaseDefault("0");
            entity.Property(e => e.Quantity).HasDatabaseDefault("0");
            entity.HasOne<VehicleOperator>()
                .WithMany()
                .HasForeignKey("OperatorId")
                .OnDelete(DeleteBehavior.ClientSetNull);
            entity.Property<bool>("EmptyReturn").HasDatabaseDefault("0");
            entity.Property<bool>("MatchReturn").HasDatabaseDefault("0");
            entity.HasIndex(e => e.StationCustomerId, "IX_StationCustomerCargo_StationCustomerId").IncludeProperties(e => new { e.CargoId, e.PackageUnitId, e.DirectionId, e.OperatingDayId, e.QuantityUnitId });
        });
}