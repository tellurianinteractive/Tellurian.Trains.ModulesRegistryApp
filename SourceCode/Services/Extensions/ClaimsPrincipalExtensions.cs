using ModulesRegistry.Data;
using ModulesRegistry.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ModulesRegistry.Services.Extensions
{
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
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string EmailAddess(this ClaimsPrincipal me) => me.GetString(ClaimTypes.Email) ?? string.Empty;
        public static string? ObjectId(this ClaimsPrincipal? me) => me?.GetString(AppClaimTypes.ObjectId);
        public static string? GivenName(this ClaimsPrincipal me) => me.GetString(ClaimTypes.GivenName);
        public static string? Surname(this ClaimsPrincipal me) => me.GetString(ClaimTypes.Surname);
        public static int UserId(this ClaimsPrincipal? me) => me.GetInt32(AppClaimTypes.UserId);
        public static int PersonId(this ClaimsPrincipal? me) => me.GetInt32(AppClaimTypes.PersonId);
        public static int CountryId(this ClaimsPrincipal? me) => me.GetInt32(AppClaimTypes.CountryId);
        public static bool IsDemo(this ClaimsPrincipal? me) => me.GetBool(AppClaimTypes.Demo);
        public static bool IsReadOnly(this ClaimsPrincipal? me) => me.GetBool(AppClaimTypes.ReadOnly);

        public static bool IsLatestTermsOfUseAccepted([NotNullWhen(true)] this ClaimsPrincipal? me) => me.Any(AppClaimTypes.LastTermsOfUseAcceptTime);
        public static bool IsAuthenticated([NotNullWhen(true)] this ClaimsPrincipal? me) => me.Any(AppClaimTypes.ObjectId);
        public static bool IsGlobalAdministrator([NotNullWhen(true)] this ClaimsPrincipal? me) => me.Any(AppClaimTypes.GlobalAdministrator);
        public static bool IsCountryAdministrator([NotNullWhen(true)] this ClaimsPrincipal? me) => me.Any(AppClaimTypes.CountryAdministrator);
        public static bool IsCountryAdministratorInCountry([NotNullWhen(true)] this ClaimsPrincipal? me, int? countryId) =>
            me.IsGlobalAdministrator() || (me.IsCountryAdministrator() && countryId.HasValue && countryId.Value == me.CountryId());

        public static bool IsAnyAdministrator([NotNullWhen(true)] this ClaimsPrincipal? me) =>
            me is not null && (me.IsGlobalAdministrator() || me.IsCountryAdministrator());

        public static bool IsAnyGroupAdministrator([NotNullWhen(true)] this ClaimsPrincipal? me, Group? group) =>
            me is not null && group is not null &&
            (me.IsCountryAdministratorInCountry(group.CountryId) ||
                (group.GroupMembers?.Any(gm => gm.PersonId == me.PersonId() && (gm.IsDataAdministrator || gm.IsGroupAdministrator)) == true)
            );

        public static bool IsAuthorisedInCountry([NotNullWhen(true)] this ClaimsPrincipal? me, int? countryId) =>
            me is not null && (me.IsGlobalAdministrator() || (countryId == me.CountryId()));
        public static bool IsAuthorisedInCountry([NotNullWhen(true)] this ClaimsPrincipal? me, int? countryId, int personId) =>
             me is not null && countryId.HasValue && (personId == me.PersonId() || me.IsGlobalAdministrator() || countryId.Value == me.CountryId());
        public static ModuleOwnershipRef UpdateFrom(this ClaimsPrincipal? me, ModuleOwnershipRef original) =>
            ModuleOwnershipRef.WithPrincipal(original, me);
        public static ModuleOwnershipRef AsModuleOwnershipRef(this ClaimsPrincipal? me) => me is null ? ModuleOwnershipRef.None : ModuleOwnershipRef.Person(me.PersonId());

        public static async ValueTask<bool> MayEdit([NotNullWhen(true)] this ClaimsPrincipal? me, ModuleOwnershipRef ownershipRef, GroupService groupService)
        {
            if (me is null) return false;
            if (ownershipRef.IsPerson)
            {
                if (ownershipRef.PersonId == me.PersonId()) return true;
                return await groupService.IsDataAdministratorInSameGroupAsMember(me, ownershipRef.PersonId).ConfigureAwait(false);
            }
            else if (ownershipRef.IsGroup)
            {
                return await groupService.IsGroupDataAdministratorAsync(me, ownershipRef.GroupId).ConfigureAwait(false);
            }
            return false;
        }

        public static bool MayRead([NotNullWhen(true)] this ClaimsPrincipal? me) =>
            me.MayRead(me.PersonId());

        public static bool MayRead([NotNullWhen(true)] this ClaimsPrincipal? me, int entityOwnerId) =>
             me is not null && (entityOwnerId == me.PersonId() || me.IsAuthorisedInCountry(me.CountryId()) || me.IsGlobalAdministrator());

        public static bool MaySave([NotNullWhen(true)] this ClaimsPrincipal? me) =>
            me?.IsReadOnly() == false && (me.IsAuthorisedInCountry(me.CountryId()) || me.IsGlobalAdministrator());

        public static bool MaySave([NotNullWhen(true)] this ClaimsPrincipal? me, ModuleOwnershipRef ownerRef, bool isGroupAdministrator = false) =>
            me?.IsReadOnly() == false && (isGroupAdministrator || me.IsCountryAdministratorInCountry(me.CountryId()) || me.IsGlobalAdministrator() || (ownerRef.IsPerson && ownerRef.PersonId == me.PersonId()));

        public static bool MayDelete([NotNullWhen(true)] this ClaimsPrincipal? me) =>
            me?.IsReadOnly() == false && (me.IsAuthorisedInCountry(me.CountryId()) || me.IsGlobalAdministrator());

        public static bool MayDelete([NotNullWhen(true)] this ClaimsPrincipal? me, ModuleOwnershipRef ownerRef, bool userMayDelete = false) =>
             me?.IsReadOnly() == false && ((userMayDelete && ownerRef.IsPerson && ownerRef.PersonId == me.PersonId()) || me.IsAuthorisedInCountry(me.CountryId()) || me.IsGlobalAdministrator());

        public static ModuleOwnershipRef OwnerRef(this ClaimsPrincipal? me) => me is null ? ModuleOwnershipRef.None : ModuleOwnershipRef.Person(me.PersonId());

        public static int PersonOwnerId(this ClaimsPrincipal? me, ModuleOwnershipRef ownerRef) => me is null ? 0 : ownerRef.IsPerson ? ownerRef.PersonId : me.PersonId();
        public static bool IsGroupMemberAdministrator([NotNullWhen(true)] this ClaimsPrincipal? me, IEnumerable<GroupMember> members) =>
            me is not null && (me.IsAuthorisedInCountry(me.CountryId()) || me.IsGlobalAdministrator() || members.Any(m => m.PersonId == me.PersonId() && m.IsGroupAdministrator));

        public static bool IsGroupDataAdministrator([NotNullWhen(true)] this ClaimsPrincipal? me, IEnumerable<GroupMember> members) =>
            me is not null && (me.IsAuthorisedInCountry(me.CountryId()) || me.IsGlobalAdministrator() || members.Any(m => m.PersonId == me.PersonId() && m.IsDataAdministrator));

        public static IList<int> GroupDomainIds(this ClaimsPrincipal? me) =>
            me is null ? Array.Empty<int>() :
            me.Claims.Where(c => c.Type == AppClaimTypes.DomainId).Select(_ => me.GetInt32(AppClaimTypes.DomainId)).ToList();

        public static bool IsMemberOfGroupSpecificGroupDomainOrNone([NotNullWhen(true)] this ClaimsPrincipal? me, int? groupDomainId) =>
            groupDomainId is null || me.None(AppClaimTypes.DomainId) || me.IsGlobalAdministrator() || me.GroupDomainIds().Contains(groupDomainId.Value);

        public static int MinimumObjectVisibility(this ClaimsPrincipal? me, ModuleOwnershipRef ownerRef, bool isMemberInGroupsInSameDomain) =>
            me is null ? (int)ObjectVisibility.Public :
            me.IsAnyAdministrator() || ownerRef.PersonId == me.PersonId() ? (int)ObjectVisibility.Private :
            ownerRef.IsPerson && isMemberInGroupsInSameDomain ? (int)ObjectVisibility.DomainMembers :
            ownerRef.GroupId > 0 ? (int)ObjectVisibility.GroupMembers : (int)ObjectVisibility.Private;

        private static string? GetString(this ClaimsPrincipal? me, string claimType, string? defaultValue = null) =>
            me is not null ? me.Claims.Claim(claimType)?.Value : defaultValue;

        private static int GetInt32(this ClaimsPrincipal? me, string claimType) =>
            me is not null && int.TryParse(me.Claims.Claim(claimType)?.Value, out var value) ? value : 0;

        private static bool GetBool(this ClaimsPrincipal? me, string claimType) =>
            me is not null && bool.TryParse(me.Claims.Claim(claimType)?.Value, out var value) && value;

        private static Claim? Claim(this IEnumerable<Claim> us, string claimType) =>
            us.SingleOrDefault(c => c?.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase) == true);

        private static bool Any(this ClaimsPrincipal? me, string claimType) => me?.Claims.Any(c => c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase)) == true;
        private static bool None(this ClaimsPrincipal? me, string claimType) => !me.Any(claimType);
    }
}
