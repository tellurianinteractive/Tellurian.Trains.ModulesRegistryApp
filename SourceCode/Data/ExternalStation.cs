using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class ExternalStation
{
    public ExternalStation()
    {
        ExternalStationCustomers = new HashSet<ExternalStationCustomer>();
    }

    public int Id { get; set; }
    public int RegionId { get; set; }
    public string FullName { get; set; }
    public string Signature { get; set; }
    public string InternationalName { get; set; }
    public string Note { get; set; }
    public short? OpenedYear { get; set; }
    public short? ClosedYear { get; set; }
    public virtual Region Region { get; set; }
    public virtual ICollection<ExternalStationCustomer> ExternalStationCustomers { get; set; }
}

#nullable enable

internal static class ExternalStationMapper
{
    public static void MapExternalStation(this ModelBuilder builder) =>
        builder.Entity<ExternalStation>(entity =>
        {
            entity.ToTable("ExternalStation");

            entity.Property(e => e.Note).HasMaxLength(20);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Signature)
                .IsRequired()
                .HasMaxLength(6);

            entity.HasOne(d => d.Region)
                .WithMany(p => p.ExternalStations)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
}