using Microsoft.EntityFrameworkCore;

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

            entity.HasOne<NHM>()
                .WithMany()
                .HasForeignKey(e => e.NhmCode);
        });
}
