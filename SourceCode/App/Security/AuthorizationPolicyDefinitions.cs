using Microsoft.Extensions.DependencyInjection;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Security
{
    internal static class AuthorizationPolicyDefinitions
    {
        public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(AppPolicyNames.Admin, policy => policy.RequireClaim(AppClaimTypes.CountryAdministrator, "True").RequireClaim(AppClaimTypes.LastTermsOfUseAcceptTime));
                options.AddPolicy(AppPolicyNames.User, policy => policy.RequireClaim(AppClaimTypes.UserId).RequireClaim(AppClaimTypes.LastTermsOfUseAcceptTime));
            });
            return services;
        }
    }
}
