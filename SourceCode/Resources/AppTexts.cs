using System.Globalization;
using Tellurian.Localization.Implementatioms;

namespace ModulesRegistry.Resources;

/// <summary>
/// Looks up a text by its key, through Tellurian.Localization's RESX provider (Migration step 2B.3).
/// Use it where the key is only known at run time (enum names, waybill labels, keys stored in data);
/// use the generated <see cref="Strings"/> properties or <c>IStringLocalizer&lt;Strings&gt;</c> where the key is fixed.
/// </summary>
/// <remarks>
/// The culture defaults to <see cref="CultureInfo.CurrentUICulture"/>, as <see cref="System.Resources.ResourceManager"/> does.
/// .NET walks the culture's parents down to the neutral language (English), so a text is only missing if the key is unknown.
/// </remarks>
public static class AppTexts
{
    private static readonly ResxResourceProvider Provider = new(typeof(Strings));

    /// <summary>
    /// The text, or <c>null</c> if there is no text with that key (like <see cref="System.Resources.ResourceManager.GetString(string, CultureInfo)"/>).
    /// </summary>
    public static string? Find(string key, CultureInfo? culture = null)
    {
        var text = Provider.GetTranslation(key, culture);
        return text.IsFound ? text.Text : null;
    }

    /// <summary>
    /// The text, or the key itself if there is no text with that key.
    /// </summary>
    public static string Translated(string key, CultureInfo? culture = null) => Provider.GetTranslation(key, culture).Text;
}