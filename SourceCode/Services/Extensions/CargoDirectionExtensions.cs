namespace ModulesRegistry.Services.Extensions;

public static class CargoDirectionExtensions
{
    public static string ShortNameLocalized(this CargoDirection? direction) =>
         direction is null ? string.Empty :
         direction.ShortName.Localized();

    public static string LongNameLocalized(this CargoDirection? direction) =>
         direction is null ? string.Empty :
         direction.FullName.Localized();
}
