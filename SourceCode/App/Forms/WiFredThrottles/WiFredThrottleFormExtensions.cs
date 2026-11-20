using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.WiFredThrottles;

public static class WiFredThrottleFormExtensions
{
    extension(WiFredThrottle entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public WiFredThrottleFormData AsFormData() => new()
        {
            OwningPersonId = entity.OwningPersonId,
            MacAddress = entity.MacAddress ?? string.Empty,
            Name = entity.Name ?? string.Empty,
            InventoryNumber = entity.InventoryNumber,
            LocoAddress1 = entity.LocoAddress1,
            LocoAddress2 = entity.LocoAddress2,
            LocoAddress3 = entity.LocoAddress3,
            LocoAddress4 = entity.LocoAddress4,
        };
    }

    extension(WiFredThrottleFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns, and keeps a validated
        /// throttle's MAC address, which can't be changed. Create: <c>form.AsEntity()</c> starts from a new entity. The service corrects
        /// the values when saving (MAC address, DCC addresses, name). Call it only with validated form data.
        /// </summary>
        public WiFredThrottle AsEntity(WiFredThrottle? existing = null)
        {
            var entity = existing ?? new WiFredThrottle();
            entity.OwningPersonId = form.OwningPersonId;
            if (!entity.IsMacAddressLocked()) entity.MacAddress = form.MacAddress;
            entity.Name = form.Name;
            entity.InventoryNumber = (short)form.InventoryNumber;
            entity.LocoAddress1 = (short?)form.LocoAddress1;
            entity.LocoAddress2 = (short?)form.LocoAddress2;
            entity.LocoAddress3 = (short?)form.LocoAddress3;
            entity.LocoAddress4 = (short?)form.LocoAddress4;
            return entity;
        }
    }
}