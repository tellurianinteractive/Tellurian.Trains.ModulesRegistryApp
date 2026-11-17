using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using System.Security.Claims;

namespace ModulesRegistry.Extensions;

/// <summary>
/// Who the user administration page sends mail to, besides all users and all with an email address in a country.
/// </summary>
public static class UserMailRecipientsExtensions
{
    extension(IEnumerable<User> administrators)
    {
        /// <summary>
        /// The administrators with an email address that <paramref name="principal"/> may send mail to: all global administrators,
        /// and the country administrators in all countries for a global administrator, otherwise only in the own country.
        /// </summary>
        public IEnumerable<Person> AdministratorsFor(ClaimsPrincipal? principal) =>
            administrators
                .Where(u => u.IsGlobalAdministrator || (u.IsCountryAdministrator && (principal.IsGlobalAdministrator() || u.Person?.CountryId == principal.CountryId())))
                .Select(u => u.Person)
                .Where(p => p is not null && p.PrimaryEmail().HasValue())
                .OrderBy(p => p.FirstName).ThenBy(p => p.LastName);
    }

    extension(IEnumerable<Person> people)
    {
        /// <summary>
        /// Persons with an email address whose user has signed in or created a password, but has not been active since <paramref name="date"/>.
        /// </summary>
        public IEnumerable<Person> NotSignedInSince(DateTimeOffset date) =>
            people.Where(p => p.PrimaryEmail().HasValue() && p.User.LastActivityTime() < date);
    }

    extension(User? user)
    {
        /// <summary>
        /// The last sign in, or when the password was created for a user that has not signed in yet;
        /// null for a user that has done neither (only invited) or no user at all.
        /// </summary>
        public DateTimeOffset? LastActivityTime() =>
            user is null ? null :
            user.LastSignInTime.HasValue ? user.LastSignInTime :
            user.HasPassword() ? user.LastEmailConfirmationTime ?? user.RegistrationTime :
            null;
    }
}