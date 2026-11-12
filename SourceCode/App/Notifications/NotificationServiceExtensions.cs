using Microsoft.Extensions.Localization;
using ModulesRegistry.Extensions;

namespace ModulesRegistry.Notifications;

public static class NotificationServiceExtensions
{
    extension(INotificationService notificationService)
    {
        /// <summary>
        /// Shows the result of an operation: success if <paramref name="count"/> is positive,
        /// info if it is negative (nothing to do), otherwise error.
        /// </summary>
        public void ShowSuccessOrFailure(IStringLocalizer localizer, int count, string? message)
        {
            var text = localizer[message ?? string.Empty];
            if (count > 0)
                notificationService.ShowSuccess(text);
            else if (count < 0)
                notificationService.ShowInfo(text);
            else
                notificationService.ShowError(text);
        }

        public void ShowNotFound<T>(IStringLocalizer localizer) =>
            notificationService.ShowError(localizer.NotFound<T>());
    }
}