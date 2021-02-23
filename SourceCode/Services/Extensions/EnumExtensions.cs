using ModulesRegistry.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;

namespace ModulesRegistry.Services.Extensions
{
    public static class EnumExtensions
    {
        private static ResourceManager ResourceManager => Resources.Strings.ResourceManager;

        public static IEnumerable<ListboxItem> ModuleFunctionalStateListboxItems() =>
            Enum.GetValues<ModuleFunctionalState>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));
        public static IEnumerable<ListboxItem> ModuleLandscapeStateListboxItems() =>
           Enum.GetValues<ModuleLandscapeState>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));
    }
}
