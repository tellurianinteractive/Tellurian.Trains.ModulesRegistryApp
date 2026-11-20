namespace ModulesRegistry.Services.Implementations;

public sealed class ModuleStandardService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<IEnumerable<ListboxItem>> GetStandardsListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.ModuleStandards
                .Include(ms => ms.Scale)
                .ToReadOnlyListAsync();
            return items
                .Select(ms => new ListboxItem(ms.Id, $"{ms.ShortName} (1:{ms.Scale.Denominator})"))
                .OrderBy(l => l.Description);
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<ListboxItem>> GetStandarsdWithScaleListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.ModuleStandards
                .Include(ms => ms.Scale)
                .ToReadOnlyListAsync();
            return items
                .Select(ms => new ListboxItem(ms.Id, $"{ms.ShortName} (1:{ms.Scale.Denominator})"))
                .OrderBy(l => l.Description);
        }
        return Array.Empty<ListboxItem>();
    }

    public async Task<IEnumerable<ModuleStandard>> All(ClaimsPrincipal? principal)
    {
        if (principal.MayRead())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleStandards
                .Include(ms => ms.Scale)
                .OrderBy(ms => ms.ShortName)
                .ToReadOnlyListAsync();
        }
        return Array.Empty<ModuleStandard>();
    }

    public async Task<ModuleStandard?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.MayRead())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleStandards
                .Where(ms => ms.Id == id)
                .Include(ms => ms.Scale)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }
        return null;
    }

    /// <summary>
    /// Adds or updates a module standard. Module standards are shared by all countries, so only global administrators save them
    /// (Migration step 3.6; before, any user who wasn't read-only could, and country administrators could open the form).
    /// </summary>
    public async Task<(int Count, string Message, ModuleStandard? Entity)> SaveAsync(ClaimsPrincipal? principal, ModuleStandard entity)
    {
        if (!principal.IsGlobalAdministrator() || principal.IsReadOnly()) return principal.SaveNotAuthorised<ModuleStandard>();
        entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
        using var dbContext = Factory.CreateDbContext();
        if (entity.Id > 0)
        {
            var existing = await dbContext.ModuleStandards.FindAsync(entity.Id).ConfigureAwait(false);
            if (existing is null) return principal.NothingToUpdate<ModuleStandard>();
            dbContext.Entry(existing).CurrentValues.SetValues(entity);
            if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(existing);
        }
        entity.Scale = null!; // Only the id is saved.
        dbContext.ModuleStandards.Add(entity);
        return (await dbContext.SaveChangesAsync().ConfigureAwait(false)).SaveResult(entity);
    }

    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.MayDelete())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = dbContext.ModuleStandards.Find(id);
            if (existing is null) return existing.NotFound();
            dbContext.ModuleStandards.Remove(existing);
            var count = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return count.DeleteResult();
        }
        return principal.NotAuthorized<ModuleStandard>();
    }
}
