using ModulesRegistry.Diagnostics;
using System.Text.RegularExpressions;

namespace ModulesRegistry.Extensions;

/// <summary>How a one-time message is shown: its colour and icon.</summary>
public enum FlashMessageLevel
{
    Error = 0,
    Success = 1,
    /// <summary>Something to pay attention to.</summary>
    Warning = 2,
    /// <summary>Neither success nor failure, such as nothing to save; light blue, as the interactive pages' information.</summary>
    Info = 3,
}

/// <summary>A message shown once on the next page: a resource key, and how it is shown.</summary>
public sealed record FlashMessageText(string TextKey, FlashMessageLevel Level)
{
    public FlashMessageText(string textKey, bool isSuccess) : this(textKey, isSuccess ? FlashMessageLevel.Success : FlashMessageLevel.Error) { }

    public bool IsSuccess => Level == FlashMessageLevel.Success;
}

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
        public void SetFlashMessage(string textKey, bool isSuccess) =>
            context.SetFlashMessage(textKey, isSuccess ? FlashMessageLevel.Success : FlashMessageLevel.Error);

        /// <summary>Sets the message for the next page; call it before the redirect.</summary>
        public void SetFlashMessage(string textKey, FlashMessageLevel level)
        {
            if (context is null || context.Response.HasStarted) return;
            // A failure shown after the redirect is logged (event 4002).
            if (level == FlashMessageLevel.Error) context.RequestServices?.GetService<ILoggerFactory>()?.CreateLogger(typeof(FlashMessageExtensions)).FailureAfterRedirect(context, textKey);
            var value = Format(new FlashMessageText(textKey, level));
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

    // "<level>:Key": "1:Key" for a success, "0:Key" for a failure, "2:Key" for a warning, "3:Key" for information.
    // Only a plain resource key is accepted.
    internal static string? Format(FlashMessageText message) =>
        ResourceKey().IsMatch(message.TextKey) && Enum.IsDefined(message.Level) ? $"{(int)message.Level}:{message.TextKey}" : null;

    internal static FlashMessageText? Parse(string? value) =>
        value is { Length: > 2 } && value[1] == ':' && value[0] is >= '0' and <= '3' && ResourceKey().IsMatch(value[2..]) ?
        new FlashMessageText(value[2..], (FlashMessageLevel)(value[0] - '0')) : null;

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9]{0,99}$")]
    private static partial Regex ResourceKey();
}