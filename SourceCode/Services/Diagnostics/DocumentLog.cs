using Microsoft.Extensions.Logging;

namespace ModulesRegistry.Services.Diagnostics;

/// <summary>
/// Document events, event ids 3000–3999 (Migration chapter I.1).
/// </summary>
/// <remarks>
/// Uploads refused for size are logged to learn what file sizes users need (or, as for SKP drawings, where the file
/// can be shrunk by purging unused objects). Only ids and sizes, no file names.
/// </remarks>
public static partial class DocumentLog
{
    public const int FileTooLargeEventId = 3001;

    [LoggerMessage(EventId = FileTooLargeEventId, Level = LogLevel.Warning, Message = "Upload of {DocumentKind} refused: {FileSizeKb} kB is over the limit of {MaxSizeKb} kB, user {UserId}, {ObjectType} {ObjectId}")]
    public static partial void FileTooLarge(this ILogger logger, string documentKind, long fileSizeKb, long maxSizeKb, int userId, string objectType, int objectId);
}