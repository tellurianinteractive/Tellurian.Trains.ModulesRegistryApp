namespace ModulesRegistry.Services.Implementations;

public sealed class ModuleStandardService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public ModuleStandardService(IDbContextFactory<ModulesDbContext> factory) => Factory = factory;

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(ClaimsPrincipal? principal)
    {
        if (principal is not null)
        {
            using var dbContext = Factory.CreateDbContext();
            var items = await dbContext.ModuleStandards
                .Include(ms => ms.Scale)
                .ToListAsync()
                .ConfigureAwait(false);
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
                .ToListAsync()
                .ConfigureAwait(false);
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

    public async Task<(int Count, string Message, ModuleStandard? Entity)> SaveAsync(ClaimsPrincipal? principal, ModuleStandard entity)
    {
        if (principal.MaySave())
        {
            if (entity.Scale is not null && entity.ScaleId != entity.Scale.Id) entity.Scale = null;
            using var dbContext = Factory.CreateDbContext();
            dbContext.ModuleStandards.Attach(entity);
            dbContext.Entry(entity).State = entity.Id.GetState();
            var count = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return count.SaveResult(entity);
        }
        return principal.SaveNotAuthorised<ModuleStandard>();
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
        return principal.DeleteNotAuthorized<ModuleStandard>();
    }
}
