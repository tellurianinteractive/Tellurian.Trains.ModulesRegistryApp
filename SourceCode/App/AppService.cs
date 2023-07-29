using System.Reflection;

namespace ModulesRegistry;

public class AppService
{
    public string AppVersion
    {
        get
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version; ;
            if (version is null) return string.Empty;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
    }

    public int LastCountryId { get; set; }
}
