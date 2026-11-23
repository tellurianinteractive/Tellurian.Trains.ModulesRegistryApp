#nullable disable

using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

public partial class Property
{
    public Property()
    {
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Value { get; set; }
}

#nullable enable
public static class PropertyExtensions
{
    extension(ModelBuilder builder)
    {
        internal void MapProperty() =>
            builder.Entity<Property>(entity =>
            {
                entity.ToTable("Property");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(50);
            });
    }
}