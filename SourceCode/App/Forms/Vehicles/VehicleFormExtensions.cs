using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.Vehicles;

public static class VehicleFormExtensions
{
    extension(Vehicle entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public VehicleFormData AsFormData() => new()
        {
            KeeperCountryId = entity.KeeperCountryId,
            KeeperSignature = entity.KeeperSignature ?? string.Empty,
            VehicleClass = entity.VehicleClass ?? string.Empty,
            VehicleNumber = entity.VehicleNumber ?? string.Empty,
            Theme = entity.Theme ?? string.Empty,
            PrototypeManufacturerName = entity.PrototypeManufacturerName,
            TractionFeatureId = entity.TractionFeatureId,
            PrototypeLength = entity.PrototypeLength,
            PrototypeWeight = entity.PrototypeWeight,
            PrototypeImageHref = entity.PrototypeImageHref,
            InventoryNumber = entity.InventoryNumber,
            ScaleId = entity.ScaleId,
            ModelManufacturerName = entity.ModelManufacturerName ?? string.Empty,
            ModelNumber = entity.ModelNumber,
            CouplingFeatureId = entity.CouplingFeatureId,
            WheelsFeatureId = entity.WheelsFeatureId,
            DecoderType = entity.DecoderType,
            HasSound = entity.HasSound,
            HasRemoteCouplings = entity.HasRemoteCouplings,
            IsWeathered = entity.IsWeathered,
            Note = entity.Note,
            F0 = entity.F0,
            F1 = entity.F1,
            F2 = entity.F2,
            F3 = entity.F3,
            F4 = entity.F4,
            F5 = entity.F5,
            F6 = entity.F6,
            F7 = entity.F7,
            F8 = entity.F8,
            F9 = entity.F9,
            F10 = entity.F10,
            F11 = entity.F11,
            F12 = entity.F12,
            F13 = entity.F13,
            F14 = entity.F14,
            F15 = entity.F15,
            F16 = entity.F16,
            F17 = entity.F17,
            F18 = entity.F18,
            F19 = entity.F19,
            F20 = entity.F20,
            F21 = entity.F21,
            F22 = entity.F22,
            F23 = entity.F23,
            F24 = entity.F24,
            F25 = entity.F25,
            F26 = entity.F26,
            F27 = entity.F27,
            F28 = entity.F28,
            F29 = entity.F29,
            PrototypeManufactureYear = entity.PrototypeManufactureYear,
            EnginePower = entity.EnginePower,
            ThisEmbodiementFromYear = entity.ThisEmbodiementFromYear,
            ThisEmbodiementUptoYear = entity.ThisEmbodiementUptoYear,
            DccAddress = entity.DccAddress,
        };
    }

    extension(VehicleFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the owner, the operator and the
        /// interoperability code stay. Create: <c>form.AsEntity(owningPersonId: …)</c> starts from a new entity owned by the given person.
        /// The service corrects the values when saving. Call it only with validated form data.
        /// </summary>
        public Vehicle AsEntity(Vehicle? existing = null, int owningPersonId = 0)
        {
            var entity = existing ?? new Vehicle { OwningPersonId = owningPersonId };
            entity.KeeperCountryId = form.KeeperCountryId;
            entity.KeeperSignature = form.KeeperSignature;
            entity.VehicleClass = form.VehicleClass;
            entity.VehicleNumber = form.VehicleNumber;
            entity.Theme = form.Theme;
            entity.PrototypeManufacturerName = form.PrototypeManufacturerName;
            entity.TractionFeatureId = form.TractionFeatureId;
            entity.PrototypeLength = form.PrototypeLength;
            entity.PrototypeWeight = form.PrototypeWeight;
            entity.PrototypeImageHref = form.PrototypeImageHref;
            entity.InventoryNumber = form.InventoryNumber;
            entity.ScaleId = form.ScaleId;
            entity.ModelManufacturerName = form.ModelManufacturerName;
            entity.ModelNumber = form.ModelNumber;
            entity.CouplingFeatureId = form.CouplingFeatureId;
            entity.WheelsFeatureId = form.WheelsFeatureId;
            entity.DecoderType = form.DecoderType;
            entity.HasSound = form.HasSound;
            entity.HasRemoteCouplings = form.HasRemoteCouplings;
            entity.IsWeathered = form.IsWeathered;
            entity.Note = form.Note;
            entity.F0 = form.F0;
            entity.F1 = form.F1;
            entity.F2 = form.F2;
            entity.F3 = form.F3;
            entity.F4 = form.F4;
            entity.F5 = form.F5;
            entity.F6 = form.F6;
            entity.F7 = form.F7;
            entity.F8 = form.F8;
            entity.F9 = form.F9;
            entity.F10 = form.F10;
            entity.F11 = form.F11;
            entity.F12 = form.F12;
            entity.F13 = form.F13;
            entity.F14 = form.F14;
            entity.F15 = form.F15;
            entity.F16 = form.F16;
            entity.F17 = form.F17;
            entity.F18 = form.F18;
            entity.F19 = form.F19;
            entity.F20 = form.F20;
            entity.F21 = form.F21;
            entity.F22 = form.F22;
            entity.F23 = form.F23;
            entity.F24 = form.F24;
            entity.F25 = form.F25;
            entity.F26 = form.F26;
            entity.F27 = form.F27;
            entity.F28 = form.F28;
            entity.F29 = form.F29;
            entity.PrototypeManufactureYear = (short?)form.PrototypeManufactureYear;
            entity.EnginePower = (short?)form.EnginePower;
            entity.ThisEmbodiementFromYear = (short?)form.ThisEmbodiementFromYear;
            entity.ThisEmbodiementUptoYear = (short?)form.ThisEmbodiementUptoYear;
            entity.DccAddress = (short?)form.DccAddress;
            return entity;
        }
    }
}