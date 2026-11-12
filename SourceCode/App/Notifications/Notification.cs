namespace ModulesRegistry.Notifications;

public sealed record Notification(Guid Id, NotificationLevel Level, string Message);