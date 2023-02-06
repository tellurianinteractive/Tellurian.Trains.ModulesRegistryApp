using System.Data;
using System.Diagnostics;
using System.Linq;

namespace ModulesRegistry.Services.Extensions;
public static class AvaliableModuleExtensions
{
    public static IEnumerable<ModulePackage> AsModulePackages(this IEnumerable<AvailableModule> modules)
    {
        if (modules is null) return Array.Empty<ModulePackage>();
        var packages = new Dictionary<string, List<(AvailableModule Module, int ScaleId, ModulePackageType type)>>(modules.Count());
        var scales = modules.GroupBy(m => m.ScaleId);
        foreach (var scale in scales)
        {
            foreach (var module in scale.OrderBy(m => m.FullName))
            {
                if (module.PackageLabel.HasValue())
                {
                    var key = module.PackageLabel;
                    if (!packages.ContainsKey(key)) packages.Add(key, new List<(AvailableModule, int, ModulePackageType)>());
                    packages[key].Add((module, scale.Key, ModulePackageType.Package));
                }
                else if (module.ConfigurationLabel.HasValue()) { 
                    var key = module.FullName;
                    if (!packages.ContainsKey(key)) packages.Add(key, new List<(AvailableModule, int, ModulePackageType)>());
                    packages[key].Add((module, scale.Key, ModulePackageType.Variants));

                }
                else
                {
                    try
                    {
                        packages.Add(module.FullName, new List<(AvailableModule, int, ModulePackageType)>() { (module, scale.Key, ModulePackageType.SingleModule) });
                    }
                    catch (Exception)
                    {
                        Debugger.Break();
                        throw;
                    }
                }
            }
        }
        return packages.Select((p, i) =>
    new ModulePackage(i, PackageType(p.Value), PackageName(p.Value), OwnerName(p.Value), OwnersPersonId(p.Value), p.Value.Select(v => v.Module).AsEnumerable()) { ScaleId = p.Value.First().ScaleId });

        static ModulePackageType PackageType(IEnumerable<(AvailableModule Module, int Scale, ModulePackageType Type)> items) => items.First().Type;
        static string PackageName(IEnumerable<(AvailableModule Module, int Scale, ModulePackageType Type)> items) =>
           PackageType(items) switch
           {
               ModulePackageType.Package => items.First().Module.PackageLabel ?? "Package?",
               _ => items.First().Module.FullName
           };
        static string OwnerName(IEnumerable<(AvailableModule Module, int Scale, ModulePackageType Type)> items) =>
            items.First().Module.OwnerName();

        static int[] OwnersPersonId(IEnumerable<(AvailableModule Module, int Scale, ModulePackageType Type)> items) =>
            new[] { items.First().Module.OwnerPersonId ?? 0};

    }

    internal static string OwnerName(this AvailableModule module) =>
        module.OwnerGroupId > 0 ? module.OwnerGroupName ?? string.Empty :
        module.OwnerPersonId > 0 ? $"{module.OwnerFirstName} {module.OwnerLastName}" ?? string.Empty : 
        string.Empty;

    public static AvailableModule MapAvailableModule(this IDataRecord record) =>
        new()
        {
            ModuleId = record.GetInt("ModuleId"),
            FullName = record.GetString("ModuleName"),
            ScaleId = record.GetInt("ScaleId"),
            StandardId = record.GetInt("StandardId"),
            FunctionalState = record.GetInt("FunctionalState"),
            Is2R = record.GetBool("Is2R"),
            Is3R = record.GetBool("Is3R"),
            LandscapeSeason = record.GetInt("LandscapeSeason"),
            PackageLabel = record.GetString("PackageLabel"),
            ConfigurationLabel = record.GetString("ConfigurationLabel"),
            FremoNumber = record.GetInt("FremoNumber"),
            StationId = record.GetInt("StationId"),
            //OwnedShare = record.GetInt("OwnedShare"),
            OwnerPersonId = record.GetInt("OwnerPersonId"),
            OwnerFirstName = record.GetString("OwnerFirstName"),
            OwnerLastName = record.GetString("OwnerLastName"),
            OwnerGroupId = record.GetInt("OwnerGroupId"),
            OwnerGroupName = record.GetString("OwnerGroupName"),
            PersonId = record.GetInt("PersonId"),
            FirstName = record.GetString("FirstName"),
            LastName = record.GetString("LastName"),
            FremoOwnerSignature = record.GetString("FremoOwnerSignature"),
            CityName = record.GetString(columnName: "CityName"),
            CountryId = record.GetInt("CountryId"),
            UserId = record.GetInt("UserId")
        };
}
