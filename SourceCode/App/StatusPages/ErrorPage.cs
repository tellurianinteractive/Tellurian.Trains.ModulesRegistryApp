using System.Diagnostics;
using System.Net;

namespace ModulesRegistry.StatusPages;

/// <summary>
/// The page shown when a request fails: <c>StatusPages/error.html</c>, served by the exception handler with status 500.
/// </summary>
/// <remarks>
/// The exception handler middleware logs the exception before this runs. The page is a static file with all fully supported
/// languages, so showing it doesn't depend on Razor, localisation or content loading, any of which may be what failed.
/// </remarks>
public static class ErrorPage
{
    public const string FileName = "error.html";
    public const string RequestIdPlaceholder = "{{RequestId}}";

    private static readonly Lazy<string?> Template = new(() =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "StatusPages", FileName);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    });

    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseErrorPage() => app.UseExceptionHandler(errorApp => errorApp.Run(WriteAsync));
    }

    public static async Task WriteAsync(HttpContext context)
    {
        var requestId = Activity.Current?.Id ?? context.TraceIdentifier;
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.Headers.CacheControl = "no-store";
        if (Template.Value is null)
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync($"An unexpected error has occurred. Reference: {requestId}");
            return;
        }
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(Template.Value.Replace(RequestIdPlaceholder, WebUtility.HtmlEncode(requestId)));
    }
}