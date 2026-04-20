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

        await using var db = await Factory.CreateDbContextAsync().ConfigureAwait(false);
        var user = await db.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => objectGuid == u.ObjectId).ConfigureAwait(false);
        if (user is null) return principal;

        foreach (var claim in await GetUserClaimsAsync(db, user).ConfigureAwait(false))
        {
            newIdentity.AddClaim(claim);
        }
        return clonedPrincipal;
    }

    private async Task<IEnumerable<Claim>> GetUserClaimsAsync(ModulesDbContext db, User user)
    {
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
            var domainIds = await db.GroupDomains.AsNoTracking()
                .Where(gd => gd.Groups.Any(g => g.GroupMembers.Any(gm => gm.PersonId == person.Id)))
                .Select(g => g.Id)
                .Distinct()
                .ToListAsync().ConfigureAwait(false);
            foreach (var domainId in domainIds) result.Add(Claim(AppClaimTypes.DomainId, domainId));
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
            if (user.IsLockedOut()) result.Add(Claim(AppClaimTypes.IsLockedOut, true));
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
        new(type, value.ToString() ?? throw new ArgumentNullException(nameof(value)), type, nameof(ModulesRegistry));
    private static Claim Claim(string type, string value) =>
        new(type, value, null, nameof(ModulesRegistry));
}
