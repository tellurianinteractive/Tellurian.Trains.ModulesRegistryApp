using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace ModulesRegistry.Services.Implementations;

public class WiFredThrottleService(IDbContextFactory<ModulesDbContext> factory, ITimeProvider timeProvider, ILogger<WiFredThrottleService> logger)
{
    private IDbContextFactory<ModulesDbContext> Factory { get; } = factory;
    public ITimeProvider TimeProvider { get; } = timeProvider;

    private static readonly ErrorCase[] SaveErrorCases =
    [
        new("UX_WIFredThrottle_InventoryNumber", "Duplicated", "InventoryNumber"),
        new("UX_WIFredThrottle_MacAddress", "Duplicated", "MacAddress")
    ];


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
        if (onlyMyThrottles) return await GetOwnersThrottles(principal, principal.PersonId());
        if (principal.MayManageWiFreds() || principal.IsGlobalOrCountryAdministrator()) return await GetAllThrottles(principal);
        return await GetOwnersThrottles(principal, principal.PersonId());
    }

    public async Task<IEnumerable<WiFredThrottle>> GetOwnersThrottles(ClaimsPrincipal? principal, int owningPersonId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.WiFredThrottles
                .Where(w => w.OwningPersonId == owningPersonId && !w.DeletedDateTime.HasValue)
                .Include(t => t.OwningPerson).ThenInclude(p => p.Country)
                .ToReadOnlyListAsync();
        }
        return Enumerable.Empty<WiFredThrottle>();
    }

    public async Task<IEnumerable<WiFredThrottle>> GetAllThrottles(ClaimsPrincipal? principal)
    {
        if (principal.MayManageWiFreds() || principal.IsGlobalOrCountryAdministrator())
        {
            using var dbContext = Factory.CreateDbContext();
            return await dbContext.WiFredThrottles
                .Include(t => t.OwningPerson).ThenInclude(p => p.Country)
                .ToReadOnlyListAsync();
        }
        return Enumerable.Empty<WiFredThrottle>();
    }

    /// <summary>
    /// Saves a throttle (Migration step 3.6): a throttle manager any throttle; others only their own, to themselves. An edited throttle
    /// is found by its id; a new one by its MAC address, which may re-register a deleted throttle, but not take over an active one.
    /// </summary>
    public async Task<(int Count, string Message, WiFredThrottle? Entity)> SaveAsync(ClaimsPrincipal? principal, WiFredThrottle entity)
    {
        if (principal.IsAuthenticated() && !principal.IsReadOnly())
        {
            var mayManage = principal.MayManageWiFreds();
            if (!mayManage && entity.OwningPersonId != principal.PersonId()) return principal.SaveNotAuthorised<WiFredThrottle>();
            entity.CorrectInput(); // Corrected, not validated (EntityCorrectionExtensions, Migration step 3.5).
            using var dbContext = Factory.CreateDbContext();
            entity.SetDccAddressOrNull();
            entity.SetMacAddressUppercase();
            entity.UpdatedDateTime = TimeProvider.Now;

            var existing = entity.Id > 0 ?
                await dbContext.WiFredThrottles.SingleOrDefaultAsync(w => w.Id == entity.Id).ConfigureAwait(false) :
                await dbContext.WiFredThrottles.SingleOrDefaultAsync(w => w.MacAddress == entity.MacAddress).ConfigureAwait(false);
            if (entity.Id > 0 && existing is null) return principal.NothingToUpdate<WiFredThrottle>();
            if (entity.Id > 0 && !mayManage && existing!.OwningPersonId != principal.PersonId()) return principal.SaveNotAuthorised<WiFredThrottle>();
            if (existing is null)
            {
                entity.RegistrationDateTime = TimeProvider.Now;
                dbContext.WiFredThrottles.Add(entity);
            }
            else if (entity.Id == 0 && !existing.DeletedDateTime.HasValue) // An active throttle has this MAC address.
            {
                return DbContextExtensions.SaveResult<WiFredThrottle>($"{"Error".Localized()}: {"Duplicated".Localized()} '{"MacAddress".Localized()}'");
            }
            else if (entity.Id == 0) // re-registration of a deleted throttle
            {
                existing.OwningPersonId = entity.OwningPersonId;
                existing.RegistrationDateTime = TimeProvider.Now;
                existing.UpdatedDateTime = TimeProvider.Now;
                existing.DeletedDateTime = null;
                existing.SetDccAddressOrNull(entity);
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
                return result.SaveResult(existing ?? entity);

            }
            catch (Exception ex)
            {
                // A duplicate is an expected input error shown to the user; anything else is logged.
                if (!ex.IsAnyOf(SaveErrorCases)) logger.LogError(ex, "Saving WiFRED throttle {ThrottleId} failed", entity.Id);
                return DbContextExtensions.SaveResult<WiFredThrottle>(ex.ErrorMessage(SaveErrorCases));
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

    public async Task<(int Count, string Message)> DeleteAsync(ClaimsPrincipal? principal, int throttleId)
    {
        if (principal.IsAuthenticated())
        {
            using var dbContext = Factory.CreateDbContext();
            var existing = await FindById(principal, throttleId);
            if (existing is null) return principal.NotFound();
            dbContext.WiFredThrottles.Attach(existing);

            if (existing.IsMacAddressLocked())
            {
                existing.DeletedDateTime = TimeProvider.Now;
            }
            else
            {
                dbContext.WiFredThrottles.Remove(existing);
            }
            var result = await dbContext.SaveChangesAsync();
            return result.DeleteResult();
        }
        return principal.NotAuthorized<WiFredThrottle>();
    }

}
