using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;

public static class AppPolicyNames
{
    public const string Admin = nameof(Admin);
    public const string User = nameof(User);
}

public static class AppClaimTypes
{
    public const string GlobalAdministrator = nameof(GlobalAdministrator);
    public const string CountryAdministrator = nameof(CountryAdministrator);
    public const string PersonId = nameof(PersonId);
    public const string UserId = nameof(UserId);
    public const string ObjectId = nameof(ObjectId);
    public const string CountryId = nameof(CountryId);
    public const string LastTermsOfUseAcceptTime = nameof(LastTermsOfUseAcceptTime);
    public const string ReadOnly = nameof(ReadOnly);
    public const string Demo = nameof(Demo);
    public const string DomainId = nameof(DomainId);
    public const string MayUploadSkpDrawing = nameof(MayUploadSkpDrawing);
}

public static class ClaimsPrincipalExtensions
{
    public static string EmailAddess(this ClaimsPrincipal principal) => principal.GetString(ClaimTypes.Email) ?? string.Empty;
    public static string? ObjectId(this ClaimsPrincipal? principal) => principal?.GetString(AppClaimTypes.ObjectId);
    public static string? GivenName(this ClaimsPrincipal principal) => principal.GetString(ClaimTypes.GivenName);
    public static string? Surname(this ClaimsPrincipal principal) => principal.GetString(ClaimTypes.Surname);
    public static int UserId(this ClaimsPrincipal? principal) => principal.GetInt32(AppClaimTypes.UserId);
    public static int PersonId(this ClaimsPrincipal? principal) => principal.GetInt32(AppClaimTypes.PersonId);
    public static int CountryId(this ClaimsPrincipal? principal) => principal.GetInt32(AppClaimTypes.CountryId);
    public static int CountryId(this ClaimsPrincipal? principal, int? maybeCountryId) => maybeCountryId ?? (principal.IsGlobalAdministrator() ? 0 : principal.CountryId());
    public static bool IsDemo(this ClaimsPrincipal? principal) => principal.GetBool(AppClaimTypes.Demo);
    public static bool IsReadOnly(this ClaimsPrincipal? principal) => principal.GetBool(AppClaimTypes.ReadOnly);
    public static bool MayUploadSkpDrawing(this ClaimsPrincipal? principal) => principal.GetBool(AppClaimTypes.MayUploadSkpDrawing);

    public static bool IsLatestTermsOfUseAccepted([NotNullWhen(true)] this ClaimsPrincipal? principal) => principal.Any(AppClaimTypes.LastTermsOfUseAcceptTime);
    public static bool IsAuthenticated([NotNullWhen(true)] this ClaimsPrincipal? principal) => principal.Any(AppClaimTypes.ObjectId);
    public static bool IsGlobalAdministrator([NotNullWhen(true)] this ClaimsPrincipal? principal) => principal.Any(AppClaimTypes.GlobalAdministrator);
    public static bool IsCountryAdministrator([NotNullWhen(true)] this ClaimsPrincipal? principal) => principal.Any(AppClaimTypes.CountryAdministrator);

    #region Administrator scope

    public static bool IsAnyAdministrator([NotNullWhen(true)] this ClaimsPrincipal? principal) =>
        principal is not null && (principal.IsGlobalAdministrator() || principal.IsCountryAdministrator());

    public static bool IsAnyGroupAdministrator([NotNullWhen(true)] this ClaimsPrincipal? principal, Group? group) =>
        principal is not null && group is not null &&
        (   principal.IsCountryAdministratorInCountry(group.CountryId) ||
            (group.GroupMembers?.Any(gm => gm.PersonId == principal.PersonId() && (gm.IsDataAdministrator || gm.IsGroupAdministrator)) == true)
        );
    public static bool IsCountryAdministratorInCountry([NotNullWhen(true)] this ClaimsPrincipal? principal, int? countryId) =>
        principal.IsGlobalAdministrator() || (principal.IsCountryAdministrator() && countryId.HasValue && countryId.Value == principal.CountryId());

    public static bool IsAuthorisedInCountry([NotNullWhen(true)] this ClaimsPrincipal? principal, int? countryId) =>
        principal is not null && (principal.IsGlobalAdministrator() || (countryId == principal.CountryId()));

    public static bool IsAuthorisedInCountry([NotNullWhen(true)] this ClaimsPrincipal? principal, int? countryId, int personId) =>
         principal is not null && countryId.HasValue && (personId == principal.PersonId() || principal.IsGlobalAdministrator() || countryId.Value == principal.CountryId());

    public static bool IsMeetingOrganiserOrAdministrator([NotNullWhen(true)] this ClaimsPrincipal? principal, Meeting? meeting) =>
        principal is not null && meeting is not null &&
        (   principal.IsGlobalAdministrator() ||
            principal.IsCountryAdministratorInCountry(meeting?.OrganiserGroup?.CountryId) ||
            principal.IsAnyGroupAdministrator(meeting?.OrganiserGroup)
        );
    public static bool IsGroupMemberAdministrator([NotNullWhen(true)] this ClaimsPrincipal? principal, IEnumerable<GroupMember> members) =>
        principal is not null && (principal.IsAuthorisedInCountry(principal.CountryId()) || principal.IsGlobalAdministrator() || members.Any(m => m.PersonId == principal.PersonId() && m.IsGroupAdministrator));

    public static bool IsGroupDataAdministrator([NotNullWhen(true)] this ClaimsPrincipal? principal, IEnumerable<GroupMember> members) =>
        principal is not null && (principal.IsAuthorisedInCountry(principal.CountryId()) || principal.IsGlobalAdministrator() || members.Any(m => m.PersonId == principal.PersonId() && m.IsDataAdministrator));

    public static bool IsMemberOfGroupSpecificGroupDomainOrNone([NotNullWhen(true)] this ClaimsPrincipal? principal, int? groupDomainId) =>
        groupDomainId is null || principal.None(AppClaimTypes.DomainId) || principal.IsGlobalAdministrator() || principal.GroupDomainIds().Contains(groupDomainId.Value);

    #endregion

    #region Data modification rights
    public static async ValueTask<bool> MayEdit([NotNullWhen(true)] this ClaimsPrincipal? principal, ModuleOwnershipRef ownershipRef, GroupService groupService)
    {
        if (principal is null) return false;
        if (ownershipRef.IsPerson || ownershipRef.IsPersonInGroup)
        {
            if (ownershipRef.PersonId == principal.PersonId()) return true;
            return await groupService.IsDataAdministratorInSameGroupAsMember(principal, ownershipRef.PersonId).ConfigureAwait(false);
        }
        else if (ownershipRef.IsGroup)
        {
            return await groupService.IsGroupDataAdministratorAsync(principal, ownershipRef.GroupId).ConfigureAwait(false);
        }
        return false;
    }

    public static bool MayRead([NotNullWhen(true)] this ClaimsPrincipal? principal) =>
        principal.MayRead(principal.PersonId());

    public static bool MayRead([NotNullWhen(true)] this ClaimsPrincipal? principal, int entityOwnerId) =>
         principal is not null && (entityOwnerId == principal.PersonId() || principal.IsAuthorisedInCountry(principal.CountryId()) || principal.IsGlobalAdministrator());

    public static bool MaySave([NotNullWhen(true)] this ClaimsPrincipal? principal) =>
        principal?.IsReadOnly() == false && (principal.IsAuthorisedInCountry(principal.CountryId()) || principal.IsGlobalAdministrator());

    public static bool MaySave([NotNullWhen(true)] this ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef, bool isGroupAdministrator = false) =>
        principal?.IsReadOnly() == false && (isGroupAdministrator || principal.IsCountryAdministratorInCountry(principal.CountryId()) || principal.IsGlobalAdministrator() || (ownerRef.IsPerson && ownerRef.PersonId == principal.PersonId()));

    public static bool MayDelete([NotNullWhen(true)] this ClaimsPrincipal? principal) =>
        principal?.IsReadOnly() == false && (principal.IsAuthorisedInCountry(principal.CountryId()));

    public static bool MayDelete([NotNullWhen(true)] this ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef, bool userMayDelete = false) =>
         principal?.IsReadOnly() == false && ((userMayDelete && ownerRef.IsPerson && ownerRef.PersonId == principal.PersonId()) && (principal.IsAuthorisedInCountry(principal.CountryId()) || principal.IsGlobalAdministrator()));

    public static bool MayDelete([NotNullWhen(true)] this ClaimsPrincipal? principal, Meeting? meeting) =>
        principal is not null && principal.IsAnyAdministrator() && meeting is not null && meeting.Layouts.Sum(l => l.LayoutParticipants.Count) == 0;

    #endregion

    #region Module ownership 

    public static ModuleOwnershipRef OwnerRef(this ClaimsPrincipal? principal) => 
        principal is null ? ModuleOwnershipRef.None : ModuleOwnershipRef.Person(principal.PersonId());
    public static ModuleOwnershipRef UpdateFrom(this ClaimsPrincipal? principal, ModuleOwnershipRef original) =>
        ModuleOwnershipRef.WithPrincipal(original, principal);
    public static ModuleOwnershipRef AsModuleOwnershipRef(this ClaimsPrincipal? principal) => principal is null ? ModuleOwnershipRef.None : ModuleOwnershipRef.Person(principal.PersonId());

    public static int PersonOwnerId(this ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef) => 
        principal is null ? 0 : ownerRef.IsPerson ? ownerRef.PersonId : principal.PersonId();

    public static IList<int> GroupDomainIds(this ClaimsPrincipal? principal) =>
        principal is null ? Array.Empty<int>() :
        principal.Claims.Where(c => c.Type == AppClaimTypes.DomainId).Select(_ => principal.GetInt32(AppClaimTypes.DomainId)).ToList();

    public static int MinimumObjectVisibility(this ClaimsPrincipal? principal, ModuleOwnershipRef ownerRef, bool isMemberInGroupsInSameDomain) =>
        principal is null ? (int)ObjectVisibility.Public :
        principal.IsAnyAdministrator() || ownerRef.PersonId == principal.PersonId() ? (int)ObjectVisibility.Private :
        ownerRef.IsPersonInGroup ? (int)ObjectVisibility.GroupMembers :
        ownerRef.IsPerson && isMemberInGroupsInSameDomain ? (int)ObjectVisibility.DomainMembers :
        ownerRef.GroupId > 0 ? (int)ObjectVisibility.GroupMembers : (int)ObjectVisibility.Private;

    #endregion

    #region Basic claims functions
    private static string? GetString(this ClaimsPrincipal? principal, string claimType, string? defaultValue = null) =>
        principal is not null ? principal.Claims.Claim(claimType)?.Value : defaultValue;

    private static int GetInt32(this ClaimsPrincipal? principal, string claimType) =>
        principal is not null && int.TryParse(principal.Claims.Claim(claimType)?.Value, out var value) ? value : 0;

    private static bool GetBool(this ClaimsPrincipal? principal, string claimType) =>
        principal is not null && bool.TryParse(principal.Claims.Claim(claimType)?.Value, out var value) && value;

    private static bool Any(this ClaimsPrincipal? principal, string claimType) => principal?.Claims.Any(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)) == true;
    private static bool None(this ClaimsPrincipal? principal, string claimType) => !principal.Any(claimType); 
    private static Claim? Claim(this IEnumerable<Claim> claims, string claimType) =>
        claims.SingleOrDefault(c => c?.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) == true);

    #endregion
}
