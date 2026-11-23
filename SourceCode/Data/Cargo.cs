using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Data;

public partial class Cargo
{
    public Cargo()
    {
    }

    public int Id { get; set; }
    public string? DefaultClasses { get; set; }
    public short? FromYear { get; set; }
    public short? UptoYear { get; set; }
    public int NhmCode { get; set; }
    public string? DA { get; set; }
    public string? DE { get; set; }
    public string? EN { get; set; }
    public string? FR { get; set; }
    public string? IT { get; set; }
    public string? NL { get; set; }
    public string? NB { get; set; }
    public string? PL { get; set; }
    public string? SV { get; set; }
    public bool IsExpress { get; set; }
    public bool IsCoolingRequired { get; set; }
}



public static class CargoExtentions
{
    public static string MajorNhmCode(this Cargo? me) =>
        me is null ? string.Empty :
        me.NhmCode == 0 ? "----" :
        $"{me.NhmCode / 10000:0000}";

    public static string MinorNhmCode(this Cargo? me) =>
        me is null ? string.Empty :
        me.NhmCode == 0 ? "–––-" :
        $"{me.NhmCode % 10000:0000}";

    public static string NhmCodeOrEmpty(this Cargo? me) =>
        me is null || me.NhmCode == 0 ? string.Empty :
        $"{me.NhmCode:0000 0000}";
}

internal static class CargoMapper
{
    public static void MapCargo(this ModelBuilder builder) =>
        builder.Entity<Cargo>(entity =>
        {
            entity.ToTable("Cargo");

            entity.Property(e => e.DefaultClasses).HasMaxLength(10);
            entity.Property(e => e.DA).HasMaxLength(50);
            entity.Property(e => e.DE).HasMaxLength(50);
            entity.Property(e => e.EN).HasMaxLength(50);
            entity.Property(e => e.FR).HasMaxLength(50);
            entity.Property(e => e.IT).HasMaxLength(50);
            entity.Property(e => e.NL).HasMaxLength(50);
            entity.Property(e => e.NB).HasMaxLength(50);
            entity.Property(e => e.PL).HasMaxLength(50);
            entity.Property(e => e.SV).HasMaxLength(50);
            entity.Property(e => e.IsExpress).HasDatabaseDefault("0");
            entity.Property(e => e.IsCoolingRequired).HasDatabaseDefault("0");
        });
}

