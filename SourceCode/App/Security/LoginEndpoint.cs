using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ModulesRegistry.Security;

/// <summary>
/// Sign-in: <c>POST /login</c> with a form (<see cref="LoginForm"/>) and the antiforgery token, posted by <c>LoginControl</c>.
/// </summary>
/// <remarks>
/// Before release 1.14.4 the login was a link, <c>GET /login?username=...&amp;password=...</c>, so passwords ended up in
/// browser history and request logs. A GET now only redirects to the start page and never signs in, so old links and
/// bookmarks don't work as credentials anymore.
/// </remarks>
public static class LoginEndpoint
{
    public const string Route = "/login";

    extension(IEndpointRouteBuilder endpoints)
    {
        public void MapLogin()
        {
            endpoints.MapPost(Route, LoginAsync).ExcludeFromDescription();
            endpoints.MapGet(Route, (HttpContext context) => TypedResults.LocalRedirect(RootUrl(context.Request.PathBase))).ExcludeFromDescription();
        }
    }

    private static async Task<RedirectHttpResult> LoginAsync([FromForm] LoginForm form, HttpContext context, ILoginHandler handler)
    {
        var pathBase = context.Request.PathBase;
        if (string.IsNullOrWhiteSpace(form.Username) || string.IsNullOrWhiteSpace(form.Password))
            return TypedResults.LocalRedirect(RootUrl(pathBase));

        await handler.SignInAsync(context, form.Username, form.Password);
        return TypedResults.LocalRedirect(ReturnUrlOrRoot(pathBase, form.ReturnUrl));
    }

    /// <summary>
    /// The local URL to return to after sign-in. <paramref name="returnUrl"/> is usually relative to the app's base path
    /// (<c>Meetings/5</c>); anything that isn't a local URL afterwards (<c>//host</c>, <c>/\host</c>) gives the start page.
    /// </summary>
    public static string ReturnUrlOrRoot(PathString pathBase, string? returnUrl)
    {
        var root = RootUrl(pathBase);
        if (string.IsNullOrWhiteSpace(returnUrl)) return root;
        var url = returnUrl.StartsWith(root, StringComparison.Ordinal) ? returnUrl : root + returnUrl;
        return IsLocalUrl(url) ? url : root;
    }

    private static string RootUrl(PathString pathBase) => $"{pathBase}/";

    private static bool IsLocalUrl(string url) =>
        url.StartsWith('/') && (url.Length == 1 || (url[1] != '/' && url[1] != '\\')) && !url.Any(char.IsControl);
}

public sealed class LoginForm
{
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? ReturnUrl { get; set; }
}

/// <summary>
/// The antiforgery token for forms posted from interactive components to endpoints. Created in <c>_Host.cshtml</c>
/// (which has the <see cref="HttpContext"/>) and cascaded from <c>App</c>.
/// </summary>
public sealed record AntiforgeryFormToken(string FieldName, string Value);