using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ModulesRegistry
{
    public class AppService
    {
        public static string AppVersion { get
            {
                var version = Version;
                if (version is null) return string.Empty;
                return $"{version.Major}.{version.Minor}.{version.Build}";
            } }
        private static Version? Version => Assembly.GetExecutingAssembly().GetName().Version;
    }
}
