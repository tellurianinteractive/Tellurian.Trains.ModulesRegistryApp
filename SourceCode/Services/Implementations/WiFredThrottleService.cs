namespace ModulesRegistry.Services.Implementations;

public class WiFredThrottleService
{
    public WiFredThrottleService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider)
    {
        Factory = factory;
        TimeProvider = timeProvider;
    }

    private IDbContextFactory<ModulesDbContext> Factory { get; }
    public ITimeProvider TimeProvider { get; }

    
    public async Task<WiFredThrottle?> FindById(ClaimsPrincipal? principal, int id)
    {
        if (principal.IsAuthenticated())
        {
            bool mayManageWifreds = principal.MayManageWiFreds();
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.WiFredThrottles.AsNoTracking()
                .Where(w => w.Id == id && (mayManageWifreds || w.OwningPersonId == principal.PersonId()))
                .Include(w => w.OwningPerson).ThenInclude(p => p.Country)
                .SingleOrDefaultAsync()
                .ConfigureAwait(false);
        }
        return null;
    }

    public async Task<IEnumerable<WiFredThrottle>> GetThrottles(ClaimsPrincipal? principal, bool onlyMyThrottles = false)
    {
        if (!onlyMyThrottles || principal.MayManageWiFreds() || principal.IsCountryOrGlobalAdministrator()) return await GetAllThrottles(principal);
        return await GetOwnersThrottles(principal, principal.PersonId());
    }

    public async Task<IEnumerable<WiFredThrottle>> GetOwnersThrottles(ClaimsPrincipal? principal, int owningPersonId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.WiFredThrottles
                .Include(t => t.OwningPerson).ThenInclude(p => p.Country)
                .Where(w => w.OwningPersonId == owningPersonId).ToReadOnlyListAsync();
        }
        return Enumerable.Empty<WiFredThrottle>();
    }

    public async Task<IEnumerable<WiFredThrottle>> GetAllThrottles(ClaimsPrincipal? principal)
    {
        if (principal.MayManageWiFreds() || principal.IsCountryOrGlobalAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.WiFredThrottles
                .Include(t => t.OwningPerson).ThenInclude(p => p.Country)
                .ToReadOnlyListAsync();
        }
        return Enumerable.Empty<WiFredThrottle>();
    }

    public async Task<(int Count, string Message, WiFredThrottle? Entity)> SaveAsync(ClaimsPrincipal? principal, WiFredThrottle entity)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            entity.SetDccAddressOrNull();
            entity.SetMacAddressUppercase();

            var existing = await dbContext.WiFredThrottles.SingleOrDefaultAsync(w => w.Id == entity.Id).ConfigureAwait(false);
            if (existing is null)
            {
                entity.RegistrationDateTime = TimeProvider.Now;
                dbContext.WiFredThrottles.Add(entity);
            }
            else
            {
                if (entity.IsMacAddressLocked()) entity.MacAddress = existing.MacAddress;
                dbContext.Entry(existing).CurrentValues.SetValues(entity);

                if (dbContext.Entry(existing).State == EntityState.Unchanged) return (-1).SaveResult(entity);
            }
            try
            {
                var result = await dbContext
                    .SaveChangesAsync()
                    .ConfigureAwait(false);
                return result.SaveResult(entity);

            }
            catch (Exception ex)
            {
                return DbContextExtensions.SaveResult<WiFredThrottle>(ex.Message);
            }
        }
        return principal.SaveNotAuthorised<WiFredThrottle>();
    }

    public async Task<(int Count, string Message)> SetVerified(ClaimsPrincipal? principal, int throttleId)
    {
        if (principal.MayManageWiFreds())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await FindById(principal, throttleId);
            if (existing is null) return principal.NotFound();
            dbContext.WiFredThrottles.Attach(existing);
            existing.ValidationDateTime = TimeProvider.Now;
            var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);
            return result.UpdateResult();
        }
        return principal.NotAuthorized<WiFredThrottle>();
    }
}
