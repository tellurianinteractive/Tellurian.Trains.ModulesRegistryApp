using Microsoft.AspNetCore.Components;
using ModulesRegistry.Services.Implementations;
using System.Diagnostics.CodeAnalysis;

namespace ModulesRegistry.Services.Extensions;
static public class ModuleExtensions
{
    public static string OwnerNames(this Module? it) =>
        it is null || it.ModuleOwnerships is null ? "?" :
        it.ModuleOwnerships.OwnerNames();

    public static bool IsStation(this Module? it) =>
        it is not null && it.StationId > 0;

    public static string StationName(this Module? it) =>
        it is null || it.Station is null ? string.Empty :
        it.Station.FullName;

    public static bool IsGroupOwned([NotNullWhen(true)] this Module? it) =>
        it is not null && it.ModuleOwnerships is not null && it.ModuleOwnerships.Any(mo => mo.GroupId > 0);
    public static bool IsPersonOwned([NotNullWhen(true)] this Module? it) =>
        it is not null && it.ModuleOwnerships is not null && it.ModuleOwnerships.Any(mo => mo.PersonId > 0);

    public static int OwningGroupId(this Module? it) =>
        it.IsGroupOwned() ? it.ModuleOwnerships.First(mo => mo.GroupId > 0).GroupId ?? 0 : 0;
    public static int[] OwningPersonsIds(this Module? it) =>
        it.IsPersonOwned() ? it.ModuleOwnerships.Where(mo => mo.PersonId > 0).Select(mo => mo.PersonId!.Value).ToArray() : Array.Empty<int>();
    public static MarkupString Name(this Module? it) =>
        it is null ? new(""):
        new(
            it.ConfigurationLabel.HasValue() ? $"{it.FullName} <span class=\"fa fa-ruler\" /> {it.ConfigurationLabel}" :
            it.PackageLabel.HasValue() ? $"{it.FullName} <span class=\"fa fa-truck-loading\" /> {it.PackageLabel}" :
            it.FullName);
    public static MarkupString StatusIcon(this Module? it) => new($"<span title=\"{it.StatusTitle()}\" class=\"{it.StatusSymbol()}\"/>");
    private static string StatusTitle(this Module? it) => 
        it is null ? string.Empty :
        LanguageUtility.GetLocalizedString(it.Status().ToString());

    private static string StatusSymbol(this Module? it) => it.Status() switch
    {
        ModuleFunctionalState.ReadyToTest => "fa fa-exclamation-circle",
        ModuleFunctionalState.Approved => "fa fa-check-circle",
        ModuleFunctionalState.Tested => "fa fa-check-circle",
        ModuleFunctionalState.Planned => "fa fa-times-circle",
        ModuleFunctionalState.UnderConstruction => "fa fa-tools",
        ModuleFunctionalState.UnderRepair => "fa fa-tools",
        _ => "fa fa-question-circle"
    };
    private static ModuleFunctionalState Status(this Module? it) =>
         it is null ? ModuleFunctionalState.Unknown :
         (ModuleFunctionalState)(it.FunctionalState);
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
