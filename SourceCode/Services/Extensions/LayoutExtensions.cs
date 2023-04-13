namespace ModulesRegistry.Services.Extensions;

public static class LayoutExtensions
{
    public static string RegistrationOpensDate(this Layout layout) => layout.RegistrationOpeningDate.ToShortDateString();
    public static string RegistrationClosesDate(this Layout layout) => layout.RegistrationClosingDate.ToShortDateString();
    public static string RegistrationOfModulesClosesDate(this Layout layout) => (layout.ModuleRegistrationClosingDate ?? layout.RegistrationClosingDate).ToShortDateString();
    internal static bool IsOpenForRegistration(this Layout layout, DateTime at) =>
        layout.IsRegistrationPermitted &&
        layout.RegistrationOpeningDate <= at &&
        layout.RegistrationClosingDate >= at;

    internal static bool IsNotYetOpenForRegistration(this Layout layout, DateTime at) =>
        layout.IsRegistrationPermitted &&
        layout.RegistrationOpeningDate > at;

}

