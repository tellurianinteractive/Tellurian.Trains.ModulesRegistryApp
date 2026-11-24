using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Resources;

namespace ModulesRegistry.Diagnostics;

/// <summary>
/// Failures shown to a user, all <see cref="LogLevel.Warning"/>, event ids 4000–4999: something the user tried didn't succeed,
/// and the page said so. Only the message's resource key, the path and the user id are logged, never the text, which may hold
/// names or addresses filled in.
/// </summary>
public static partial class FailureLog
{
    public const int PageMessageEventId = 4001;
    public const int FlashMessageEventId = 4002;

    /// <summary>The key logged for a text that isn't one of the app's texts, such as one with values filled in.</summary>
    public const string UnknownText = nameof(UnknownText);

    [LoggerMessage(EventId = PageMessageEventId, Level = LogLevel.Warning, Message = "Failure shown on page {Path}: {MessageKey}, user {UserId}")]
    public static partial void FailureShown(this ILogger logger, string path, string messageKey, int userId);

    [LoggerMessage(EventId = FlashMessageEventId, Level = LogLevel.Warning, Message = "Failure shown after {Method} {Path}: {MessageKey}, user {UserId}")]
    public static partial void FailureAfterRedirect(this ILogger logger, string method, string path, string messageKey, int userId);

    extension(ILogger logger)
    {
        /// <summary>Logs a failure message shown on the page of the current request.</summary>
        public void FailureShown(HttpContext? context, string? message) =>
            logger.FailureShown(context?.Request.Path.Value ?? string.Empty, MessageKey(message), context?.User.UserId() ?? 0);

        /// <summary>Logs a failure message set for the page after the redirect.</summary>
        public void FailureAfterRedirect(HttpContext context, string message) =>
            logger.FailureAfterRedirect(context.Request.Method, context.Request.Path.Value ?? string.Empty, MessageKey(message), context.User.UserId());
    }

    internal static string MessageKey(string? message) => AppTexts.KeyOf(message) ?? UnknownText;
}