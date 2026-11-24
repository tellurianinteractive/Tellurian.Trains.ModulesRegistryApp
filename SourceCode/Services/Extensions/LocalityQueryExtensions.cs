namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// The localities a user may see, and those a module may be stored at. A venue and a locality without a group are seen by everyone;
/// a storage place used by a group only by the group's members, global administrators and the country administrators of its country.
/// </summary>
public static class LocalityQueryExtensions
{
    extension(ModulesDbContext dbContext)
    {
        public IQueryable<Locality> VisibleLocalities(ClaimsPrincipal principal)
        {
            var isGlobalAdministrator = principal.IsGlobalAdministrator();
            var administratedCountryId = principal.IsCountryAdministrator() ? principal.CountryId() : -1;
            var personId = principal.PersonId();
            return dbContext.Localities.Where(l =>
                isGlobalAdministrator || l.IsMeetingVenue || l.UsedByGroupId == null || l.CountryId == administratedCountryId ||
                dbContext.GroupMembers.Any(gm => gm.GroupId == l.UsedByGroupId && gm.PersonId == personId));
        }

        /// <summary>
        /// Whether the user may edit the locality: a global administrator, a country administrator in its country, or a data
        /// administrator of the group using it. Never a read-only user.
        /// </summary>
        public async Task<bool> MayEditLocalityAsync(ClaimsPrincipal? principal, Locality locality)
        {
            if (!principal.IsAuthenticated() || principal.IsReadOnly()) return false;
            if (principal.IsCountryAdministratorInCountry(locality.CountryId)) return true;
            var personId = principal.PersonId();
            return locality.UsedByGroupId is int groupId &&
                await dbContext.GroupMembers.AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == personId && gm.IsDataAdministrator);
        }

        /// <summary>
        /// The storage places a module may be stored at: localities marked as storage, in the country of the module's owners, that
        /// the user may see.
        /// </summary>
        public IQueryable<Locality> StorageLocalitiesFor(ClaimsPrincipal principal, int[] ownerCountryIds) =>
            dbContext.VisibleLocalities(principal).Where(l => l.IsModuleStorage && ownerCountryIds.Contains(l.CountryId));

        /// <summary>
        /// The countries of a module's owners (persons or groups owning a share), or for a new module (<paramref name="moduleId"/> 0)
        /// the country of the owner it is added for.
        /// </summary>
        public async Task<int[]> ModuleOwnerCountryIdsAsync(int moduleId, ModuleOwnershipRef ownerRef)
        {
            if (moduleId > 0)
                return await dbContext.ModuleOwnerships
                    .Where(mo => mo.ModuleId == moduleId && mo.OwnedShare > 0)
                    .Select(mo => mo.PersonId != null ? mo.Person.CountryId : mo.Group.CountryId)
                    .Distinct()
                    .ToArrayAsync();
            if (ownerRef.IsGroup)
                return await dbContext.Groups.Where(g => g.Id == ownerRef.GroupId).Select(g => g.CountryId).ToArrayAsync();
            return await dbContext.People.Where(p => p.Id == ownerRef.PersonId).Select(p => p.CountryId).ToArrayAsync();
        }
    }
}