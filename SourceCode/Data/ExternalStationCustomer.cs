using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class ExternalStationCustomer
{
    public ExternalStationCustomer()
    {
        ExternalStationCustomerCargos = new HashSet<ExternalStationCustomerCargo>();
    }

    public int Id { get; set; }
    public int ExternalStationId { get; set; }
    public string CustomerName { get; set; }
    public short? OpenedYear { get; set; }
    public short? ClosedYear { get; set; }

    public virtual ExternalStation ExternalStation { get; set; }
    public virtual ICollection<ExternalStationCustomerCargo> ExternalStationCustomerCargos { get; set; }
}

#nullable enable

internal static class ExternalStationCustomerMapper
{
    public static void MapExternalStationCustomer(this ModelBuilder builder) =>
        builder.Entity<ExternalStationCustomer>(entity =>
        {
            entity.ToTable("ExternalStationCustomer");

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.ExternalStation)
                .WithMany(p => p.ExternalStationCustomers)
                .HasForeignKey(d => d.ExternalStationId);
        });
}
