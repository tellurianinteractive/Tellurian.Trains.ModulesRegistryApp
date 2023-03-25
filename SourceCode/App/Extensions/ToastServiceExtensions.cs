using Blazored.Toast.Services;
using Microsoft.Extensions.Localization;

namespace ModulesRegistry.Extensions;

public static class ToastServiceExtensions
{

    public static void ShowSuccessOrFailure(this IToastService toastService, IStringLocalizer localizer, int count, string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) message = string.Empty;
        if (count > 0)
            toastService.ShowSuccess(localizer[message]);
        else if (count < 0)
            toastService.ShowInfo(localizer[message]);
        else
            toastService.ShowError(localizer[message]);
    }

    public static void ShowNotFound<T>(this IToastService me, IStringLocalizer localizer) =>
        me.ShowError(localizer.NotFound<T>());
}
