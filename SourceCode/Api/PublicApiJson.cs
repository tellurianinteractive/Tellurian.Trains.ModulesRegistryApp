using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;

namespace ModulesRegistry.Api;

/// <summary>
/// The JSON format of the public API. It is the format the MVC controllers had, kept so external consumers see no change:
/// web defaults (camelCase), indented, and <see cref="ReferenceHandler.Preserve"/>, so a list is written as
/// <c>{"$id":"1","$values":[...]}</c>. Non-ASCII letters are written as they are (<c>"Borås"</c>, not escaped as a Unicode sequence):
/// MVC's output formatter used <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/> when no encoder was configured. A format with plain arrays would come as a new API version (<c>/api/v2</c>).
/// </summary>
/// <remarks>
/// Only the <c>/api</c> endpoints use these options. The app's own <c>/app</c> endpoints keep the default minimal API format.
/// </remarks>
public static class PublicApiJson
{
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.Preserve,
        MaxDepth = 200,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}

/// <summary>
/// <c>200 OK</c> with a body in the <see cref="PublicApiJson"/> format. Like <see cref="Microsoft.AspNetCore.Http.HttpResults.Ok{TValue}"/>,
/// it describes <typeparamref name="TValue"/> as the response type in the OpenAPI document, so the documented type is the returned one.
/// </summary>
/// <remarks>
/// The value is serialised with its runtime type, as the MVC controllers did.
/// </remarks>
public sealed class PublicApiOk<TValue> : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult, IValueHttpResult<TValue>
{
    internal PublicApiOk(TValue value) => Value = value;

    public TValue Value { get; }
    object? IValueHttpResult.Value => Value;
    public int StatusCode => StatusCodes.Status200OK;
    int? IStatusCodeHttpResult.StatusCode => StatusCode;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = StatusCode;
        return httpContext.Response.WriteAsJsonAsync(Value, Value?.GetType() ?? typeof(TValue), PublicApiJson.Options, httpContext.RequestAborted);
    }

    static void IEndpointMetadataProvider.PopulateMetadata(MethodInfo method, EndpointBuilder builder) =>
        builder.Metadata.Add(new ProducesResponseTypeMetadata(StatusCodes.Status200OK, typeof(TValue), ["application/json"]));
}

internal static class PublicApiResults
{
    public static PublicApiOk<TValue> Ok<TValue>(TValue value) => new(value);
}