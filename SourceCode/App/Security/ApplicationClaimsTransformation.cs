using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Implementations;
using System.Security.Claims;

namespace ModulesRegistry.Security;

public class ApplicationClaimsTransformation : IClaimsTransformation
{
    private readonly IDbContextFactory<ModulesDbContext> Factory;
    private readonly ContentService ContentService;
    private readonly AppService AppService;

    public ApplicationClaimsTransformation(IDbContextFactory<ModulesDbContext> factory, ContentService contentService, AppService appService)
    {
        Factory = factory;
        ContentService = contentService;
        AppService = appService;
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
        var user = await db.Users.Where(u => u.ObjectId == objectGuid).SingleOrDefaultAsync().ConfigureAwait(false);
        if (user is null) return principal;

        foreach (var claim in await GetUserClaimsAsync(user).ConfigureAwait(false))
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
        if (user.LastTermsOfUseAcceptTime is not null)
        {
            var lastTermsOfUseChanged = await ContentService.GetLastModifiedTimeOfTextContent("TermsOfUse").ConfigureAwait(false);
            if (lastTermsOfUseChanged < user.LastTermsOfUseAcceptTime)
            {
                result.Add(Claim(AppClaimTypes.LastTermsOfUseAcceptTime, true));
            }
        }
        var person = await db.People.AsNoTracking().SingleOrDefaultAsync(p => p.UserId == user.Id).ConfigureAwait(false);
        if (person is not null)
        {
            AddPersonalClaims(person, result);
            AppService.LastCountryId = person.CountryId;
            var groupDomainIds = db.GroupDomains.Where(gd => gd.Groups.Any(g => g.GroupMembers.Any(gm => gm.PersonId == person.Id))).Select(g => g.Id).Distinct();
            if (groupDomainIds is not null)
            {
                foreach (var domainId in groupDomainIds) result.Add(Claim(AppClaimTypes.DomainId, domainId));
            }
        }
        return result;

        static void AddAdministratorClaims(User user, List<Claim> result)
        {
            if (user.IsGlobalAdministrator) result.Add(Claim(AppClaimTypes.GlobalAdministrator, true));
            if (user.IsCountryAdministrator) result.Add(Claim(AppClaimTypes.CountryAdministrator, true));
            if (user.IsReadOnly) result.Add(Claim(AppClaimTypes.ReadOnly, true));
            if (user.IsDemo) result.Add(Claim(AppClaimTypes.Demo, true));
            if (user.MayUploadSkpDrawing) result.Add(Claim(AppClaimTypes.MayUploadSkpDrawing, true));
        }

        static void AddPersonalClaims(Person? person, List<Claim> result)
        {
            if (person is not null)
            {
                result.Add(Claim(ClaimTypes.GivenName, person.FirstName));
                result.Add(Claim(ClaimTypes.Surname, person.LastName));
                result.Add(Claim(ClaimTypes.Name, $"{person.FirstName} {person.LastName}"));
                result.Add(Claim(AppClaimTypes.PersonId, person.Id));
                result.Add(Claim(AppClaimTypes.CountryId, person.CountryId));
            }
        }
    }

    private static Claim Claim(string type, object value) =>
        new(type, value.ToString() ?? throw new ArgumentNullException(nameof(value)), null, nameof(ModulesRegistry));
    private static Claim Claim(string type, string value) =>
        new(type, value, null, nameof(ModulesRegistry));
}
