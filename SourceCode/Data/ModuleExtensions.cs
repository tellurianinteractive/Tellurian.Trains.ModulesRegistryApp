using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ModulesRegistry.Data
{
    public static class ModuleExtensions
    {
        public static Module Clone(this Module me) =>
            new()
            {
                Angle = me.Angle,
                FullName = me.CloneFullName(),
                FunctionalState = me.FunctionalState,
                HasIntegratedLocoNet = me.HasIntegratedLocoNet,
                HasNarrowGauge = me.HasNarrowGauge,
                HasNormalGauge = me.HasNormalGauge,
                Is2R = me.Is2R,
                Is3R = me.Is3R,
                IsDuckunder = me.IsDuckunder,
                IsJunction = me.IsJunction,
                IsSignal = me.IsSignal,
                IsStation = me.IsStation,
                IsTurntable = me.IsTurntable,
                IsUnavailable = me.IsUnavailable,
                LandscapeState = me.LandscapeState,
                Length = me.Length,
                ModuleExits = me.ModuleExits.Select(it => new ModuleExit
                {
                    Direction = it.Direction,
                    GableTypeId = it.GableTypeId,
                    Label = it.Label
                }).ToArray(),
                ModuleOwnerships = me.ModuleOwnerships.Select(it => new ModuleOwnership
                {
                    PersonId = it.PersonId,
                    GroupId = it.GroupId,
                    OwnedShare = it.OwnedShare
                }).ToArray(),
                Note = me.Note,
                NumberOfSections = me.NumberOfSections,
                NumberOfThroughTracks = me.NumberOfThroughTracks,
                ObjectVisibilityId = me.ObjectVisibilityId,
                OverheadLineFeature = me.OverheadLineFeature,
                PackageLabel = me.PackageLabel,
                Radius = me.Radius,
                RepresentsFromYear = me.RepresentsFromYear,
                RepresentsUptoYear = me.RepresentsUptoYear,
                ScaleId = me.ScaleId,
                SignalFeature = me.SignalFeature,
                SpeedLimit = me.SpeedLimit,
                StandardId = me.StandardId,
                Theme = me.Theme
            };

        static string CloneFullName(this Module module)
        {
            var random = new Random();
            var appended = $"-{random.Next(1, 1000)}";
            var totalLength = module.FullName.Length + appended.Length;
            if (totalLength <= 50) return $"{module.FullName}{appended}";
            return $"{module.FullName.Substring(0, 50 - appended.Length)}{appended}";
        }

        public static bool IsPartOfStation([NotNullWhen(true)] this Module me) => me.StationId.HasValue;

        public static IEnumerable<int> DocumentIds(this Module me)
        {
            if (me.PdfDocumentationId.HasValue) yield return me.PdfDocumentationId.Value;
            if (me.DwgDrawingId.HasValue) yield return me.DwgDrawingId.Value;
            if (me.SkpDrawingId.HasValue) yield return me.SkpDrawingId.Value;
        }
    }
}
