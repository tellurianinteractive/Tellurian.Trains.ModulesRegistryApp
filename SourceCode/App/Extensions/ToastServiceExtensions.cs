using Blazored.Toast.Services;
using Microsoft.Extensions.Localization;

namespace ModulesRegistry.Extensions
{
    public static class ToastServiceExtensions
    {

        public static void ShowSuccessOrFailure(this IToastService me, IStringLocalizer localizer, bool isSuccess, string message)
        {
            if (isSuccess)
                me.ShowSuccess(localizer[message], localizer["Success"].ToString());
            else
                me.ShowError(localizer[message], localizer["Failure"].ToString());
        }
    }
}
