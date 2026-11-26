using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

/// <summary>
/// Checks an API key. Separated from <see cref="ApiKeyFilter"/> so the endpoints can be tested without a database.
/// </summary>
public interface IApiKeyValidator
{
    /// <summary>The user of a valid API key, with the claims a signed-in user has; null for a key that isn't valid.</summary>
    Task<ClaimsPrincipal?> FindPrincipalAsync(string? apiKey);
}

/// <summary>
/// The user of an API key gets the claims the app gives a signed-in user (its <see cref="IClaimsTransformation"/>), so the services
/// check permissions, and the change log records the user, as for the user's own requests.
/// </summary>
internal sealed class ApiKeyValidator(UserService userService, IClaimsTransformation claimsTransformation) : IApiKeyValidator
{
    public const string AuthenticationType = "ApiKey";

    public async Task<ClaimsPrincipal?> FindPrincipalAsync(string? apiKey)
    {
        var user = await userService.FindByApiKeyAsync(apiKey);
        if (user is null) return null;
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(AppClaimTypes.ObjectId, user.ObjectId.ToString())], AuthenticationType));
        return await claimsTransformation.TransformAsync(principal);
    }
}

/// <summary>
/// Requires a valid API key on every <c>/api</c> endpoint: in the <c>X-API-Key</c> header, or in the <c>apiKey</c> query parameter.
/// Without one the response is <c>401 Unauthorized</c> with an empty body. With one, the request's user is the key's user
/// (<see cref="IApiKeyValidator.FindPrincipalAsync"/>), also when the request has a sign-in cookie.
/// </summary>
public sealed class ApiKeyFilter(IApiKeyValidator validator) : IEndpointFilter
{
    public const string HeaderName = "X-API-Key";
    public const string QueryParameterName = "apiKey";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (await validator.FindPrincipalAsync(ReadApiKey(context.HttpContext.Request)) is not { } principal) return TypedResults.Unauthorized();
        context.HttpContext.User = principal;
        return await next(context);
    }

    private static string? ReadApiKey(HttpRequest request)
    {
        if (request.Headers.TryGetValue(HeaderName, out var headerValue))
        {
            var header = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(header)) return header;
        }
        return request.Query.TryGetValue(QueryParameterName, out var queryValue) ? queryValue.ToString() : null;
    }
}