using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Security;

public class ApplicationClaimsTransformation(IDbContextFactory<ModulesDbContext> factory, ContentService contentService, AppService appService) : IClaimsTransformation
{
    private readonly IDbContextFactory<ModulesDbContext> Factory = factory;
    private readonly ContentService ContentService = contentService;
    private readonly AppService AppService = appService;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (!Guid.TryParse(principal.ObjectId(), out var objectGuid)) return principal;
        var clonedPrincipal = principal.Clone();
        if (clonedPrincipal.Identity is not ClaimsIdentity newIdentity) return principal;

        // Runs for every request from a signed-in user, so everything is read in one query (one round trip to the database).
        await using var db = await Factory.CreateDbContextAsync().ConfigureAwait(false);
        var data = await db.Users.AsNoTracking()
            .Where(u => u.ObjectId == objectGuid)
            .Select(u => new UserClaimsData(
                u,
                u.Person == null ? null : new PersonClaimsData(u.Person.Id, u.Person.FirstName, u.Person.LastName, u.Person.CountryId),
                u.Person.GroupMembers.Where(gm => gm.Group.GroupDomainId != null).Select(gm => gm.Group.GroupDomainId!.Value).ToList()))
            .SingleOrDefaultAsync().ConfigureAwait(false);
        if (data is null) return principal;

        foreach (var claim in await GetUserClaimsAsync(data).ConfigureAwait(false))
        {
            newIdentity.AddClaim(claim);
        }
        return clonedPrincipal;
    }

    private sealed record UserClaimsData(User User, PersonClaimsData? Person, List<int> DomainIds);
    private sealed record PersonClaimsData(int Id, string FirstName, string LastName, int CountryId);

    private async Task<IEnumerable<Claim>> GetUserClaimsAsync(UserClaimsData data)
    {
        var user = data.User;
        var result = new List<Claim>(20)
            {
                Claim(AppClaimTypes.UserId, user.Id)
            };
        AddAdministratorClaims(user, result);

        if (user.LastTermsOfUseAcceptTime is not null)
        {
            var lastTermsOfUseChanged = await ContentService.GetLastModifiedTimeOfTextContent("TermsOfUse").ConfigureAwait(false);
            if (lastTermsOfUseChanged < user.LastTermsOfUseAcceptTime)
            {
                result.Add(Claim(AppClaimTypes.LastTermsOfUseAcceptTime, true));
            }
        }

        var person = data.Person;
        if (person is not null)
        {
            AddPersonalClaims(person, result);
            AppService.LastCountryId = person.CountryId;
            // The domains of the groups the person is a member of.
            foreach (var domainId in data.DomainIds.Distinct()) result.Add(Claim(AppClaimTypes.DomainId, domainId));
        }
        return result;

        static void AddAdministratorClaims(User user, List<Claim> result)
        {
            if (user.IsGlobalAdministrator) result.Add(Claim(AppClaimTypes.GlobalAdministrator, true));
            if (user.IsCountryAdministrator) result.Add(Claim(AppClaimTypes.CountryAdministrator, true));
            if (user.IsReadOnly) result.Add(Claim(AppClaimTypes.ReadOnly, true));
            if (user.IsDemo) result.Add(Claim(AppClaimTypes.Demo, true));
            // Special claims
            if (user.MayUploadSkpDrawing) result.Add(Claim(AppClaimTypes.MayUploadSkpDrawing, true));
            if (user.MayManageWiFreds) result.Add(Claim(AppClaimTypes.MayManageWiFreds, true));
            if (user.MayManageEndProfiles) result.Add(Claim(AppClaimTypes.MayManageEndProfiles, true));
            if (user.IsLockedOut()) result.Add(Claim(AppClaimTypes.IsLockedOut, true));
        }

        static void AddPersonalClaims(PersonClaimsData person, List<Claim> result)
        {
            result.Add(Claim(ClaimTypes.GivenName, person.FirstName));
            result.Add(Claim(ClaimTypes.Surname, person.LastName));
            result.Add(Claim(ClaimTypes.Name, $"{person.FirstName} {person.LastName}"));
            result.Add(Claim(AppClaimTypes.PersonId, person.Id));
            result.Add(Claim(AppClaimTypes.CountryId, person.CountryId));
        }
    }

    private static Claim Claim(string type, object value) =>
        new(type, value.ToString() ?? throw new ArgumentNullException(nameof(value)), type, nameof(ModulesRegistry));
    private static Claim Claim(string type, string value) =>
        new(type, value, null, nameof(ModulesRegistry));
}
