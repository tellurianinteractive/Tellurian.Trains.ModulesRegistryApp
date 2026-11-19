using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Forms.Regions;

public static class RegionFormExtensions
{
    extension(Region entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public RegionFormData AsFormData() => new()
        {
            CountryId = entity.CountryId,
            RepresentativeExternalStationId = entity.RepresentativeExternalStationId,
            LocalName = entity.LocalName ?? string.Empty,
            // A colour from before the colours were limited must be chosen again.
            BackColor = RegionColors.Permitted(entity.BackColor) ?? string.Empty,
            Description = entity.Description,
        };
    }

    extension(RegionFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns.
        /// Create: <c>form.AsEntity()</c> starts from a new entity. Call it only with validated form data.
        /// </summary>
        public Region AsEntity(Region? existing = null)
        {
            var entity = existing ?? new Region();
            entity.CountryId = form.CountryId;
            entity.RepresentativeExternalStationId = form.RepresentativeExternalStationId;
            entity.LocalName = form.LocalName.Trim();
            entity.BackColor = form.BackColor;
            entity.ForeColor = entity.BackColor.TextColor;
            entity.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
            return entity;
        }
    }
}