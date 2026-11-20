using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.ExternalStationCustomers;

public static class ExternalStationCustomerFormExtensions
{
    extension(ExternalStationCustomer entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ExternalStationCustomerFormData AsFormData() => new()
        {
            CustomerName = entity.CustomerName ?? string.Empty,
            OpenedYear = entity.OpenedYear,
            ClosedYear = entity.ClosedYear,
        };
    }

    extension(ExternalStationCustomerFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the cargo flows stay as
        /// loaded. Create: <c>form.AsEntity(stationId: ...)</c> starts a new customer at the station. The service corrects the values
        /// when saving. Call it only with validated form data.
        /// </summary>
        public ExternalStationCustomer AsEntity(ExternalStationCustomer? existing = null, int stationId = 0)
        {
            var entity = existing ?? new ExternalStationCustomer { ExternalStationId = stationId };
            entity.CustomerName = form.CustomerName;
            entity.OpenedYear = (short?)form.OpenedYear;
            entity.ClosedYear = (short?)form.ClosedYear;
            return entity;
        }
    }

    extension(ExternalStationCustomerCargo entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public ExternalStationCustomerCargoFormData AsFormData() => new()
        {
            CargoId = entity.CargoId,
            PackageUnitId = entity.PackageUnitId,
            OperatingDayId = entity.OperatingDayId,
            DirectionId = entity.DirectionId,
            Quantity = entity.Quantity,
            QuantityUnitId = entity.QuantityUnitId,
            SpecificWagonClass = entity.SpecificWagonClass,
            SpecialCargoName = entity.SpecialCargoName,
            FromYear = entity.FromYear,
            UptoYear = entity.UptoYear,
        };
    }

    extension(ExternalStationCustomerCargoFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns. Create:
        /// <c>form.AsEntity(customerId: ...)</c> starts a new cargo flow of the customer. The service corrects the values when saving.
        /// Call it only with validated form data.
        /// </summary>
        public ExternalStationCustomerCargo AsEntity(ExternalStationCustomerCargo? existing = null, int customerId = 0)
        {
            var entity = existing ?? new ExternalStationCustomerCargo { ExternalStationCustomerId = customerId };
            entity.CargoId = form.CargoId;
            entity.PackageUnitId = form.PackageUnitId;
            entity.OperatingDayId = form.OperatingDayId;
            entity.DirectionId = form.DirectionId;
            entity.Quantity = form.Quantity;
            entity.QuantityUnitId = form.QuantityUnitId;
            entity.SpecificWagonClass = form.SpecificWagonClass;
            entity.SpecialCargoName = form.SpecialCargoName;
            entity.FromYear = (short?)form.FromYear;
            entity.UptoYear = (short?)form.UptoYear;
            return entity;
        }
    }
}
