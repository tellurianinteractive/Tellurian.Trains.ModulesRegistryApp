namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Who may add a module for an owner (Migration step 3.6), from the database: for a person, the person, a global administrator, a
/// country administrator in the person's country, or a data administrator of a group the person is a member of; for a group, a
/// global administrator, a country administrator in the group's country, or a data administrator of the group. A read-only user
/// may not. An existing module is edited by those who may edit it (<c>MayEditModuleAsync</c>), whatever owner the address names.
/// </summary>
public static class ModulePermissionExtensions
{
    extension(ModulesDbContext dbContext)
    {
        public async Task<bool> MayAddModuleForAsync(ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef)
        {
            if (!principal.IsAuthenticated() || principal.IsReadOnly()) return false;
            var personId = principal.PersonId();
            if (ownerRef.IsGroup)
            {
                var countryId = await dbContext.Groups.AsNoTracking().Where(g => g.Id == ownerRef.GroupId).Select(g => (int?)g.CountryId).SingleOrDefaultAsync().ConfigureAwait(false);
                if (countryId is null) return false;
                if (principal.IsCountryAdministratorInCountry(countryId)) return true;
                return await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.GroupId == ownerRef.GroupId && gm.PersonId == personId && gm.IsDataAdministrator).ConfigureAwait(false);
            }
            if (ownerRef.PersonId > 0)
            {
                if (ownerRef.PersonId == personId) return true;
                var countryId = await dbContext.People.AsNoTracking().Where(p => p.Id == ownerRef.PersonId).Select(p => (int?)p.CountryId).SingleOrDefaultAsync().ConfigureAwait(false);
                if (countryId is null) return false;
                if (principal.IsCountryAdministratorInCountry(countryId)) return true;
                return await dbContext.GroupMembers.AsNoTracking().AnyAsync(admin => admin.PersonId == personId && admin.IsDataAdministrator &&
                    admin.Group.GroupMembers.Any(member => member.PersonId == ownerRef.PersonId)).ConfigureAwait(false);
            }
            return false;
        }

        /// <summary>
        /// Who may delete a module (Migration step 3.6): its sole owner (a person owning 1/1, or a data administrator of a group owning it
        /// 1/1), a global administrator, or a country administrator in the country of an owner. Not assistants. A read-only user may not.
        /// Before, any country administrator could delete any module, and a group's module only an administrator.
        /// </summary>
        public async Task<bool> MayDeleteModuleAsync(ClaimsPrincipal? principal, int moduleId)
        {
            if (!principal.IsAuthenticated() || principal.IsReadOnly()) return false;
            if (principal.IsGlobalAdministrator()) return true;
            var owners = await dbContext.ModuleOwnerships.AsNoTracking()
                .Where(mo => mo.ModuleId == moduleId && mo.OwnedShare > 0)
                .Select(mo => new { mo.PersonId, mo.GroupId, mo.OwnedShare, CountryId = mo.PersonId.HasValue ? mo.Person.CountryId : mo.Group.CountryId })
                .ToListAsync().ConfigureAwait(false);
            if (principal.IsCountryAdministrator() && owners.Any(o => o.CountryId == principal.CountryId())) return true;
            if (owners.Count != 1 || owners[0].OwnedShare < 1) return false;
            var owner = owners[0];
            var personId = principal.PersonId();
            if (owner.PersonId.HasValue) return owner.PersonId == personId;
            return await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.GroupId == owner.GroupId && gm.PersonId == personId && gm.IsDataAdministrator).ConfigureAwait(false);
        }
    }
}