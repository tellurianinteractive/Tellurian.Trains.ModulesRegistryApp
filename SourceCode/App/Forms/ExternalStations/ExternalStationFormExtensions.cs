using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.ExternalStations;

public static class ExternalStationFormExtensions
{
    extension(ExternalStation entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ExternalStationFormData AsFormData() => new()
        {
            RegionId = entity.RegionId,
            FullName = entity.FullName ?? string.Empty,
            InternationalName = entity.InternationalName,
            Signature = entity.Signature ?? string.Empty,
            OpenedYear = entity.OpenedYear,
            ClosedYear = entity.ClosedYear,
        };
    }

    extension(ExternalStationFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns, and keeps the region of a
        /// saved station, which can't be changed. Create: <c>form.AsEntity()</c> starts from a new entity. The service corrects the
        /// values when saving (names, signature, years). Call it only with validated form data.
        /// </summary>
        public ExternalStation AsEntity(ExternalStation? existing = null)
        {
            var entity = existing ?? new ExternalStation();
            if (entity.Id == 0) entity.RegionId = form.RegionId;
            entity.FullName = form.FullName;
            entity.InternationalName = form.InternationalName;
            entity.Signature = form.Signature;
            entity.OpenedYear = (short?)form.OpenedYear;
            entity.ClosedYear = (short?)form.ClosedYear;
            return entity;
        }
    }
}
