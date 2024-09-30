namespace ModulesRegistry.Services.Extensions;
public static class PersonExtensions
{
    public static string NameWithCityAndCountry(this Person me) =>
        me is null ? string.Empty :
        me.Country is null ? $"{me.Name()}, {me.CityName}" :
        $"{me.Name()}, {me.CityName}, {me.Country.EnglishName.Localized()}";
}
