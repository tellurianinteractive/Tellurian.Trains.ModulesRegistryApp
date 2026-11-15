using Microsoft.Extensions.Logging;

namespace ModulesRegistry.Services.Diagnostics;

/// <summary>
/// Security events, event ids 1000–1999 (Migration step 2C.4, section 9.1): successful sign-in is Information, everything
/// else Warning.
/// </summary>
/// <remarks>
/// Only user ids are logged, never user names, e-mail addresses or passwords. A sign-in or password reset for an address
/// that has no user is logged without any id, and without the address.
/// </remarks>
public static partial class SecurityLog
{
    public const int SignedInEventId = 1001;
    public const int SignInFailedEventId = 1002;
    public const int SignInFailedUnknownUserEventId = 1003;
    public const int SignedOutEventId = 1004;
    public const int LockedOutEventId = 1005;
    public const int PasswordResetRequestedEventId = 1006;
    public const int PasswordResetUnknownUserEventId = 1007;
    public const int DocumentChangeDeniedEventId = 1008;

    [LoggerMessage(EventId = SignedInEventId, Level = LogLevel.Information, Message = "User {UserId} signed in")]
    public static partial void SignedIn(this ILogger logger, int userId);

    [LoggerMessage(EventId = SignInFailedEventId, Level = LogLevel.Warning, Message = "Sign-in failed for user {UserId}: {Reason}")]
    public static partial void SignInFailed(this ILogger logger, int userId, SignInFailure reason);

    [LoggerMessage(EventId = SignInFailedUnknownUserEventId, Level = LogLevel.Warning, Message = "Sign-in failed for an address that has no user")]
    public static partial void SignInFailedUnknownUser(this ILogger logger);

    [LoggerMessage(EventId = SignedOutEventId, Level = LogLevel.Warning, Message = "User {UserId} signed out")]
    public static partial void SignedOut(this ILogger logger, int userId);

    [LoggerMessage(EventId = LockedOutEventId, Level = LogLevel.Warning, Message = "User {UserId} is locked out after {FailedAttempts} failed sign-ins")]
    public static partial void LockedOut(this ILogger logger, int userId, int failedAttempts);

    [LoggerMessage(EventId = PasswordResetRequestedEventId, Level = LogLevel.Warning, Message = "Password reset requested for user {UserId}, request {Attempt}, limit reached: {LimitReached}")]
    public static partial void PasswordResetRequested(this ILogger logger, int userId, int attempt, bool limitReached);

    [LoggerMessage(EventId = PasswordResetUnknownUserEventId, Level = LogLevel.Warning, Message = "Password reset requested for an address that has no user")]
    public static partial void PasswordResetUnknownUser(this ILogger logger);

    [LoggerMessage(EventId = DocumentChangeDeniedEventId, Level = LogLevel.Warning, Message = "User {UserId} was refused to {Operation} a document of {ObjectType} {ObjectId}")]
    public static partial void DocumentChangeDenied(this ILogger logger, int userId, string operation, string objectType, int objectId);
}

public enum SignInFailure
{
    WrongPassword,
    AlreadyLockedOut,
    NoPerson,
}