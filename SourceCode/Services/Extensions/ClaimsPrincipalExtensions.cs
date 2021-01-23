using System;
using System.Linq;
using System.Security.Claims;

namespace ModulesRegistry.Services.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string Name(this ClaimsPrincipal me) =>
            me.Claims.SingleOrDefault(c => c is not null && c.Type.Equals("name", StringComparison.OrdinalIgnoreCase))?.Value ?? "Unknown";

        public static string PreferredUserName(this ClaimsPrincipal me) =>
            me.Claims.SingleOrDefault(c => c is not null && c.Type.Equals("preferred_username", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
        public static string EmailAddess(this ClaimsPrincipal me) =>
            me.Claims.SingleOrDefault(c => c is not null && c.Type.Equals("preferred_username", StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;

        public static string? ObjectId(this ClaimsPrincipal me) =>
           me.Claims.SingleOrDefault(c => c is not null && c.Type.Equals("http://schemas.microsoft.com/identity/claims/objectidentifier", StringComparison.OrdinalIgnoreCase))?.Value;

        public static string? GivenName(this ClaimsPrincipal me) =>
            me.Claims.SingleOrDefault(c => c is not null && c.Type.Equals(ClaimTypes.GivenName, StringComparison.OrdinalIgnoreCase))?.Value;
    }
}
