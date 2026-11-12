namespace ModulesRegistry.Notifications;

/// <summary>
/// Keeps the current user's notifications and removes each one after <see cref="DisplayTime"/>.
/// </summary>
public sealed class NotificationService(TimeProvider timeProvider) : INotificationService, IDisposable
{
    public static readonly TimeSpan DisplayTime = TimeSpan.FromSeconds(2);

    private readonly Lock _lock = new();
    private readonly List<Notification> _notifications = [];
    private readonly Dictionary<Guid, ITimer> _timers = [];

    public event Action? Changed;

    public IReadOnlyList<Notification> Notifications
    {
        get
        {
            lock (_lock) return [.. _notifications];
        }
    }

    public void ShowInfo(string message) => Show(NotificationLevel.Info, message);
    public void ShowSuccess(string message) => Show(NotificationLevel.Success, message);
    public void ShowWarning(string message) => Show(NotificationLevel.Warning, message);
    public void ShowError(string message) => Show(NotificationLevel.Error, message);

    public void Remove(Guid id)
    {
        bool removed;
        lock (_lock)
        {
            removed = _notifications.RemoveAll(notification => notification.Id == id) > 0;
            if (_timers.Remove(id, out var timer)) timer.Dispose();
        }
        if (removed) Changed?.Invoke();
    }

    public void Clear()
    {
        bool removed;
        lock (_lock)
        {
            removed = _notifications.Count > 0;
            _notifications.Clear();
            DisposeTimers();
        }
        if (removed) Changed?.Invoke();
    }

    public void Dispose()
    {
        lock (_lock) DisposeTimers();
    }

    private void Show(NotificationLevel level, string message)
    {
        var notification = new Notification(Guid.NewGuid(), level, message);
        lock (_lock)
        {
            _notifications.Add(notification);
            _timers[notification.Id] = timeProvider.CreateTimer(_ => Remove(notification.Id), null, DisplayTime, Timeout.InfiniteTimeSpan);
        }
        Changed?.Invoke();
    }

    private void DisposeTimers()
    {
        foreach (var timer in _timers.Values) timer.Dispose();
        _timers.Clear();
    }
}