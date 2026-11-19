using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.Cargos;

public static class CargoFormExtensions
{
    extension(Cargo entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public CargoFormData AsFormData() => new()
        {
            NhmCode = entity.NhmCode,
            DefaultClasses = entity.DefaultClasses,
            FromYear = entity.FromYear,
            UptoYear = entity.UptoYear,
            IsExpress = entity.IsExpress,
            IsCoolingRequired = entity.IsCoolingRequired,
            EN = entity.EN ?? string.Empty,
            DE = entity.DE ?? string.Empty,
            NL = entity.NL,
            FR = entity.FR,
            IT = entity.IT,
            PL = entity.PL,
            DA = entity.DA ?? string.Empty,
            SV = entity.SV ?? string.Empty,
            NB = entity.NB ?? string.Empty,
        };
    }

    extension(CargoFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns. Create: <c>form.AsEntity()</c>
        /// starts from a new entity. The service corrects the values when saving (text, years). Call it only with validated form data.
        /// </summary>
        public Cargo AsEntity(Cargo? existing = null)
        {
            var entity = existing ?? new Cargo();
            entity.NhmCode = form.NhmCode;
            entity.DefaultClasses = form.DefaultClasses;
            entity.FromYear = (short?)form.FromYear;
            entity.UptoYear = (short?)form.UptoYear;
            entity.IsExpress = form.IsExpress;
            entity.IsCoolingRequired = form.IsCoolingRequired;
            entity.EN = form.EN;
            entity.DE = form.DE;
            entity.NL = form.NL;
            entity.FR = form.FR;
            entity.IT = form.IT;
            entity.PL = form.PL;
            entity.DA = form.DA;
            entity.SV = form.SV;
            entity.NB = form.NB;
            return entity;
        }
    }
}