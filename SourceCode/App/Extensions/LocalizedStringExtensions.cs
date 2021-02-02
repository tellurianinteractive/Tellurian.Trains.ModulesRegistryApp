using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ModulesRegistry.Extensions
{
    public static class LocalizedStringExtensions
    {
        public static string AddOrEdit(this IStringLocalizer me, string objectName, bool isAdd) =>
            isAdd ? me["Add"].Value : me["Edit"].Value + " " + me[objectName].Value.ToLowerInvariant();
    }
}
