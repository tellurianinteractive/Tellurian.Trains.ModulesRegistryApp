using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Specialized;
using System.Web;

namespace ModulesRegistry.Extensions;

public static class NavigationManagerExtensions
{
    public static bool TryGetQueryStringValue<T>(this NavigationManager navigator, string key, out T value)
    {
        var uri = navigator.ToAbsoluteUri(navigator.Uri);
        if (QueryHelpers.ParseQuery(uri.Query).TryGetValue(key, out var valueFromQueryString))
        {
            if (typeof(T) == typeof(int) && int.TryParse(valueFromQueryString, out var valueAsInt))
            {
                value = (T)(object)valueAsInt;
                return true;
            }

            if (typeof(T) == typeof(string))
            {
                value = (T)(object)valueFromQueryString.ToString();
                return true;
            }

            if (typeof(T) == typeof(decimal) && decimal.TryParse(valueFromQueryString, out var valueAsDecimal))
            {
                value = (T)(object)valueAsDecimal;
                return true;
            }
        }
        value = default!;
        return false;
    }

    public static NameValueCollection QueryString(this NavigationManager navigationManager)
    {
        return HttpUtility.ParseQueryString(new Uri(navigationManager.Uri.ToLowerInvariant()).Query);
    }

    // get single querystring value with specified key
    public static string? QueryString(this NavigationManager navigationManager, string key)
    {
        return navigationManager.QueryString()[key.ToLowerInvariant()];
    }
}


