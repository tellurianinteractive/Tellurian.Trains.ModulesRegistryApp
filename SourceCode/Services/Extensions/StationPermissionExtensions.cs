namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Who may change a station's freight customers and their waybills, the same rule as on the station pages: a global
/// administrator, a country administrator in the station's country, an owner of the station's primary module, a data
/// administrator of an owning group, or a data administrator of a group that an owning person is a member of.
/// A read-only user may not. Also for the station itself, its tracks and its deletion (since Migration step 3.6).
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
            return station.PrimaryModuleId is not null && await dbContext.IsOwnerOrDataAdministratorOfModuleAsync(principal, station.PrimaryModuleId.Value).ConfigureAwait(false);
        }

        /// <summary>
        /// Who may add a station to a module (Migration step 3.6): a global administrator, a country administrator in the country
        /// of a person or group that owns the module, an owner, or a data administrator as for <see cref="MayEditStationAsync"/>.
        /// </summary>
        public async Task<bool> MayEditModuleAsync(ClaimsPrincipal? principal, int moduleId)
        {
            if (!principal.IsAuthenticated() || principal.IsReadOnly()) return false;
            if (principal.IsGlobalAdministrator()) return true;
            if (principal.IsCountryAdministrator())
            {
                var countryId = principal.CountryId();
                if (await dbContext.ModuleOwnerships.AsNoTracking().AnyAsync(mo => mo.ModuleId == moduleId && mo.OwnedShare > 0 &&
                    (mo.PersonId.HasValue && mo.Person.CountryId == countryId || mo.GroupId.HasValue && mo.Group.CountryId == countryId)).ConfigureAwait(false)) return true;
            }
            return await dbContext.IsOwnerOrDataAdministratorOfModuleAsync(principal, moduleId).ConfigureAwait(false);
        }

        private async Task<bool> IsOwnerOrDataAdministratorOfModuleAsync(ClaimsPrincipal principal, int moduleId)
        {
            var owners = await dbContext.ModuleOwnerships.AsNoTracking()
                .Where(mo => mo.ModuleId == moduleId) // Also assistants (no share): they help the owners edit everything about the module.
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