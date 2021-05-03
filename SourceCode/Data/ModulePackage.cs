using ModulesRegistry.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry.Data
{
    /// <summary>
    /// This is not an entity that is stored in the database 
    /// </summary>
    public class ModulePackage
    {
        public ModulePackage(int id, string name, IEnumerable<Module> modules)
        {
            Id = id;
            Name = name;
            Modules = modules;
        }
        public int Id { get; }
        public string Name { get; }
        public IEnumerable<Module> Modules { get; }
        public int ScaleId { get; init; }
        public int LayoutId { get; set; }
        public override string ToString() => Modules.Count() == 1 ? Name : $"{Name} ({string.Join(',', Modules.Select(m => m.FullName)) })";
    }

    public static class ModulePackageExtensions
    {
        public static string ContaningModules(this ModulePackage me) => string.Join(',', me.Modules.Select(m => m.FullName));
        public static IEnumerable<ModulePackage> AsPackages(this IEnumerable<Module>? modules)
        {
            if (modules is null) return Array.Empty<ModulePackage>();
            var packages = new Dictionary<string, List<(Module Module, int ScaleId)>>(modules.Count());
            var scales = modules.GroupBy(m => m.Standard.ScaleId);
            foreach (var scale in scales)
            {
                foreach (var module in scale.OrderBy(m => m.FullName))
                {
                    if (module.PackageLabel.HasValue())
                    {
                        var packageKey = module.PackageLabel;
                        if (packageKey.HasValue())
                        {
                            if (!packages.ContainsKey(packageKey)) packages.Add(packageKey, new List<(Module, int)>());
                            packages[packageKey].Add((module, scale.Key));
                        }
                    }
                    else if (module.ConfigurationLabel.HasValue())
                    {
                        var packageKey = module.FullName;
                        if (!packages.ContainsKey(packageKey)) packages.Add(packageKey, new List<(Module, int)>());
                        packages[packageKey].Add((module, scale.Key));
                    }
                    else
                    {
                        packages.Add(module.FullName, new List<(Module, int)>() { (module, scale.Key) });
                    }
                }
            }
            return packages.Select((p, i) => new ModulePackage(i, p.Value.Count == 1 ? p.Value.First().Module.FullName : p.Key, p.Value.Select(v => v.Module).AsEnumerable()) { ScaleId = p.Value.First().ScaleId });
        }
    }
}
