namespace ModulesRegistry.Services.Extensions;
public static class PersonExtensions
{
    public static string NameCityAndCountry(this Person me) =>
    me.Country is null ? $"{me.Name()}, {me.CityName}" :
    $"{me.Name()}, {me.CityName}, {me.Country.EnglishName.Localized()}";
}
