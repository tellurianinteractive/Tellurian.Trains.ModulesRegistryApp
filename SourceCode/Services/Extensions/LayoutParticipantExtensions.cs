namespace ModulesRegistry.Services.Extensions;

public static class LayoutParticipantExtensions
{
    public static string NameWithCityAndCountry(this LayoutParticipant? layoutParticipant) =>
        layoutParticipant is null || layoutParticipant.Person is null ? string.Empty :
        layoutParticipant.Person.NameWithCityAndCountry();

}
