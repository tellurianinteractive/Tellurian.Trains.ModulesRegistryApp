namespace ModulesRegistry.Services;

public class LanguageLabel
{
    public string ResourceKey { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}

public class LanguageLabels
{
    public string LanguageCode { get; set; } = string.Empty;
    public ICollection<LanguageLabel> Labels { get; set; } = Array.Empty<LanguageLabel>();
}

public static class LanguageLabelsExtensions
{
    public static IDictionary<string, string> Texts(this LanguageLabels me) => me.Labels.ToDictionary(v => v.ResourceKey, v => v.Text);
    public static LanguageLabels CreateLabels(this string languageCode, IEnumerable<string> resourceKeys)
    {
        var culture = new CultureInfo(languageCode);
        var resourceManager = Resources.Strings.ResourceManager;
        return new LanguageLabels
        {
            LanguageCode = languageCode,
            Labels = resourceKeys.Select(k => new LanguageLabel { ResourceKey = k, Text = resourceManager.GetString(k, culture) ?? k }).ToArray()
        };
    }
    public static string GetLabelText(this IEnumerable<LanguageLabels> me, string resourceKey, string languageCode) =>
        me.SingleOrDefault(l => l.LanguageCode == languageCode)?.GetLabelText(resourceKey) ?? resourceKey;

    private static string GetLabelText(this LanguageLabels me, string resourceKey) =>
        me.Labels.SingleOrDefault(l => l.ResourceKey == resourceKey)?.Text ?? resourceKey;
}
