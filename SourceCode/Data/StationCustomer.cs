#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class StationCustomer
{
    public const string ImportingCustomerResourceName = "ImportAgent";
    public const string ExportingCustomerResourceName = "ExportAgent";

    public StationCustomer()
    {
        Cargos = new HashSet<StationCustomerCargo>();
        Waybills = new HashSet<StationCustomerWaybill>();
    }

    public int Id { get; set; }
    public int StationId { get; set; }
    public int? LayoutId { get; set; }
    public string CustomerName { get; set; }
    public string Comment { get; set; }
    public string TrackOrArea { get; set; }
    public string TrackOrAreaColor { get; set; }
    public short? OpenedYear { get; set; }
    public short? ClosedYear { get; set; }

    public virtual Station Station { get; set; }
    public virtual ICollection<StationCustomerCargo> Cargos { get; set; }
    public virtual ICollection<StationCustomerWaybill> Waybills { get; set; }

}

#nullable enable
public static class StationCustomerExtensions
{
    extension(ModelBuilder builder)
    {
        internal void MapStationCustomer() =>
            builder.Entity<StationCustomer>(entity =>
            {
                entity.ToTable("StationCustomer", tb => tb.HasTrigger("DeleteStationCustomer"));

                entity.Property(e => e.Comment)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.TrackOrArea).HasMaxLength(20);

                entity.Property(e => e.TrackOrAreaColor).HasMaxLength(7).IsFixedLength();

                entity.HasOne(d => d.Station)
                    .WithMany(p => p.StationCustomers)
                    .HasForeignKey(d => d.StationId)
                    .OnDelete(DeleteBehavior.ClientCascade);
                entity.HasIndex(e => e.StationId, "IX_StationCustomer_StationId");
            });
    }
}