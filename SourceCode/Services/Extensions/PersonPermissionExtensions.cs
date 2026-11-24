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
        /// <paramref name="groupId"/>, one who administers the group's members when the person is a member of the group. Only global
        /// and country administrators add persons (2026-11-24). Never a read-only user. For a stored person, pass the stored values,
        /// not the posted ones.
        /// </summary>
        public async Task<bool> MayEditPersonAsync(ClaimsPrincipal? principal, Person person, int groupId = 0)
        {
            if (principal is null || principal.IsReadOnly() || !principal.IsAuthenticated()) return false;
            if (principal.IsCountryAdministratorInCountry(person.CountryId)) return true;
            if (person.Id > 0 && person.Id == principal.PersonId()) return true;
            if (person.Id < 1 || groupId < 1 || !await dbContext.MayAdministerGroupMembersAsync(principal, groupId).ConfigureAwait(false)) return false;
            return await dbContext.GroupMembers.AsNoTracking().AnyAsync(gm => gm.GroupId == groupId && gm.PersonId == person.Id).ConfigureAwait(false);
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
        /// Only a global administrator changes the country of a stored person (<paramref name="storedCountryId"/>); otherwise it stays.
        /// A new person gets any country from a global administrator and the own country from a country administrator.
        /// </summary>
        public bool MaySetPersonCountry(int countryId, int? storedCountryId) =>
            storedCountryId.HasValue ?
                countryId == storedCountryId || principal.IsGlobalAdministrator() :
                principal.IsCountryAdministratorInCountry(countryId);

        /// <summary>Only the super user grants or removes the global administrator right.</summary>
        public bool MayChangeGlobalAdministrator => principal.IsSuperUser();

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

        /// <summary>
        /// Sets back the stored value of each right <paramref name="principal"/> may not change, and the stored area of responsibility
        /// when the user is neither a global nor a country administrator.
        /// </summary>
        public void KeepRightsNotChangeableBy(ClaimsPrincipal? principal, User stored, int personCountryId)
        {
            if (!principal.MayChangeGlobalAdministrator) user.IsGlobalAdministrator = stored.IsGlobalAdministrator;
            if (!principal.MayChangeCountryAdministrator) user.IsCountryAdministrator = stored.IsCountryAdministrator;
            if (!principal.MayChangeUserRights(personCountryId))
            {
                user.IsApiAccessPermitted = stored.IsApiAccessPermitted;
                user.MayUploadSkpDrawing = stored.MayUploadSkpDrawing;
                user.MayManageWiFreds = stored.MayManageWiFreds;
                user.AdministratorAreaOfResposibility = stored.AdministratorAreaOfResposibility;
            }
            if (!user.IsGlobalAdministrator && !user.IsCountryAdministrator) user.AdministratorAreaOfResposibility = stored.AdministratorAreaOfResposibility;
        }
    }
}