using ModulesRegistry.Data;

namespace ModulesRegistry.Forms.StationCustomers;

public static class StationCustomerFormExtensions
{
    extension(StationCustomer entity)
    {
        /// <summary>Entity → form data (GET: populate the form). A colour outside the palette is shown as the nearest one in it.</summary>
        public StationCustomerFormData AsFormData() => new()
        {
            CustomerName = entity.CustomerName ?? string.Empty,
            Comment = entity.Comment,
            TrackOrArea = entity.TrackOrArea,
            TrackOrAreaColor = RegionColors.Nearest(entity.TrackOrAreaColor),
            OpenedYear = entity.OpenedYear,
            ClosedYear = entity.ClosedYear,
        };
    }

    extension(StationCustomerFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns; the station, layout and
        /// cargo flows stay as loaded. Create: <c>form.AsEntity(stationId: ...)</c> starts a new customer at the station. The service
        /// corrects the values when saving. Call it only with validated form data.
        /// </summary>
        public StationCustomer AsEntity(StationCustomer? existing = null, int stationId = 0)
        {
            var entity = existing ?? new StationCustomer { StationId = stationId };
            entity.CustomerName = form.CustomerName;
            entity.Comment = form.Comment;
            entity.TrackOrArea = form.TrackOrArea;
            entity.TrackOrAreaColor = form.TrackOrAreaColor;
            entity.OpenedYear = (short?)form.OpenedYear;
            entity.ClosedYear = (short?)form.ClosedYear;
            return entity;
        }
    }

    extension(StationCustomerCargo entity)
    {
        /// <summary>Entity → form data (GET: populate the form).</summary>
        public StationCustomerCargoFormData AsFormData() => new()
        {
            OperatingDayId = entity.OperatingDayId,
            DirectionId = entity.DirectionId,
            QuantityUnitId = entity.QuantityUnitId,
            Quantity = entity.Quantity,
            ReadyTimeId = entity.ReadyTimeId,
            MaxTrainsetLength = entity.MaxTrainsetLength,
            CargoId = entity.CargoId,
            SpecificWagonClass = entity.SpecificWagonClass,
            PackageUnitId = entity.PackageUnitId,
            TrackOrArea = entity.TrackOrArea,
            TrackOrAreaColor = RegionColors.Nearest(entity.TrackOrAreaColor),
            FromYear = entity.FromYear,
            UptoYear = entity.UptoYear,
        };
    }

    extension(StationCustomerCargoFormData form)
    {
        /// <summary>
        /// Form data → entity. Edit: <c>form.AsEntity(existing)</c> only touches the fields the form owns (the special cargo name,
        /// operator and return flags stay). Create: <c>form.AsEntity(customerId: ...)</c> starts a new cargo flow of the customer.
        /// The service corrects the values when saving. Call it only with validated form data.
        /// </summary>
        public StationCustomerCargo AsEntity(StationCustomerCargo? existing = null, int customerId = 0)
        {
            var entity = existing ?? new StationCustomerCargo { StationCustomerId = customerId };
            entity.OperatingDayId = form.OperatingDayId;
            entity.DirectionId = form.DirectionId;
            entity.QuantityUnitId = form.QuantityUnitId;
            entity.Quantity = form.Quantity;
            entity.ReadyTimeId = form.ReadyTimeId;
            entity.MaxTrainsetLength = form.MaxTrainsetLength;
            entity.CargoId = form.CargoId;
            entity.SpecificWagonClass = form.SpecificWagonClass;
            entity.PackageUnitId = form.PackageUnitId;
            entity.TrackOrArea = form.TrackOrArea;
            entity.TrackOrAreaColor = form.TrackOrAreaColor;
            entity.FromYear = (short?)form.FromYear;
            entity.UptoYear = (short?)form.UptoYear;
            return entity;
        }
    }
}