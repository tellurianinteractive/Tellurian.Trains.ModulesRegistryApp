using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ModulesRegistry.Data.ChangeTracking;

/// <summary>
/// Before a save: properties marked as modified with an unchanged value are reset,
/// and an entry with nothing changed becomes <see cref="EntityState.Unchanged"/>,
/// so a save without changes doesn't reach the database and an update writes only the changed columns.
/// </summary>
/// <remarks>
/// The original values must come from the database: load the entity and change it (or use
/// <c>CurrentValues.SetValues</c>). An entity attached and forced to <see cref="EntityState.Modified"/>
/// has original values equal to its current values, so it is not saved at all.
/// Stateless, so one instance can be shared by all contexts.
/// </remarks>
public sealed class SkipUnchangedValuesInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        SkipUnchangedValues(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        SkipUnchangedValues(eventData.Context);
        return ValueTask.FromResult(result);
    }

    internal static void SkipUnchangedValues(DbContext? context)
    {
        if (context is null) return;
        var changeTracker = context.ChangeTracker;
        if (changeTracker.AutoDetectChangesEnabled) changeTracker.DetectChanges();

        foreach (var entry in changeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList())
        {
            var modifiedProperties = entry.ModifiedProperties();
            if (modifiedProperties.IsModified)
            {
                foreach (var property in entry.Properties.Where(p => p.IsModified && !p.HasChangedValue))
                {
                    property.IsModified = false;
                }
            }
            else
            {
                entry.State = EntityState.Unchanged;
            }
        }
    }
}