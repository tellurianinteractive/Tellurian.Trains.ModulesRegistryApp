using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

#nullable disable
public partial class Country
{
    public Country()
    {
        Groups = new HashSet<Group>();
        People = new HashSet<Person>();
        Regions = new HashSet<Region>();
    }

    public int Id { get; set; }
    public string EnglishName { get; set; }
    public string DomainSuffix { get; set; }
    public string Languages { get; set; }
    public short PhoneNumber { get; set; }
    public bool IsFullySupported { get; set; }

    public virtual ICollection<Group> Groups { get; set; }
    public virtual ICollection<Person> People { get; set; }
    public virtual ICollection<Region> Regions { get; set; }
}

#nullable enable

public static class CountryExtensions
{
    public static string FlagSrc(this Country country) =>
        country is null ? string.Empty :
        $"images/flags/{country.DomainSuffix}.png";
}

internal static class CountryMapper
{
    public static void MapCountry(this ModelBuilder builder) =>
        builder.Entity<Country>(entity =>
        {
            entity.ToTable("Country");

            entity.Property(e => e.DomainSuffix)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength(true);

            entity.Property(e => e.EnglishName)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Languages)
                .HasMaxLength(10);
        });
}
