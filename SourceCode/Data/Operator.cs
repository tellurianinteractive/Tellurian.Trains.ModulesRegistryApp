#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class Operator
{
    public int Id { get; set; }
    public string Signature { get; set; }
    public string FullName { get; set; }
    public int PrimaryOperatingCountryId { get; set; }
    public short? FirstYearInOperation { get; set; }
    public short? FinalYearInOperation { get; set; }
    public bool IsPassengerOperator { get; set; }
    public bool IsFreightOperator { get; set; }
    public bool IsConstructionOperator { get; set; }
    public bool IsVeteranOperator { get; set; }
    public bool IsAuthority { get; set; }

    public virtual Country PrimaryOperatingCountry { get; set; }
}

public static class OperatorMapper
{
    internal static void MapOperator(this ModelBuilder modelBuilder) =>
         modelBuilder.Entity<Operator>(entity =>
         {
             entity.ToTable("Operator");

             entity.Property(e => e.FullName)
                 .IsRequired()
                 .HasMaxLength(50);

             entity.Property(e => e.Signature)
                 .IsRequired()
                 .HasMaxLength(6);

             entity.HasOne(c => c.PrimaryOperatingCountry)
                 .WithMany()
                 .HasForeignKey(e => e.PrimaryOperatingCountryId);
         });
}
