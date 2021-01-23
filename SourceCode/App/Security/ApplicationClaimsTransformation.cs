using Microsoft.AspNetCore.Authentication;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Data;
using ModulesRegistry.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Security
{
    public class ApplicationClaimsTransformation : IClaimsTransformation
    {
        private readonly IUserService UserService;
        private readonly IPersonService PersonService;

        public ApplicationClaimsTransformation(IUserService userService, IPersonService personService)
        {
            UserService = userService;
            PersonService = personService;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var clonedPrincipal = principal.Clone();
            if (clonedPrincipal is null) return principal;
            var newIdentity = (ClaimsIdentity?)clonedPrincipal.Identity;
            if (newIdentity is null) return principal;
            var objectId = principal.ObjectId();
            if (objectId is null) return principal;
            var user = await UserService.FindOrCreateAsync(objectId, principal.EmailAddess());
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
            var result = new List<Claim>(20)
            {
                Claim("User", $"{user.Id}")
            };
            AddGlobalAdministratorClaim(user, result);
            AddCookieConsentTimeClaim(user, result);
            var person = await PersonService.GetByAsync(user.Id);
            AddPersonalClaims(result, person); return result;

            static void AddCookieConsentTimeClaim(User user, List<Claim> result) =>
                result.Add(CookieConsentTimeClaim(user));
            static Claim CookieConsentTimeClaim(User user) =>
                user.LastTermsOfUseAcceptTime is null ?
                Claim(nameof(user.LastTermsOfUseAcceptTime), DateTimeOffset.MinValue.ToString("o")) :
                Claim(nameof(user.LastTermsOfUseAcceptTime), user.LastTermsOfUseAcceptTime.Value.ToString("o"));

            static void AddGlobalAdministratorClaim(User user, List<Claim> result)
            {
                if (user.IsGlobalAdministrator) result.Add(Claim("Administrator", "Global"));
            }

            static void AddPersonalClaims(List<Claim> result, Person? person)
            {
                if (person is not null)
                {
                    result.Add(Claim("Person", $"{person.Id}"));
                    result.Add(Claim(ClaimTypes.GivenName, person.FirstName));
                    result.Add(Claim(ClaimTypes.Surname, person.LastName));
                    result.Add(Claim(ClaimTypes.Email, person.EmailAddresses));
                }
            }
        }
        private static Claim Claim(string type, string value) => new Claim(type, value, null, nameof(ModulesRegistry));
    }
}
