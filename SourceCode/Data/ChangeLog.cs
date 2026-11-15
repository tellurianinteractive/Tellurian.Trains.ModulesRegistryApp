using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Data;

/// <summary>
/// One added, changed or deleted entity (Migration step 2C.5c), written by
/// <see cref="ChangeTracking.ChangeLogInterceptor"/>. Only property names, never values.
/// </summary>
public sealed class ChangeLog
{
    public int Id { get; set; }
    public required string EntityType { get; set; }
    public int EntityId { get; set; }
    public ChangeOperation Operation { get; set; }
    /// <summary>
    /// Comma-separated names of the changed properties; null when added or deleted.
    /// </summary>
    public string? ChangedProperties { get; set; }
    /// <summary>
    /// Null when saved without a signed-in user. No foreign key: the log must not stop a user from being deleted.
    /// </summary>
    public int? UserId { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
}

public enum ChangeOperation : byte
{
    Added = 1,
    Modified = 2,
    Deleted = 3,
}

public static class ChangeLogExtensions
{
    public const int EntityTypeMaxLength = 50;
    public const int ChangedPropertiesMaxLength = 1000;

    extension(ModelBuilder builder)
    {
        internal void MapChangeLog() =>
            builder.Entity<ChangeLog>(entity =>
            {
                entity.ToTable("ChangeLog", table =>
                    table.HasCheckConstraint("CK_ChangeLog_Operation", "[Operation] IN (1, 2, 3)"));

                entity.Property(c => c.EntityType).IsRequired().HasMaxLength(EntityTypeMaxLength);
                entity.Property(c => c.Operation).HasConversion<byte>();
                entity.Property(c => c.ChangedProperties).HasMaxLength(ChangedPropertiesMaxLength);

                entity.HasIndex(c => new { c.EntityType, c.EntityId, c.ChangedAt }, "IX_ChangeLog_Entity")
                    .IsDescending(false, false, true);
                entity.HasIndex(c => new { c.UserId, c.ChangedAt }, "IX_ChangeLog_UserId")
                    .IsDescending(false, true);
                entity.HasIndex(c => c.ChangedAt, "IX_ChangeLog_ChangedAt");
            });
    }
}