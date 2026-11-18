using Rationals;

namespace ModulesRegistry.Services.Implementations;
public class ModuleOwnershipService(IDbContextFactory<ModulesDbContext> factory)
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;

    public async Task<(int Count, string Message, Module? Entity)> TransferOwnershipAsync(ClaimsPrincipal? principal, ModuleOwnership origin, ModuleOwnership newOwnership)
    {
        var share = newOwnership.OwnedShare();
        if (principal.IsAuthenticated() && share > Rational.Zero)
        {
            var transfer = new ModuleOwnershipTransfer(origin.AsModuleOwnershipRef(), newOwnership.AsModuleOwnershipRef(), newOwnership.OwnedShare);
            if (transfer.IsZero) return principal.NothingToUpdate<Module>();

            var (From, To) = origin.Transfer(transfer);
            using var dbContext = Factory.CreateDbContext();
            var existingOrigin = await dbContext.ModuleOwnerships.FindAsync(origin.Id);
            if (existingOrigin is null) return principal.NothingToUpdate<Module>();
            if (From.OwnedShare < 0.01)
            {
                dbContext.ModuleOwnerships.Remove(existingOrigin);
            }
            else
            {
                dbContext.ModuleOwnerships.Entry(existingOrigin).CurrentValues.SetValues(From);
            }
            var existingNewOwnership = await dbContext.ModuleOwnerships
                 .SingleOrDefaultAsync(mo => mo.ModuleId == To.ModuleId && (To.PersonId.HasValue && mo.PersonId == To.PersonId.Value || To.GroupId.HasValue && mo.GroupId == To.GroupId.Value));
            if (existingNewOwnership is null)
            {
                if (To.OwnedShare > 0) dbContext.ModuleOwnerships.Add(To);
            }
            else
            {
                existingNewOwnership.OwnedShare += To.OwnedShare;
            }
            var result = await dbContext.SaveChangesAsync();

            var module = await GetModule(origin.ModuleId);
            return result.SaveResult(module);
        }
        return principal.SaveNotAuthorised<Module>();
    }
    public async Task<(int Count, string Message, Module? Entity)> AddAssistantAsync(ClaimsPrincipal? principal, ModuleOwnership ownership)
    {
        if (principal.IsAuthenticated() && ownership.IsAssistantOnly())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = dbContext.ModuleOwnerships
                .AsNoTracking()
                .SingleOrDefault(mo => mo.ModuleId == ownership.ModuleId && (ownership.PersonId.HasValue && mo.PersonId == ownership.PersonId || ownership.GroupId.HasValue && mo.GroupId == ownership.GroupId));
            if (existing is not null) return principal.SaveNotAuthorised<Module>();

            var result = await SaveAssistant(dbContext, ownership);
            var module = await GetModule(ownership.ModuleId);
            return result.SaveResult(module);
        }
        return principal.SaveNotAuthorised<Module>();
    }

    private async Task<Module?> GetModule(int moduleId)
    {
        using var dbContext = Factory.CreateDbContext();
        return await dbContext.Modules
           .AsNoTracking()
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Person)
           .Include(m => m.ModuleOwnerships).ThenInclude(mo => mo.Group)
           .SingleOrDefaultAsync(m => m.Id == moduleId)
           .ConfigureAwait(false);
    }

    /// <summary>
    /// A new entity with only the ids, so that navigations on <paramref name="ownership"/> aren't added too.
    /// (Was raw SQL, from before <c>BlankTriggerAddingConvention</c> made inserts into tables with triggers work.)
    /// </summary>
    private static async Task<int> SaveAssistant(ModulesDbContext dbContext, ModuleOwnership ownership)
    {
        dbContext.ModuleOwnerships.Add(new ModuleOwnership
        {
            PersonId = ownership.PersonId,
            GroupId = ownership.GroupId,
            ModuleId = ownership.ModuleId,
            OwnedShare = 0,
        });
        return await dbContext.SaveChangesAsync();
    }

    public async Task<(int Count, string Message)> RemoveAssistantAsync(ClaimsPrincipal? principal, ModuleOwnership ownership)
    {
        if (principal.IsAuthenticated() && ownership.IsAssistantOnly())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await dbContext.ModuleOwnerships.SingleOrDefaultAsync(mo => mo.Id == ownership.Id);
            if (existing is null) return principal.NotFound();
            dbContext.ModuleOwnerships.Remove(existing);
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.NotAuthorized<ModuleOwnership>();
    }
}
