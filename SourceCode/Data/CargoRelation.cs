using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class CargoRelation
{
    public int Id { get; set; }
    public int SupplierStationCustomerCargoId { get; set; }
    public int ConsumerStationCustomerCargoId { get; set; }
    public int DefaultWagonClassId { get; set; }
    public int? OperatingDayId { get; set; }
    public int? OperatorId { get; set; }
    public int? Layout { get; set; }

    public virtual StationCustomerCargo ConsumerStationCustomerCargo { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }
    public virtual Operator Operator { get; set; }
    public virtual StationCustomerCargo SupplierStationCustomerCargo { get; set; }
}

#nullable enable

internal static class CargoRelationMapper
{
    public static void MapCargoRelation(this ModelBuilder builder) =>
        builder.Entity<CargoRelation>(entity =>
        {
            entity.ToTable("CargoRelation");

            entity.HasOne(d => d.OperatingDay)
                .WithMany()
                .HasForeignKey(d => d.OperatingDayId);

            entity.HasOne(d => d.SupplierStationCustomerCargo)
               .WithMany()
               .HasForeignKey(d => d.SupplierStationCustomerCargoId);

            entity.HasOne(d => d.ConsumerStationCustomerCargo)
                 .WithMany()
                 .HasForeignKey(d => d.ConsumerStationCustomerCargoId);
        });
}
