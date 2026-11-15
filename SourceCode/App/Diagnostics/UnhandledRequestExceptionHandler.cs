using Microsoft.AspNetCore.Diagnostics;
using ModulesRegistry.StatusPages;

namespace ModulesRegistry.Diagnostics;

/// <summary>
/// Logs an exception that no endpoint handled as Critical and shows the error page (<c>StatusPages/error.html</c>).
/// </summary>
/// <remarks>
/// Since it returns true, the exception handler middleware doesn't write its own Error entry (.NET 10 and later),
/// so the exception is logged once. Used outside Development, through <see cref="ErrorPage"/>.
/// </remarks>
public sealed class UnhandledRequestExceptionHandler(ILogger<UnhandledRequestExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.UnhandledRequestException(exception, httpContext.Request.Method, httpContext.Request.Path.Value ?? "/");
        await ErrorPage.WriteAsync(httpContext);
        return true;
    }
}