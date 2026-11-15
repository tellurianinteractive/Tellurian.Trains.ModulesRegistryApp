using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ModulesRegistry.Data.ChangeTracking;

/// <summary>
/// What a save really changes: EF marks a property as modified when it is set (or when the whole entity
/// is forced to <see cref="EntityState.Modified"/>), also when the new value equals the original.
/// These extensions compare the values instead.
/// </summary>
public static class EntityEntryExtensions
{
    extension(EntityEntry entry)
    {
        /// <summary>
        /// The properties marked as modified whose current value differs from the original value.
        /// Empty unless the entry is <see cref="EntityState.Modified"/>.
        /// </summary>
        public IReadOnlyList<PropertyEntry> ModifiedProperties() =>
            entry.State == EntityState.Modified ?
            [.. entry.Properties.Where(p => p.IsModified && p.HasChangedValue)] :
            [];
    }

    extension(IReadOnlyCollection<PropertyEntry> properties)
    {
        /// <summary>
        /// True when at least one property has a changed value.
        /// </summary>
        public bool IsModified => properties.Count > 0;
    }

    extension(PropertyEntry property)
    {
        /// <summary>
        /// True when the current value differs from the original value, compared as EF compares them.
        /// </summary>
        public bool HasChangedValue =>
            !property.Metadata.GetValueComparer().Equals(property.CurrentValue, property.OriginalValue);
    }
}