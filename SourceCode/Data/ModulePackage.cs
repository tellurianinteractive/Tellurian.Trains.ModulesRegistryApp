namespace ModulesRegistry.Data;

/// <summary>
/// This is not an entity that is stored in the database 
/// </summary>
public class ModulePackage(int id, ModulePackageType type, string name, string ownerName, int[] ownerPersonId, IEnumerable<AvailableModule> modules)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public string OwnerName { get; } = ownerName;
    public int[] OwnerPersonId { get; } = ownerPersonId;
    public IEnumerable<AvailableModule> Modules { get; } = modules;
    public ModulePackageType PackageType { get; } = type;
    public int ScaleId { get; init; }
    public int LayoutId { get; set; }
    public override string ToString() => this.ModuleNames();
}

public static class ModulePackageExtensions
{
    public static string ContaningModules(this ModulePackage me) => string.Join(',', me.Modules.Select(m => m.FullName));

    public static string ModuleNames(this ModulePackage it) =>
              it.PackageType switch
              {
                  ModulePackageType.Variants => string.Join(", ", it.Modules.Select(i => i.ConfigurationLabel)),
                  _ => string.Join(", ", it.Modules.Select(m => m.FullName))
              };
}

