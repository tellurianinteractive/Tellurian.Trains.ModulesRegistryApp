using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.Stations;

public static class StationFormExtensions
{
    extension(Station entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public StationFormData AsFormData() => new()
        {
            ModuleId = entity.PrimaryModuleId ?? 0,
            RegionId = entity.RegionId,
            FullName = entity.FullName ?? string.Empty,
            Signature = entity.Signature ?? string.Empty,
            HasCargoCustomers = entity.HasCargoCustomers,
            IsJunction = entity.IsJunction,
            IsTerminus = entity.IsTerminus,
            IsShadow = entity.IsShadow,
            IsHarbour = entity.IsHarbour,
            IsKeyRequired = entity.IsKeyRequired,
            OperationInstructionsMarkdown = entity.OperationInstructionsMarkdown,
        };
    }

    extension(StationFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the primary module, the document
        /// and the tracks stay. Create: <c>form.AsEntity()</c> starts from a new entity; the module is given to the service. The service
        /// corrects the values when saving. Call it only with validated form data.
        /// </summary>
        public Station AsEntity(Station? existing = null)
        {
            var entity = existing ?? new Station();
            entity.RegionId = form.RegionId is null or 0 ? null : form.RegionId;
            entity.FullName = form.FullName;
            entity.Signature = form.Signature;
            entity.HasCargoCustomers = form.HasCargoCustomers;
            entity.IsJunction = form.IsJunction;
            entity.IsTerminus = form.IsTerminus;
            entity.IsShadow = form.IsShadow;
            entity.IsHarbour = form.IsHarbour;
            entity.IsKeyRequired = form.IsKeyRequired;
            entity.OperationInstructionsMarkdown = form.OperationInstructionsMarkdown;
            return entity;
        }
    }

    extension(StationTrack entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public StationTrackFormData AsFormData() => new()
        {
            Designation = entity.Designation ?? string.Empty,
            DisplayOrder = entity.DisplayOrder,
            DirectionId = entity.DirectionId,
            MaxTrainLength = entity.MaxTrainLength,
            PlatformLength = entity.PlatformLength,
            SpeedLimit = entity.SpeedLimit,
            UsageNote = entity.UsageNote,
            IsThroughTrack = entity.IsThroughTrack,
            IsSiding = entity.IsSiding,
        };
    }

    extension(StationTrackFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns. Create:
        /// <c>form.AsEntity(stationId: …)</c> starts from a new track of the station. Call it only with validated form data.
        /// </summary>
        public StationTrack AsEntity(StationTrack? existing = null, int stationId = 0)
        {
            var entity = existing ?? new StationTrack { StationId = stationId };
            entity.Designation = form.Designation;
            entity.DisplayOrder = (short)form.DisplayOrder;
            entity.DirectionId = form.DirectionId;
            entity.MaxTrainLength = form.MaxTrainLength;
            entity.PlatformLength = form.PlatformLength;
            entity.SpeedLimit = (short?)form.SpeedLimit;
            entity.UsageNote = form.UsageNote;
            entity.IsThroughTrack = form.IsThroughTrack;
            entity.IsSiding = form.IsSiding;
            return entity;
        }
    }
}