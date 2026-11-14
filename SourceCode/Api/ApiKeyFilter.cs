using Microsoft.AspNetCore.Http;
using ModulesRegistry.Services.Implementations;

namespace ModulesRegistry.Api;

/// <summary>
/// Checks an API key. Separated from <see cref="ApiKeyFilter"/> so the endpoints can be tested without a database.
/// </summary>
public interface IApiKeyValidator
{
    Task<bool> IsValidAsync(string? apiKey);
}

internal sealed class ApiKeyValidator(UserService userService) : IApiKeyValidator
{
    public async Task<bool> IsValidAsync(string? apiKey) => await userService.FindByApiKeyAsync(apiKey) is not null;
}

/// <summary>
/// Requires a valid API key on every <c>/api</c> endpoint: in the <c>X-API-Key</c> header, or in the <c>apiKey</c> query parameter.
/// Without one the response is <c>401 Unauthorized</c> with an empty body.
/// </summary>
public sealed class ApiKeyFilter(IApiKeyValidator validator) : IEndpointFilter
{
    public const string HeaderName = "X-API-Key";
    public const string QueryParameterName = "apiKey";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (await validator.IsValidAsync(ReadApiKey(context.HttpContext.Request))) return await next(context);
        return TypedResults.Unauthorized();
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