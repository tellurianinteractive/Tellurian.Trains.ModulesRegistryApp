#nullable disable

namespace ModulesRegistry.Data;

public partial class Scale
{
    public Scale()
    {
        ModuleStandards = new HashSet<ModuleStandard>();
        Modules = new HashSet<Module>();
    }

    public int Id { get; set; }
    public string ShortName { get; set; }
    public int Denominator { get; set; }

    public virtual ICollection<ModuleStandard> ModuleStandards { get; set; }
    public virtual ICollection<Module> Modules { get; set; }
}

# nullable enable
public static class ScaleExtensions
{
    public static string Display(this Scale? me) => me is null ? string.Empty : $"{me.ShortName} 1:{me.Denominator}";
}
