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

        public static IEnumerable<ListboxItem> OverheadLineFeatureListboxItems() =>
           Enum.GetValues<OverheadLineFeature>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));

        public static IEnumerable<ListboxItem> SignalFeatureListboxItems() =>
           Enum.GetValues<SignalFeature>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));

        public static IEnumerable<ListboxItem> StationEntryDirectionsListboxItems() =>
            Enum.GetValues<ModuleExitDirection>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));

        public static IEnumerable<ListboxItem> MeetingStatusListboxItems() =>
            Enum.GetValues<MeetingStatus>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));
        public static string MeetingStatus(this int id) => MeetingStatusListboxItems().Single(i => i.Id == id).Description;

        public static IEnumerable<ListboxItem> ObjectVisibilityListboxItems() =>
            Enum.GetValues<ObjectVisibility>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));
        public static IEnumerable<ListboxItem> StationTrackDirectionListboxItems() =>
            Enum.GetValues<StationTrackDirection>().Select(value => new ListboxItem((int)value, ResourceManager.GetString(value.ToString()) ?? value.ToString()));
        public static IEnumerable<string> StationTrackDirections() =>
            Enum.GetValues<StationTrackDirection>().Select(value => ResourceManager.GetString(value.ToString()) ?? value.ToString());

    }
}
