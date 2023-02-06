using Microsoft.EntityFrameworkCore.Query;
using ModulesRegistry.Data.Extensions;
using System.Diagnostics;

namespace ModulesRegistry.Data;

public static class ModulePackageExtensions
{
    public static string ContaningModules(this ModulePackage me) => string.Join(',', me.Modules.Select(m => m.FullName));
    //public static IEnumerable<ModulePackage> AsPackages(this IEnumerable<Module>? modules)
    //{
    //    if (modules is null) return Array.Empty<ModulePackage>();
    //    var packages = new Dictionary<string, List<(Module Module, int ScaleId, ModulePackageType type)>>(modules.Count());
    //    var scales = modules.GroupBy(m => m.Standard.ScaleId);
    //    foreach (var scale in scales)
    //    {
    //        foreach (var module in scale.OrderBy(m => m.FullName))
    //        {
    //            if (module.PackageLabel.HasValue())
    //            {
    //                var packageKey = module.PackageLabel;
    //                if (packageKey.HasValue())
    //                {
    //                    if (!packages.ContainsKey(packageKey)) packages.Add(packageKey, new List<(Module, int, ModulePackageType)>());
    //                    packages[packageKey].Add((module, scale.Key, ModulePackageType.Package));
    //                }
    //            }
    //            else if (module.ConfigurationLabel.HasValue())
    //            {
    //                var packageKey = module.FullName;
    //                if (!packages.ContainsKey(packageKey)) packages.Add(packageKey, new List<(Module, int, ModulePackageType)>());
    //                packages[packageKey].Add((module, scale.Key, ModulePackageType.Variants));
    //            }
    //            else
    //            {
    //                try
    //                {
    //                packages.Add(module.FullName, new List<(Module, int, ModulePackageType)>() { (module, scale.Key, ModulePackageType.SingleModule) });

    //                }
    //                catch (Exception)
    //                {
    //                    Debugger.Break();
    //                    throw;
    //                }
    //            }
    //        }
    //    }
    //    return packages.Select((p, i) => 
    //        new ModulePackage(i, PackageType(p.Value), PackageName(p.Value), OwnerName(p.Value), OwnersPersonId(p.Value),  p.Value.Select(v => v.Module).AsEnumerable()) { ScaleId = p.Value.First().ScaleId });

    //    static ModulePackageType PackageType(IEnumerable<(Module Module, int Scale, ModulePackageType Type)> items) => items.First().Type;
    //    static string PackageName(IEnumerable<(Module Module, int Scale, ModulePackageType Type)> items) =>
    //       PackageType(items) switch
    //       {
    //           ModulePackageType.Package => items.First().Module.PackageLabel,
    //           _ => items.First().Module.FullName
    //       };
    //    static string OwnerName(IEnumerable<(Module Module, int Scale, ModulePackageType Type)> items) =>
    //        string.Join(", ", items.First().Module.ModuleOwnerships.OwnerNames());

    //    static int[] OwnersPersonId(IEnumerable<(Module Module, int Scale, ModulePackageType Type)> items) =>
    //        items.First().Module.ModuleOwnerships.Where(mo => mo.PersonId > 0).Select(mo => mo.PersonId!.Value).ToArray();

    //}
    public static string ModuleNames(this ModulePackage it) =>
              it.PackageType switch
              {
                  ModulePackageType.Variants => string.Join(", ", it.Modules.Select(i => i.ConfigurationLabel)),
                  _ => string.Join(", ", it.Modules.Select(m => m.FullName))
              };


}
