using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public class NHM
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public byte LevelDigits { get; set; }
    public bool HasEmptyWagonOrder { get; set; }
    public string? DA { get; set; }
    public string? DE { get; set; }
    public string? EN { get; set; }
    public string? NL { get; set; }
    public string? PL { get; set; }
    public string? SV { get; set; }
}

public static class NHM_Mapper
{
    internal static void MapNHM(this ModelBuilder modelBuilder) =>
        modelBuilder.Entity<NHM>(entity =>
        {
            entity.ToTable("NHM");
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Code).HasMaxLength(8).IsFixedLength().IsRequired();
            entity.Property(e => e.DA).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.DE).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.EN).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.NL).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.PL).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.SV).HasMaxLength(2000).IsRequired();
            entity.Property(e => e.HasEmptyWagonOrder).HasDatabaseDefault("1");
        });
}
