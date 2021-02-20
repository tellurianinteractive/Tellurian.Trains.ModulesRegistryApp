using Microsoft.AspNetCore.Authentication;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Data;
using ModulesRegistry.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ModulesRegistry.Security
{
    public class ApplicationClaimsTransformation : IClaimsTransformation
    {
        private readonly IDbContextFactory<ModulesDbContext> Factory;

        public ApplicationClaimsTransformation(IDbContextFactory<ModulesDbContext> factory)
        {
            Factory = factory;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            using var db = Factory.CreateDbContext();
            var clonedPrincipal = principal.Clone();
            if (clonedPrincipal is null) return principal;
            var newIdentity = (ClaimsIdentity?)clonedPrincipal.Identity;
            if (newIdentity is null) return principal;
            var objectId = principal.ObjectId();
            if (objectId is null) return principal;
            var objectGuid = Guid.Parse(objectId);
            var user = await db.Users.Where(u => u.ObjectId == objectGuid).SingleOrDefaultAsync();
            if (user is null) return principal;

            foreach (var claim in await GetUserClaimsAsync(user))
            {
                newIdentity.AddClaim(claim);
            }
            return clonedPrincipal;
        }
        /// <summary>
        /// This is the function that adds all local <see cref="Claim">claims</see>.
        /// </summary>
        /// <param name="user">The user to assign <see cref="Claim">claims</see> for.</param>
        /// <returns></returns>
        private async Task<IEnumerable<Claim>> GetUserClaimsAsync(User user)
        {
            using var db = Factory.CreateDbContext();

            var result = new List<Claim>(20)
            {
                Claim(AppClaimTypes.UserId, user.Id)
            };
            AddAdministratorClaims(user, result);
            AddLastTermsOfUseAcceptTimeClaim(user, result);

            var person = await db.People.SingleOrDefaultAsync(p => p.UserId == user.Id);
            if (person is not null)
            {
                AddPersonalClaims(person, result);
            }
            return result;

            static void AddLastTermsOfUseAcceptTimeClaim(User user, List<Claim> result) =>
                result.Add(CookieConsentTimeClaim(user));

            static Claim CookieConsentTimeClaim(User user) =>
                user.LastTermsOfUseAcceptTime is null ?
                Claim(nameof(user.LastTermsOfUseAcceptTime), DateTimeOffset.MinValue.ToString("o")) :
                Claim(nameof(user.LastTermsOfUseAcceptTime), user.LastTermsOfUseAcceptTime.Value.ToString("o"));

            static void AddAdministratorClaims(User user, List<Claim> result)
            {
                if (user.IsGlobalAdministrator) result.Add(Claim(AppClaimTypes.GlobalAdministrator, true));
                if (user.IsCountryAdministrator) result.Add(Claim(AppClaimTypes.CountryAdministrator, true));
            }

            static void AddPersonalClaims(Person? person, List<Claim> result)
            {
                if (person is not null)
                {
                    result.Add(Claim(AppClaimTypes.PersonId, person.Id));
                    result.Add(Claim(ClaimTypes.GivenName, person.FirstName));
                    result.Add(Claim(ClaimTypes.Surname, person.LastName));
                    result.Add(Claim(ClaimTypes.Email, person.EmailAddresses));
                    result.Add(Claim(AppClaimTypes.CountryId, person.CountryId));
                }
            }

        }
        private static Claim Claim(string type, object value) =>
            new Claim(type, value.ToString() ?? throw new ArgumentNullException(nameof(value)), null, nameof(ModulesRegistry));
        private static Claim Claim(string type, string value) =>
            new Claim(type, value, null, nameof(ModulesRegistry));

    }
}
