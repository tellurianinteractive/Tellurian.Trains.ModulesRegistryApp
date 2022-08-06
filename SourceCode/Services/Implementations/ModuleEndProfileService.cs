namespace ModulesRegistry.Services.Implementations;

public class ModuleEndProfileService
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    public ModuleEndProfileService(IDbContextFactory<ModulesDbContext> factory)
    {
        Factory = factory;
    }

    public async Task<IEnumerable<ListboxItem>> ListboxItemsAsync(int? scaleId)
    {
        var dbContext = Factory.CreateDbContext();
        return await dbContext.ModuleEndProfiles.AsNoTracking()
            .Where(mgt => !scaleId.HasValue || mgt.ScaleId == scaleId)
            .Select(mgt => new ListboxItem(mgt.Id, mgt.Designation))
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IEnumerable<ModuleEndProfile>> GetAllAsync()
    {
        var dbContext = Factory.CreateDbContext();
        return await dbContext.ModuleEndProfiles.AsNoTracking()
            .Include(mgt => mgt.Scale)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<ModuleEndProfile?> FindByIdAsync(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAnyAdministrator())
        {
            var dbContext = Factory.CreateDbContext();
            return await dbContext.ModuleEndProfiles.FindAsync(id).ConfigureAwait(false);
        }
        return null;
    }

    public async Task<(int Count, string Message, ModuleEndProfile? Entity)> SaveAsync(ClaimsPrincipal? principal, ModuleEndProfile entity)
    {
        if (principal.IsAnyAdministrator())
        {
            var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.ModuleEndProfiles.FindAsync(entity.Id).ConfigureAwait(false);
            if (existing is null)
            {
                dbContext.ModuleEndProfiles.Add(entity);
            }
            else
            {
                dbContext.Entry(existing).CurrentValues.SetValues(entity);
                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(existing);
            }
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.SaveResult(existing ?? entity);
        }
        return principal.SaveNotAuthorised<ModuleEndProfile>();
    }
}
