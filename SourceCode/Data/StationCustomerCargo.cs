#nullable disable

using Microsoft.EntityFrameworkCore;

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
}

public static class StationCustomerCargoMapping
{
    public static void MapStationCustomerCargo(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<StationCustomerCargo>(entity =>
        {
            entity.ToTable("StationCustomerCargo",
                tb => tb.HasTrigger("DeleteStationCustomerCargo"));

            entity.HasOne(d => d.StationCustomer)
              .WithMany(p => p.Cargos)
              .HasForeignKey(d => d.StationCustomerId)
              .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Cargo)
               .WithMany()
               .HasForeignKey(d => d.CargoId)
               .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Direction)
              .WithMany()
              .HasForeignKey(d => d.DirectionId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ReadyTime)
              .WithMany()
              .HasForeignKey(d => d.ReadyTimeId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.QuantityUnit)
              .WithMany()
              .HasForeignKey(d => d.QuantityUnitId)
              .OnDelete(DeleteBehavior.ClientSetNull);

            entity.Property(e => e.QuantityUnitId)
             .HasDefaultValueSql("((4))");

            entity.Property(e => e.TrackOrArea)
               .HasMaxLength(10);

            entity.Property(e => e.TrackOrAreaColor)
              .HasMaxLength(10)
              .IsFixedLength(true);

            entity.Property(e => e.SpecialCargoName)
              .HasMaxLength(20);

            entity.Property(e => e.SpecificWagonClass)
             .HasMaxLength(10);

            entity.HasOne(d => d.OperatingDay)
              .WithMany()
              .HasForeignKey(d => d.OperatingDayId);

        });
}