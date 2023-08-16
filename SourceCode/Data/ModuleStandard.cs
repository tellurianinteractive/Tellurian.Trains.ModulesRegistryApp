#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class ModuleStandard
{
    public int Id { get; set; }
    public string ShortName { get; set; }
    public int ScaleId { get; set; }
    public string TrackSystem { get; set; }
    public double? NormalGauge { get; set; }
    public double? NarrowGauge { get; set; }
    public string Wheelset { get; set; }
    public string Couplings { get; set; }
    public string Electricity { get; set; }
    public string PreferredTheme { get; set; }
    public string AcceptedNorm { get; set; }
    public string MainTheme { get; set; } = "EUROPE";

    public virtual Scale Scale { get; set; }
}

public static class ModuleStandardMapping
{
    internal static void MapModuleStandard(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ModuleStandard>(entity =>
        {
            entity.ToTable("ModuleStandard");

            entity.Property(e => e.AcceptedNorm)
                .HasMaxLength(255);

            entity.Property(e => e.Couplings)
                .HasMaxLength(20);

            entity.Property(e => e.Electricity)
                .HasMaxLength(20);

            entity.Property(e => e.PreferredTheme)
                .HasMaxLength(20);

            entity.Property(e => e.ShortName)
                .HasMaxLength(10);

            entity.Property(e => e.TrackSystem)
                .HasMaxLength(20);

            entity.Property(e => e.Wheelset)
                .HasMaxLength(50);

            entity.HasOne(d => d.Scale)
                .WithMany(p => p.ModuleStandards)
                .HasForeignKey(d => d.ScaleId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });
}