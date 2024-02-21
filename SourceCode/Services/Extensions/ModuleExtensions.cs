using Microsoft.AspNetCore.Components;
using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
static public class ModuleExtensions
{
    public static string OwnerNames(this Module? module) =>
        module is null || module.ModuleOwnerships is null ? "?" :
        module.ModuleOwnerships.OwnerNames();

    public static bool IsStation(this Module? module) =>
        module is not null && module.StationId > 0;

    public static string StationName(this Module? module) =>
        module is null || module.Station is null ? string.Empty :
        module.Station.FullName;

    public static bool IsGroupOwned([NotNullWhen(true)] this Module? module) =>
        module is not null && module.ModuleOwnerships is not null && module.ModuleOwnerships.Any(mo => mo.GroupId > 0);
    public static bool IsPersonOwned([NotNullWhen(true)] this Module? module) =>
        module is not null && module.ModuleOwnerships is not null && module.ModuleOwnerships.Any(mo => mo.PersonId > 0);

    public static int OwningGroupId(this Module? module) =>
        module.IsGroupOwned() ? module.ModuleOwnerships.First(mo => mo.GroupId > 0).GroupId ?? 0 : 0;
    public static int[] OwningPersonsIds(this Module? module) =>
        module.IsPersonOwned() ? module.ModuleOwnerships.Where(mo => mo.PersonId > 0).Select(mo => mo.PersonId!.Value).ToArray() : [];
    public static MarkupString Name(this Module? module) =>
        module is null ? new(""):
        new(
            module.ConfigurationLabel.HasValue() ? $"{module.FullName} <span class=\"fa fa-ruler\" /> {module.ConfigurationLabel}" :
            module.PackageLabel.HasValue() ? $"{module.FullName} <span class=\"fa fa-truck-loading\" /> {module.PackageLabel}" :
            module.FullName);
    public static MarkupString StatusIcon(this Module? module) => new($"<span title=\"{module.StatusTitle()}\" class=\"{module.StatusSymbol()}\"/>");
    private static string StatusTitle(this Module? module) => 
        module is null ? string.Empty :
        LanguageExtensions.GetLocalizedString(module.Status().ToString());

    private static string StatusSymbol(this Module? module) => module.Status() switch
    {
        ModuleFunctionalState.ReadyToTest => "fa fa-exclamation-circle",
        ModuleFunctionalState.Approved => "fa fa-check-circle",
        ModuleFunctionalState.Tested => "fa fa-check-circle",
        ModuleFunctionalState.Planned => "fa fa-times-circle",
        ModuleFunctionalState.UnderConstruction => "fa fa-tools",
        ModuleFunctionalState.UnderRepair => "fa fa-tools",
        _ => "fa fa-question-circle"
    };
    private static ModuleFunctionalState Status(this Module? module) =>
         module is null ? ModuleFunctionalState.Unknown :
         (ModuleFunctionalState)(module.FunctionalState);
}

public enum ModuleStatus
{
    Unknown = 0,
    Planned = 1,
    Unavailable = 2,
    Untested = 3,
    Incomplete = 4,
    Available = 5,
}
