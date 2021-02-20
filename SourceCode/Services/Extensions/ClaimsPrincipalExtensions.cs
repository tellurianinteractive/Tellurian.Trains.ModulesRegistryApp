using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;

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
    }

    public static class ClaimsPrincipalExtensions
    {
        public static string EmailAddess(this ClaimsPrincipal me) => me.GetString(ClaimTypes.Email) ?? string.Empty;
        public static string? ObjectId(this ClaimsPrincipal me) => me.GetString(AppClaimTypes.ObjectId);
        public static string? GivenName(this ClaimsPrincipal me) => me.GetString(ClaimTypes.GivenName);
        public static string? Surname(this ClaimsPrincipal me) => me.GetString(ClaimTypes.Surname);
        public static int UserId(this ClaimsPrincipal? me) => me.GetInt32(AppClaimTypes.UserId);
        public static int PersonId(this ClaimsPrincipal? me) => me.GetInt32(AppClaimTypes.PersonId);
        public static int CountryId(this ClaimsPrincipal? me) => me.GetInt32(AppClaimTypes.PersonId);

        public static bool IsGlobalAdministrator(this ClaimsPrincipal me) => me.Claims.Any(c => c.Type == AppClaimTypes.GlobalAdministrator);
        public static bool IsCountryAdministrator(this ClaimsPrincipal me) => me.Claims.Any(c => c.Type == AppClaimTypes.CountryAdministrator);
        public static bool IsAuthorisedInCountry(this ClaimsPrincipal? principal, int countryId) => principal is not null && (principal.IsGlobalAdministrator() || countryId == principal.CountryId());

        private static string? GetString(this ClaimsPrincipal? me, string claimType, string? defaultValue = null) =>
            me is not null ? me.Claims.SingleOrDefault(c => c is not null && c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value : defaultValue;

        private static int GetInt32(this ClaimsPrincipal? me, string claimType) =>
            me is not null && int.TryParse(me.Claims.SingleOrDefault(c => c is not null && c.Type.Equals(claimType, StringComparison.OrdinalIgnoreCase))?.Value, out var value) ? value : 0;
    }
}
