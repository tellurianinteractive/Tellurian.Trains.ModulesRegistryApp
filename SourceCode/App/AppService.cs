using System;
using System.Reflection;

namespace ModulesRegistry
{
    public class AppService
    {
        public static string AppVersion
        {
            get
            {
                var version = Version;
                if (version is null) return string.Empty;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            }
        }
        private static Version? Version => Assembly.GetExecutingAssembly().GetName().Version;
    }
}
