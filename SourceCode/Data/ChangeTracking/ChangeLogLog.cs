using Microsoft.Extensions.Logging;

namespace ModulesRegistry.Data.ChangeTracking;

/// <summary>
/// The change log's own errors, event ids 2000–2999 (Migration chapter I.1).
/// </summary>
public static partial class ChangeLogLog
{
    public const int CleanupFailedEventId = 2001;

    [LoggerMessage(EventId = CleanupFailedEventId, Level = LogLevel.Error, Message = "Deleting change log rows older than {MaxAgeDays} days failed")]
    public static partial void CleanupFailed(this ILogger logger, Exception exception, int maxAgeDays);
}