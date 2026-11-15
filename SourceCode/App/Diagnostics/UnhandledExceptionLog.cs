namespace ModulesRegistry.Diagnostics;

/// <summary>
/// Log entries for unhandled exceptions, all <see cref="LogLevel.Critical"/> ("Fatal"), event ids 9000–9999
/// (Migration step 2C.2, section 9.1). Each path logs its exception once.
/// </summary>
/// <remarks>
/// Only the path is logged, never the query string: it can hold an API key.
/// </remarks>
public static partial class UnhandledExceptionLog
{
    public const int RequestEventId = 9001;
    public const int ComponentEventId = 9002;
    public const int ProcessEventId = 9003;
    public const int UnobservedTaskEventId = 9004;

    [LoggerMessage(EventId = RequestEventId, Level = LogLevel.Critical, Message = "Unhandled exception in request {Method} {Path}")]
    public static partial void UnhandledRequestException(this ILogger logger, Exception exception, string method, string path);

    [LoggerMessage(EventId = ComponentEventId, Level = LogLevel.Critical, Message = "Unhandled exception on page {Page}")]
    public static partial void UnhandledComponentException(this ILogger logger, Exception exception, string page);

    [LoggerMessage(EventId = ProcessEventId, Level = LogLevel.Critical, Message = "Unhandled exception in the process, terminating: {IsTerminating}")]
    public static partial void UnhandledProcessException(this ILogger logger, Exception exception, bool isTerminating);

    [LoggerMessage(EventId = UnobservedTaskEventId, Level = LogLevel.Critical, Message = "Unobserved exception in a background task")]
    public static partial void UnobservedTaskException(this ILogger logger, Exception exception);
}