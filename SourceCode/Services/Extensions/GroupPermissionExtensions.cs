namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Who administers a group's members, and so its members' layout modules: a global administrator, a country administrator in
/// the group's country, or one of the group's administrators. A read-only user may not.
/// </summary>
/// <remarks>
/// Before release 1.15.25 adding a member only required a signed-in user (the old extension's country check was always true), and
/// adding or removing a group's layout modules too, so anyone could change another group by opening the address directly.
/// </remarks>
public static class GroupPermissionExtensions
{
    extension(ModulesDbContext dbContext)
    {
        public async Task<bool> MayAdministerGroupMembersAsync(ClaimsPrincipal? principal, int groupId, int? countryId = null)
        {
            if (principal is null || principal.IsReadOnly()) return false;
            countryId ??= await dbContext.Groups.AsNoTracking().Where(g => g.Id == groupId).Select(g => (int?)g.CountryId).SingleOrDefaultAsync().ConfigureAwait(false);
            if (countryId is null) return false;
            if (principal.IsCountryAdministratorInCountry(countryId)) return true;
            var personId = principal.PersonId();
            return personId > 0 && await dbContext.GroupMembers.AsNoTracking()
                .AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == personId && gm.IsGroupAdministrator).ConfigureAwait(false);
        }
    }
}