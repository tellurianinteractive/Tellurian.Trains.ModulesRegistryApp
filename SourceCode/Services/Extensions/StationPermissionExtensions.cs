namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Who may change a station's freight customers and their waybills, the same rule as on the station pages: a global
/// administrator, a country administrator in the station's country, an owner of the station's primary module, a data
/// administrator of an owning group, or a data administrator of a group that an owning person is a member of.
/// A read-only user may not.
/// </summary>
/// <remarks>
/// Before release 1.15.10 these writes only required a signed-in user, so anyone could change another member's freight
/// customers and waybills by opening the edit address directly.
/// </remarks>
public static class StationPermissionExtensions
{
    extension(ModulesDbContext dbContext)
    {
        public async Task<bool> MayEditStationAsync(ClaimsPrincipal? principal, int stationId)
        {
            if (!principal.IsAuthenticated() || principal.IsReadOnly()) return false;
            var station = await dbContext.Stations.AsNoTracking()
                .Where(s => s.Id == stationId)
                .Select(s => new { s.PrimaryModuleId, CountryId = (int?)s.Region.CountryId })
                .SingleOrDefaultAsync().ConfigureAwait(false);
            if (station is null) return false;
            if (principal.IsGlobalAdministrator() || principal.IsCountryAdministratorInCountry(station.CountryId)) return true;
            if (station.PrimaryModuleId is null) return false;

            var owners = await dbContext.ModuleOwnerships.AsNoTracking()
                .Where(mo => mo.ModuleId == station.PrimaryModuleId)
                .Select(mo => new { mo.PersonId, mo.GroupId })
                .ToListAsync().ConfigureAwait(false);
            var personId = principal.PersonId();
            if (personId > 0 && owners.Any(o => o.PersonId == personId)) return true;

            List<int> ownerGroupIds = [.. owners.Where(o => o.GroupId.HasValue).Select(o => o.GroupId!.Value)];
            List<int> ownerPersonIds = [.. owners.Where(o => o.PersonId.HasValue).Select(o => o.PersonId!.Value)];
            if (personId < 1 || ownerGroupIds.Count + ownerPersonIds.Count == 0) return false;
            return await dbContext.GroupMembers.AsNoTracking()
                .AnyAsync(admin => admin.PersonId == personId && admin.IsDataAdministrator &&
                    (ownerGroupIds.Contains(admin.GroupId) || admin.Group.GroupMembers.Any(member => ownerPersonIds.Contains(member.PersonId))))
                .ConfigureAwait(false);
        }

        public async Task<bool> MayEditStationOfCustomerAsync(ClaimsPrincipal? principal, int stationCustomerId)
        {
            var stationId = await dbContext.StationCustomers.AsNoTracking()
                .Where(sc => sc.Id == stationCustomerId)
                .Select(sc => sc.StationId)
                .SingleOrDefaultAsync().ConfigureAwait(false);
            return stationId > 0 && await dbContext.MayEditStationAsync(principal, stationId).ConfigureAwait(false);
        }
    }
}