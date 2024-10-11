using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public class NHM
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public byte LevelDigits { get; set; }
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
        modelBuilder.Entity<NHM>(entity => entity.ToTable("NHM"));
}
