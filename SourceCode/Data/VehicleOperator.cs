#nullable disable

using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

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
                .HasForeignKey(e => e.PrimaryOperatingCountryId)
                .OnDelete(DeleteBehavior.ClientCascade);
            entity.HasOne(e => e.OnlyInLayout)
                .WithMany()
                .HasForeignKey(e => e.OnlyInLayoutId);
            entity.Property(e => e.LastYearInOperation).HasColumnName("FinalYearInOperation");
            entity.Property(e => e.Signature).HasMaxLength(6).IsRequired();
            entity.Property(e => e.ShortName).HasMaxLength(30).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IsPassengerOperator).HasDatabaseDefault("0");
            entity.Property(e => e.IsFreightOperator).HasDatabaseDefault("0");
            entity.Property(e => e.IsConstructionOperator).HasDatabaseDefault("0");
            entity.Property(e => e.IsVeteranOperator).HasDatabaseDefault("0");
            // UNIQUE constraints in the database. HasFilter(null): EF would otherwise leave out rows with NULL, as SQL Server does not.
            entity.HasIndex(e => new { e.PrimaryOperatingCountryId, e.Signature, e.FirstYearInOperation, e.OnlyInLayoutId }, "UX_VehicleOperator").IsUnique().HasFilter(null);
        });
    }
}

