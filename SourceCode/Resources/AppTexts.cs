using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Globalization;
using System.Reflection;
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

    /// <summary>
    /// The key of a text, or <c>null</c> if it isn't one of the texts in the culture, such as a text with values filled in.
    /// A key is returned as it is. Used to log which message a user saw without logging the text, which may hold personal data.
    /// </summary>
    public static string? KeyOf(string? text, CultureInfo? culture = null)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        culture ??= CultureInfo.CurrentUICulture;
        if (Find(text, culture) is not null) return text;
        return KeysByText.GetOrAdd(culture.Name, _ => TextKeys(culture)).GetValueOrDefault(text);
    }

    private static readonly ConcurrentDictionary<string, FrozenDictionary<string, string>> KeysByText = new();

    // The generated class has one string property per key.
    private static FrozenDictionary<string, string> TextKeys(CultureInfo culture)
    {
        var keys = new Dictionary<string, string>();
        foreach (var property in typeof(Strings).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(p => p.PropertyType == typeof(string)))
        {
            if (Find(property.Name, culture) is { Length: > 0 } text) keys.TryAdd(text, property.Name);
        }
        return keys.ToFrozenDictionary();
    }
}