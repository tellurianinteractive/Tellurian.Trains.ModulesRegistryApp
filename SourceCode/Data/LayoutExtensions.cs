#nullable disable


using System;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Data
{
    public static class LayoutExtensions
    {
        public static string Name(this Layout? me) =>
            me is null ? string.Empty :
            me.PrimaryModuleStandard?.ShortName;

        public static bool RegistrationIsOpen([NotNullWhen(true)] this Layout? me, DateTimeOffset atTime) =>
            me is not null && atTime >= me.RegistrationOpeningDate && atTime < me.RegistrationClosingDate.AddDays(1);
    }
}
