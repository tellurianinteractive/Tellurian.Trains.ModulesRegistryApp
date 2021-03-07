using Blazored.Toast.Services;
using Microsoft.Extensions.Localization;

namespace ModulesRegistry.Extensions
{
    public static class ToastServiceExtensions
    {

       public static void ShowSuccessOrFailure(this IToastService me, IStringLocalizer localizer, int count, string message)
        {
            if (count > 0)
                me.ShowSuccess(localizer[message], localizer["Success"].ToString());
            else if (count < 0)
                me.ShowInfo(localizer[message], localizer["Info"].ToString());
            else
                me.ShowError(localizer[message], localizer["Fault"].ToString());
        }     
    }
}
