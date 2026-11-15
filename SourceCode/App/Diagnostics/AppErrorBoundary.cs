using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace ModulesRegistry.Diagnostics;

/// <summary>
/// Error boundary around the page in <c>MainLayout</c>: an exception in a page or its components is logged as Critical,
/// and the page area shows <see cref="ErrorBoundaryBase.ErrorContent"/> while the menu and the rest of the app keep working.
/// </summary>
/// <remarks>
/// Replaces the framework's logging of the exception (<see cref="IErrorBoundaryLogger"/>), so it is logged once.
/// Without the boundary the exception ended the circuit, which the framework logged as Error.
/// </remarks>
public sealed class AppErrorBoundary : ErrorBoundary
{
    [Inject] private ILogger<AppErrorBoundary> Logger { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    protected override Task OnErrorAsync(Exception exception)
    {
        Logger.UnhandledComponentException(exception, PagePath(Navigation));
        return Task.CompletedTask;
    }

    /// <summary>The page's path relative to the base address, without the query string.</summary>
    public static string PagePath(NavigationManager navigation)
    {
        var relative = navigation.ToBaseRelativePath(navigation.Uri);
        var end = relative.IndexOfAny(['?', '#']);
        return "/" + (end < 0 ? relative : relative[..end]);
    }
}