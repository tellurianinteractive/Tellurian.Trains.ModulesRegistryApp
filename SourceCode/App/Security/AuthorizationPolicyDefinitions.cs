using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Security;

internal static class AuthorizationPolicyDefinitions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AppPolicyNames.Admin, policy => policy
                .RequireClaim(AppClaimTypes.CountryAdministrator, "True")
                .RequireClaim(AppClaimTypes.LastTermsOfUseAcceptTime)
                .RequireAssertion(ctx => !ctx.User.IsLockedOut()))
            .AddPolicy(AppPolicyNames.User, policy => policy
                .RequireClaim(AppClaimTypes.UserId)
                .RequireClaim(AppClaimTypes.LastTermsOfUseAcceptTime)
                .RequireAssertion(ctx => !ctx.User.IsLockedOut()));
        return services;
    }
}

