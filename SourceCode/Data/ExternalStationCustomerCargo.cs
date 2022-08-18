using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class ExternalStationCustomerCargo
{
    public static ExternalStationCustomerCargo Default(int customerId) => 
        new () { ExternalStationCustomerId = customerId, DirectionId = 1, QuantityUnitId = 4, OperatingDayId = 8 };

    public int Id { get; set; }
    public int ExternalStationCustomerId { get; set; }
    public int CargoId { get; set; }
    public int PackageUnitId { get; set; }
    public string SpecificWagonClass { get; set; }
    public string SpecialCargoName { get; set; }
    public int DirectionId { get; set; }
    public int OperatingDayId { get; set; }
    public int QuantityUnitId { get; set; }
    public int Quantity { get; set; }
    public short? FromYear { get; set; }
    public short? UptoYear { get; set; }

    public virtual Cargo Cargo { get; set; }
    public virtual CargoDirection Direction { get; set; }
    public virtual ExternalStationCustomer ExternalStationCustomer { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }
    public virtual CargoQuantityUnit QuantityUnit { get; set; }
}

#nullable enable
public static class ExternalStationCustomerCargoExtensions
{
    public static bool IsUnloading(this ExternalStationCustomerCargo me) => me.DirectionId == 1 || me.DirectionId == 3;
    public static bool IsLoading(this ExternalStationCustomerCargo me) => me.DirectionId == 2 || me.DirectionId == 4;

    public static string CargoTypeName(this ExternalStationCustomerCargo? it, IEnumerable<ListboxItem>? cargoTypeItems) 
        => it is null ? string.Empty : string.IsNullOrWhiteSpace(it.SpecialCargoName) ? it.CargoType(cargoTypeItems) : it.SpecialCargoName;
    public static string CargoType(this ExternalStationCustomerCargo? it, IEnumerable<ListboxItem>? cargoTypeItems) 
        => it is null ? string.Empty : cargoTypeItems is not null ? cargoTypeItems.SingleOrDefault(i => i.Id == it.CargoId)?.Description ?? string.Empty : string.Empty;

    public static string CargoDirection(this ExternalStationCustomerCargo? it, IEnumerable<ListboxItem>? cargoDirectionItems) => 
        it is not null && cargoDirectionItems is not null ? cargoDirectionItems.SingleOrDefault(i => i.Id == it.DirectionId)?.Description ?? string.Empty : string.Empty;

    public static string OperatingDay(this ExternalStationCustomerCargo? it, IEnumerable<ListboxItem>? operatingDayItems) => 
        it is not null && operatingDayItems is not null ? operatingDayItems.SingleOrDefault(i => i.Id == it.OperatingDayId)?.Description ?? string.Empty : string.Empty;

    public static string PackagingUnit(this ExternalStationCustomerCargo it, IEnumerable<ListboxItem>? packagingUnitItems) 
        => it is null || packagingUnitItems is null ? string.Empty : packagingUnitItems.SingleOrDefault(i => i.Id == it.PackageUnitId)?.Description ?? string.Empty;

    public static string QuantityUnit(this ExternalStationCustomerCargo? it, IEnumerable<ListboxItem>? quantityUnitItems) 
        => it is not null && quantityUnitItems is not null ? quantityUnitItems.SingleOrDefault(i => i.Id == it.QuantityUnitId)?.Description ?? string.Empty : string.Empty;
}

internal static class ExternalStationCustomerCargoMapper
{
    public static void MapExternalStationCustomerCargo(this ModelBuilder builder) =>
        builder.Entity<ExternalStationCustomerCargo>(entity =>
        {
            entity.ToTable("ExternalStationCustomerCargo");

            entity.Property(e => e.SpecialCargoName).HasMaxLength(20);

            entity.Property(e => e.SpecificWagonClass).HasMaxLength(10);

            entity.HasOne(e => e.Cargo)
                .WithMany()
                .HasForeignKey(d => d.CargoId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Direction)
                .WithMany()
                .HasForeignKey(d => d.DirectionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.ExternalStationCustomer)
                .WithMany(p => p.ExternalStationCustomerCargos)
                .HasForeignKey(d => d.ExternalStationCustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OperatingDay)
                .WithMany()
                .HasForeignKey(d => d.OperatingDayId);

            entity.HasOne(e => e.QuantityUnit)
                .WithMany()
                .HasForeignKey(d => d.QuantityUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
}
