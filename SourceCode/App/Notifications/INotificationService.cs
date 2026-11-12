namespace ModulesRegistry.Notifications;

/// <summary>
/// Short messages to the user, for example the result of a save. Shown by the <c>NotificationArea</c> component.
/// </summary>
/// <remarks>
/// The implementation is scoped, so each user (circuit) has its own notifications.
/// When pages move to static SSR (Migration.md stage 3), an implementation that keeps messages in TempData
/// across POST → redirect → GET can be added behind this interface.
/// </remarks>
public interface INotificationService
{
    /// <summary>
    /// Raised when a notification is added or removed.
    /// </summary>
    event Action? Changed;

    IReadOnlyList<Notification> Notifications { get; }

    void ShowInfo(string message);
    void ShowSuccess(string message);
    void ShowWarning(string message);
    void ShowError(string message);
    void Remove(Guid id);
    void Clear();
}