namespace ModulesRegistry.Data.Extensions;

/// <summary>
/// The corrections each entity gets when saved (Migration step 3.5, decided 2026-11-19): what the app can correct is corrected
/// instead of validated, with the kind of name per field (<see cref="NameStyle"/>), year periods (<see cref="YearPeriod"/>),
/// zero as "not given" and common notations. Each service calls <c>CorrectInput</c> before saving; the interactive validators
/// then only refuse what can't be corrected. Ordinary text is corrected when each form moves to static SSR (step 3.6).
/// </summary>
public static class EntityCorrectionExtensions
{
    extension(Station station)
    {
        public void CorrectInput()
        {
            station.FullName = station.FullName.AsCapitalizedName(NameStyle.FirstLetter);
            station.Signature = station.Signature.AsCapitalizedName(NameStyle.FirstLetter);
            foreach (var track in station.StationTracks ?? []) track.CorrectInput();
        }
    }

    extension(ExternalStation station)
    {
        public void CorrectInput()
        {
            station.FullName = station.FullName.AsCapitalizedName(NameStyle.FirstLetter);
            station.InternationalName = station.InternationalName.AsCapitalizedName(NameStyle.FirstLetter);
            station.Signature = station.Signature.AsCapitalizedName(NameStyle.FirstLetter);
            (station.OpenedYear, station.ClosedYear) = YearPeriod.Corrected(station.OpenedYear, station.ClosedYear);
        }
    }

    extension(ExternalStationCustomer customer)
    {
        public void CorrectInput()
        {
            customer.CustomerName = customer.CustomerName.AsCapitalizedName(NameStyle.FirstLetter);
            (customer.OpenedYear, customer.ClosedYear) = YearPeriod.Corrected(customer.OpenedYear, customer.ClosedYear);
            foreach (var cargo in customer.ExternalStationCustomerCargos ?? []) cargo.CorrectInput();
        }
    }

    extension(ExternalStationCustomerCargo cargo)
    {
        public void CorrectInput() => (cargo.FromYear, cargo.UptoYear) = YearPeriod.Corrected(cargo.FromYear, cargo.UptoYear);
    }

    extension(StationCustomer customer)
    {
        public void CorrectInput()
        {
            customer.CustomerName = customer.CustomerName.AsCapitalizedName(NameStyle.FirstLetter);
            (customer.OpenedYear, customer.ClosedYear) = YearPeriod.Corrected(customer.OpenedYear, customer.ClosedYear);
            customer.TrackOrAreaColor = customer.TrackOrAreaColor.AsHexColor();
            foreach (var cargo in customer.Cargos ?? []) cargo.CorrectInput();
        }
    }

    extension(StationCustomerCargo cargo)
    {
        public void CorrectInput()
        {
            (cargo.FromYear, cargo.UptoYear) = YearPeriod.Corrected(cargo.FromYear, cargo.UptoYear);
            cargo.TrackOrAreaColor = cargo.TrackOrAreaColor.AsHexColor();
        }
    }

    extension(Cargo cargo)
    {
        public void CorrectInput() => (cargo.FromYear, cargo.UptoYear) = YearPeriod.Corrected(cargo.FromYear, cargo.UptoYear);
    }

    extension(Module module)
    {
        public void CorrectInput()
        {
            module.FullName = module.FullName.AsCapitalizedName(NameStyle.FirstLetter);
            (module.RepresentsFromYear, module.RepresentsUptoYear) = YearPeriod.Corrected(module.RepresentsFromYear, module.RepresentsUptoYear);
        }
    }

    extension(Group group)
    {
        public void CorrectInput()
        {
            group.ShortName = group.ShortName.AsCapitalizedName(NameStyle.FirstLetter); // An abbreviation: "MEC", "KdMJK".
            group.FullName = group.FullName.AsCapitalizedName(NameStyle.Organisation);
            group.CityName = group.CityName.AsCapitalizedName(NameStyle.AllWords);
        }
    }

    extension(Meeting meeting)
    {
        public void CorrectInput() => meeting.CityName = meeting.CityName.AsCapitalizedName(NameStyle.AllWords);
    }

    extension(Layout layout)
    {
        public void CorrectInput() => (layout.FirstYear, layout.LastYear) = YearPeriod.Corrected(layout.FirstYear, layout.LastYear);
    }

    extension(Vehicle vehicle)
    {
        public void CorrectInput()
        {
            (vehicle.ThisEmbodiementFromYear, vehicle.ThisEmbodiementUptoYear) = YearPeriod.Corrected(vehicle.ThisEmbodiementFromYear, vehicle.ThisEmbodiementUptoYear);
            vehicle.PrototypeManufactureYear = vehicle.PrototypeManufactureYear > 0 ? vehicle.PrototypeManufactureYear : null;
            vehicle.EnginePower = vehicle.EnginePower.AsNullIfZero();
            vehicle.PrototypeLength = vehicle.PrototypeLength.AsNullIfZero();
            vehicle.PrototypeWeight = vehicle.PrototypeWeight.AsNullIfZero();
        }
    }

    extension(StationTrack track)
    {
        public void CorrectInput() => track.PlatformLength = track.PlatformLength.AsNullIfZero();
    }

    extension(WiFredThrottle throttle)
    {
        public void CorrectInput()
        {
            throttle.MacAddress = throttle.MacAddress.AsMacAddress();
            throttle.LocoAddress1 = throttle.LocoAddress1.AsNullIfZero();
            throttle.LocoAddress2 = throttle.LocoAddress2.AsNullIfZero();
            throttle.LocoAddress3 = throttle.LocoAddress3.AsNullIfZero();
            throttle.LocoAddress4 = throttle.LocoAddress4.AsNullIfZero();
        }
    }

    extension(Person person)
    {
        /// <summary>The names (<see cref="PersonExtensions.CorrectNames"/>), the email addresses and the reserved loco addresses.</summary>
        public void CorrectInput()
        {
            person.CorrectNames();
            person.EmailAddresses = person.EmailAddresses.AsEmailAddresses().AsNullIfEmpty();
            person.FremoReservedAdresses = person.FremoReservedAdresses.AsLocoAddresses().AsNullIfEmpty();
        }
    }
}