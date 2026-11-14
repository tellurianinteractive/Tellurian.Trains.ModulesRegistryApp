using ModulesRegistry.Data;
using ModulesRegistry.Data.Extensions;

namespace ModulesRegistry.Forms.Regions;

public static class RegionFormExtensions
{
    extension(Region entity)
    {
        // Entity → form data (GET: populate the form)
        public RegionFormData AsFormData() => new()
        {
            Id = entity.Id,
            CountryId = entity.CountryId == 0 ? null : entity.CountryId,
            RepresentativeExternalStationId = entity.RepresentativeExternalStationId,
            LocalName = entity.LocalName ?? string.Empty,
            BackColor = entity.BackColor ?? string.Empty,
            Description = entity.Description,
        };
    }

    extension(RegionFormData form)
    {
        // Form data → entity.
        // POST edit:   form.AsEntity(existing) only touches the fields the form owns.
        // POST create: form.AsEntity() starts from a new entity.
        public Region AsEntity(Region? existing = null)
        {
            var entity = existing ?? new Region { Id = form.Id };
            entity.CountryId = form.CountryId!.Value;
            entity.RepresentativeExternalStationId = form.RepresentativeExternalStationId;
            entity.LocalName = form.LocalName.Trim();
            entity.BackColor = form.BackColor;
            entity.ForeColor = form.BackColor.TextColor();
            entity.Description = string.IsNullOrWhiteSpace(form.Description) ? null : form.Description.Trim();
            return entity;
        }
    }
}
