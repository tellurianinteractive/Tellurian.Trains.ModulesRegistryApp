#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class OperatingBasicDay
{
    public int OperatingDayId { get; set; }
    public int BasicDayId { get; set; }

    public virtual OperatingDay BasicDay { get; set; }
    public virtual OperatingDay OperatingDay { get; set; }
}

public static class OperatingBasicDayMapper
{
    internal static void MapOperatingBasicDay(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OperatingBasicDay>(entity =>
        {
            entity.HasKey(e => new { e.OperatingDayId, e.BasicDayId });

            entity.ToTable("OperatingBasicDay");

            entity.HasOne(d => d.BasicDay)
                .WithMany(p => p.OperatingBasicDayBasicDays)
                .HasForeignKey(d => d.BasicDayId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.OperatingDay)
                .WithMany(p => p.OperatingBasicDayOperatingDays)
                .HasForeignKey(d => d.OperatingDayId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
    }
}
