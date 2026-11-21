namespace ModulesRegistry.Services.Extensions;

/// <summary>
/// Who edits a person, their country and their user's rights (Migration step 3.6, release 1.15.28). The person form posts every
/// field, so the service decides; the page only hides or disables what the user may not change.
/// </summary>
public static class PersonPermissionExtensions
{
    extension(ModulesDbContext dbContext)
    {
        /// <summary>
        /// A global administrator, a country administrator in the person's country, the person themselves, or, with
        /// <paramref name="groupId"/>, one who administers the group's members when the person is new or a member of the group.
        /// Never a read-only user. For a stored person, pass the stored values, not the posted ones.
        /// </summary>
        public async Task<bool> MayEditPersonAsync(ClaimsPrincipal? principal, Person person, int groupId = 0)
        {
            if (principal is null || principal.IsReadOnly() || !principal.IsAuthenticated()) return false;
            if (principal.IsCountryAdministratorInCountry(person.CountryId)) return true;
            if (person.Id > 0 && person.Id == principal.PersonId()) return true;
            if (groupId < 1 || !await dbContext.MayAdministerGroupMembersAsync(principal, groupId).ConfigureAwait(false)) return false;
            return person.Id == 0 || await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == person.Id).ConfigureAwait(false);
        }

        /// <summary>
        /// Who invites a person to become a user (Migration step 3.6): a global administrator, a country administrator in the person's
        /// country, or one who administers the members of a group the person is a member of, as the persons and group members pages show
        /// Invite. Never a read-only user. Before, any signed-in user could invite anyone.
        /// </summary>
        public async Task<bool> MayInvitePersonAsync(ClaimsPrincipal? principal, Person person)
        {
            if (principal is null || principal.IsReadOnly() || !principal.IsAuthenticated()) return false;
            if (principal.IsCountryAdministratorInCountry(person.CountryId)) return true;
            var groupIds = await dbContext.GroupMembers.AsNoTracking().Where(gm => gm.PersonId == person.Id).Select(gm => gm.GroupId).ToListAsync().ConfigureAwait(false);
            foreach (var groupId in groupIds)
                if (await dbContext.MayAdministerGroupMembersAsync(principal, groupId).ConfigureAwait(false)) return true;
            return false;
        }
    }

    extension(ClaimsPrincipal? principal)
    {
        /// <summary>
        /// A global administrator sets any country, a country administrator their own, and a group's member administrator
        /// (<paramref name="viaGroup"/>) any, since members may come from other countries; otherwise the stored country stays.
        /// </summary>
        public bool MaySetPersonCountry(int countryId, int? storedCountryId, bool viaGroup = false) =>
            principal.IsCountryAdministratorInCountry(countryId) || viaGroup || countryId == storedCountryId;

        /// <summary>
        /// Only a global administrator removes their own global administrator right; nobody grants it here.
        /// </summary>
        public bool MayChangeGlobalAdministrator(User user) =>
            principal.IsGlobalAdministrator() && principal.UserId() == user.Id;

        /// <summary>Only a global administrator makes a user a country administrator or removes the right.</summary>
        public bool MayChangeCountryAdministrator => principal.IsGlobalAdministrator();

        /// <summary>
        /// The other rights (API access, uploading drawings, managing wiFREDs) and the area of responsibility: a global administrator,
        /// or a country administrator in the person's country.
        /// </summary>
        public bool MayChangeUserRights(int personCountryId) => principal.IsCountryAdministratorInCountry(personCountryId);

        /// <summary>
        /// Only a country administrator in the person's country deletes a person (decided 2026-11-21); being a global administrator
        /// isn't enough. Never a read-only user.
        /// </summary>
        public bool MayDeletePerson(int personCountryId) =>
            principal is not null && !principal.IsReadOnly() && principal.IsCountryAdministrator() && principal.CountryId() == personCountryId;
    }

    extension(User user)
    {
        /// <summary>
        /// Copies only what the person form changes on a user: the rights, the area of responsibility and the sign-in address. The
        /// rest (password, sign-in times and attempts, read-only and demo flags) is never taken from a posted or stale user.
        /// </summary>
        public void SetRightsAndSignInAddressFrom(User source)
        {
            user.IsGlobalAdministrator = source.IsGlobalAdministrator;
            user.IsCountryAdministrator = source.IsCountryAdministrator;
            user.IsApiAccessPermitted = source.IsApiAccessPermitted;
            user.MayUploadSkpDrawing = source.MayUploadSkpDrawing;
            user.MayManageWiFreds = source.MayManageWiFreds;
            user.AdministratorAreaOfResposibility = source.AdministratorAreaOfResposibility;
            if (source.EmailAddress.HasValue) user.EmailAddress = source.EmailAddress;
        }

        /// <summary>Sets back the stored value of each right <paramref name="principal"/> may not change.</summary>
        public void KeepRightsNotChangeableBy(ClaimsPrincipal? principal, User stored, int personCountryId)
        {
            if (!principal.MayChangeGlobalAdministrator(stored)) user.IsGlobalAdministrator = stored.IsGlobalAdministrator;
            if (!principal.MayChangeCountryAdministrator) user.IsCountryAdministrator = stored.IsCountryAdministrator;
            if (!principal.MayChangeUserRights(personCountryId))
            {
                user.IsApiAccessPermitted = stored.IsApiAccessPermitted;
                user.MayUploadSkpDrawing = stored.MayUploadSkpDrawing;
                user.MayManageWiFreds = stored.MayManageWiFreds;
                user.AdministratorAreaOfResposibility = stored.AdministratorAreaOfResposibility;
            }
        }
    }
}