using System.Text.RegularExpressions;

namespace ModulesRegistry.Extensions;

/// <summary>A message shown once on the next page: a resource key, and whether it tells a success or a failure.</summary>
public sealed record FlashMessageText(string TextKey, bool IsSuccess);

/// <summary>
/// One-time messages after a POST on a static SSR page (Migration step 3.3), as TempData in MVC: the POST sets a short-lived
/// cookie before its redirect, and <c>FlashMessage</c> in the layout shows the message on the next page and deletes the
/// cookie. The address stays clean, so a reload, a bookmark or a sort link doesn't show the message again.
/// </summary>
public static partial class FlashMessageExtensions
{
    public const string CookieName = "ModulesRegistry.Flash";

    extension(HttpContext? context)
    {
        /// <summary>Sets the message for the next page; call it before the redirect.</summary>
        public void SetFlashMessage(string textKey, bool isSuccess)
        {
            if (context is null || context.Response.HasStarted) return;
            var value = Format(new FlashMessageText(textKey, isSuccess));
            if (value is null) return;
            context.Response.Cookies.Append(CookieName, value, new CookieOptions
            {
                HttpOnly = true,
                Secure = context.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                IsEssential = true,
                MaxAge = TimeSpan.FromMinutes(1),
            });
        }

        /// <summary>The message set by the previous request, if any; the cookie is deleted so it is shown only once.</summary>
        public FlashMessageText? TakeFlashMessage()
        {
            if (context is null || !context.Request.Cookies.TryGetValue(CookieName, out var value)) return null;
            // A streamed page may have sent its headers; the cookie then expires by itself.
            if (!context.Response.HasStarted) context.Response.Cookies.Delete(CookieName);
            return Parse(value);
        }
    }

    // "1:Key" for a success, "0:Key" for a failure. Only a plain resource key is accepted.
    internal static string? Format(FlashMessageText message) =>
        ResourceKey().IsMatch(message.TextKey) ? $"{(message.IsSuccess ? '1' : '0')}:{message.TextKey}" : null;

    internal static FlashMessageText? Parse(string? value) =>
        value is { Length: > 2 } && value[1] == ':' && (value[0] == '0' || value[0] == '1') && ResourceKey().IsMatch(value[2..]) ?
        new FlashMessageText(value[2..], value[0] == '1') : null;

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9]{0,99}$")]
    private static partial Regex ResourceKey();
}