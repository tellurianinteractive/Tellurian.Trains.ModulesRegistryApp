
namespace ModulesRegistry.Data;

public static class LayoutStationExtensions
{
    public static string NameInLayout(this LayoutStation? me) =>
        me is null ? string.Empty :
        me.OtherName ?? me.Station?.FullName ?? $"Name missing for {me.Id}";

    public static string SignatireInLayout(this LayoutStation? me) =>
         me is null ? string.Empty :
         me.OtherSignature ?? me.Station?.Signature ?? $"Signature missing for {me.Id}";

    public static string CountryEnglishName(this LayoutStation? me) =>
        me is null ? string.Empty :
        me.OtherCountry?.EnglishName ?? me.Station?.Region?.Country?.EnglishName ?? string.Empty;

}

