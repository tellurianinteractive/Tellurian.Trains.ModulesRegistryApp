using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.Modules;

public static class ModuleFormExtensions
{
    extension(Module entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ModuleFormData AsFormData() => new()
        {
            StandardId = entity.StandardId,
            FullName = entity.FullName ?? string.Empty,
            ConfigurationLabel = entity.ConfigurationLabel,
            PackageLabel = entity.PackageLabel,
            FremoNumber = entity.FremoNumber,
            ObjectVisibilityId = entity.ObjectVisibilityId,
            FunctionalState = entity.FunctionalState,
            LandscapeState = entity.LandscapeState,
            LandscapeSeason = entity.LandscapeSeason,
            SignalFeature = entity.SignalFeature,
            OverheadLineFeature = entity.OverheadLineFeature,
            StationId = entity.StationId,
            StoredAtLocalityId = entity.StoredAtLocalityId,
            Theme = entity.Theme,
            PrimaryThemeId = entity.PrimaryThemeId.IsTheme ? entity.PrimaryThemeId : null,
            SecondaryThemeId = entity.SecondaryThemeId,
            Radius = entity.Radius,
            Angle = entity.Angle,
            Straight = entity.Straight,
            Width = entity.Width,
            IsUnavailable = entity.IsUnavailable,
            IsStandAlone = entity.IsStandAlone,
            IsTurntable = entity.IsTurntable,
            IsDuckunder = entity.IsDuckunder,
            IsSignalModule = entity.IsSignalModule,
            IsEndProfileSwitcher = entity.IsEndProfileSwitcher,
            IsEndOfLine = entity.IsEndOfLine,
            HasNormalGauge = entity.HasNormalGauge,
            HasNarrowGauge = entity.HasNarrowGauge,
            HasIntegratedLocoNet = entity.HasIntegratedLocoNet,
            Is2R = entity.Is2R,
            Is3R = entity.Is3R,
            Note = entity.Note,
            NumberOfSections = entity.NumberOfSections,
            RepresentsFromYear = entity.RepresentsFromYear,
            RepresentsUptoYear = entity.RepresentsUptoYear,
            SpeedLimit = entity.SpeedLimit,
            NumberOfThroughTracks = entity.NumberOfThroughTracks,
        };
    }

    extension(ModuleFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the scale, length, documents,
        /// exits and owners stay (the service sets scale and length). Create: <c>form.AsEntity()</c> starts from a new entity; the owner
        /// is given to the service. Call it only with validated form data.
        /// </summary>
        public Module AsEntity(Module? existing = null)
        {
            var entity = existing ?? new Module();
            entity.StandardId = form.StandardId;
            entity.FullName = form.FullName;
            entity.ConfigurationLabel = form.ConfigurationLabel;
            entity.PackageLabel = form.PackageLabel;
            entity.FremoNumber = form.FremoNumber;
            entity.ObjectVisibilityId = form.ObjectVisibilityId;
            entity.FunctionalState = form.FunctionalState;
            entity.LandscapeState = form.LandscapeState;
            entity.LandscapeSeason = form.LandscapeSeason;
            entity.SignalFeature = form.SignalFeature;
            entity.OverheadLineFeature = form.OverheadLineFeature;
            entity.Theme = form.Theme;
            entity.PrimaryThemeId = form.PrimaryThemeId ?? 0; // The service gives the standard's theme.
            entity.SecondaryThemeId = form.SecondaryThemeId;
            entity.Radius = form.Radius;
            entity.Angle = form.Angle;
            entity.Straight = form.Straight;
            entity.Width = form.Width;
            entity.IsUnavailable = form.IsUnavailable;
            entity.IsStandAlone = form.IsStandAlone;
            entity.IsTurntable = form.IsTurntable;
            entity.IsDuckunder = form.IsDuckunder;
            entity.IsSignalModule = form.IsSignalModule;
            entity.IsEndProfileSwitcher = form.IsEndProfileSwitcher;
            entity.IsEndOfLine = form.IsEndOfLine;
            entity.HasNormalGauge = form.HasNormalGauge;
            entity.HasNarrowGauge = form.HasNarrowGauge;
            entity.HasIntegratedLocoNet = form.HasIntegratedLocoNet;
            entity.Is2R = form.Is2R;
            entity.Is3R = form.Is3R;
            entity.Note = form.Note;
            entity.StationId = form.StationId is null or 0 ? null : form.StationId;
            entity.StoredAtLocalityId = form.StoredAtLocalityId is null or 0 ? null : form.StoredAtLocalityId;
            entity.NumberOfSections = (short?)form.NumberOfSections;
            entity.RepresentsFromYear = (short?)form.RepresentsFromYear;
            entity.RepresentsUptoYear = (short?)form.RepresentsUptoYear;
            entity.SpeedLimit = (short?)form.SpeedLimit;
            entity.NumberOfThroughTracks = (short)form.NumberOfThroughTracks;
            return entity;
        }
    }

    extension(ModuleExit entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ModuleExitFormData AsFormData() => new()
        {
            Direction = entity.Direction,
            Label = entity.Label ?? string.Empty,
            EndProfileId = entity.EndProfileId,
        };
    }

    extension(ModuleExitFormData form)
    {
        /// <summary>Form data → entity: <c>form.AsEntity(existing)</c>, or <c>form.AsEntity(moduleId: …)</c> for a new exit of the module.</summary>
        public ModuleExit AsEntity(ModuleExit? existing = null, int moduleId = 0)
        {
            var entity = existing ?? new ModuleExit { ModuleId = moduleId };
            entity.Direction = form.Direction;
            entity.Label = form.Label;
            entity.EndProfileId = form.EndProfileId;
            return entity;
        }
    }
}