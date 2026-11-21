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
    }
}