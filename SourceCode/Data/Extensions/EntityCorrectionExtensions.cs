namespace ModulesRegistry.Data.Extensions;

/// <summary>
/// The corrections each entity gets when saved (Migration step 3.5, decided 2026-11-19): what the app can correct is corrected
/// instead of validated, declared per field with Tellurian.Utilities <c>Corrected(TextCase, TextCleanup)</c> (ordinary text by
/// default), year periods (<see cref="YearPeriod"/>), zero as "not given" and common notations. A signature's casing depends on
/// the language of the station's country (<see cref="TextKinds"/>, declared in <c>AppLanguages</c>). Each service calls <c>CorrectInput</c> before saving; the interactive validators
/// then only refuse what can't be corrected. Ordinary text is corrected when each form moves to static SSR (step 3.6).
/// </summary>
public static class EntityCorrectionExtensions
{
    // A colour as #RRGGBB, also from a name or #RGB; anything else is kept, to be refused.
    private static string? Color(string? color) => color.NormalizedHexColor ?? color?.Trim();

    // An optional text: ordinary text, null when empty.
    private const TextCleanup Optional = TextCleanup.OrdinaryText | TextCleanup.NullIfEmpty;

    extension(Station station)
    {
        /// <param name="countryCasings">The casing rules of the station's country (<c>AppLanguages.CasingsOf</c>).</param>
        public void CorrectInput(TextCasings countryCasings)
        {
            station.FullName = station.FullName.Corrected(TextCase.InitialCapital);
            station.Signature = station.Signature.Corrected(countryCasings.Of(TextKinds.StationSignature, TextCase.InitialCapital));
            foreach (var track in station.StationTracks ?? []) track.CorrectInput();
        }
    }

    extension(ExternalStation station)
    {
        /// <param name="countryCasings">The casing rules of the station's country (<c>AppLanguages.CasingsOf</c>).</param>
        public void CorrectInput(TextCasings countryCasings)
        {
            station.FullName = station.FullName.Corrected(TextCase.InitialCapital);
            station.InternationalName = station.InternationalName.Corrected(TextCase.InitialCapital, Optional);
            station.Signature = station.Signature.Corrected(countryCasings.Of(TextKinds.StationSignature, TextCase.InitialCapital));
            (station.OpenedYear, station.ClosedYear) = YearPeriod.Corrected(station.OpenedYear, station.ClosedYear);
        }
    }

    extension(ExternalStationCustomer customer)
    {
        public void CorrectInput()
        {
            customer.CustomerName = customer.CustomerName.Corrected(TextCase.InitialCapital);
            (customer.OpenedYear, customer.ClosedYear) = YearPeriod.Corrected(customer.OpenedYear, customer.ClosedYear);
            foreach (var cargo in customer.ExternalStationCustomerCargos ?? []) cargo.CorrectInput();
        }
    }

    extension(ExternalStationCustomerCargo cargo)
    {
        public void CorrectInput()
        {
            (cargo.FromYear, cargo.UptoYear) = YearPeriod.Corrected(cargo.FromYear, cargo.UptoYear);
            cargo.SpecificWagonClass = cargo.SpecificWagonClass.Corrected(TextCase.AsTyped, Optional);
            cargo.SpecialCargoName = cargo.SpecialCargoName.Corrected(TextCase.Sentence, Optional); // A cargo name, as the cargo types'.
        }
    }

    extension(StationCustomer customer)
    {
        public void CorrectInput()
        {
            customer.CustomerName = customer.CustomerName.Corrected(TextCase.InitialCapital);
            (customer.OpenedYear, customer.ClosedYear) = YearPeriod.Corrected(customer.OpenedYear, customer.ClosedYear);
            customer.TrackOrAreaColor = Color(customer.TrackOrAreaColor);
            foreach (var cargo in customer.Cargos ?? []) cargo.CorrectInput();
        }
    }

    extension(StationCustomerCargo cargo)
    {
        public void CorrectInput()
        {
            (cargo.FromYear, cargo.UptoYear) = YearPeriod.Corrected(cargo.FromYear, cargo.UptoYear);
            cargo.TrackOrAreaColor = Color(cargo.TrackOrAreaColor);
        }
    }

    extension(Cargo cargo)
    {
        public void CorrectInput()
        {
            (cargo.FromYear, cargo.UptoYear) = YearPeriod.Corrected(cargo.FromYear, cargo.UptoYear);
            cargo.DefaultClasses = cargo.DefaultClasses.Corrected(TextCase.AsTyped, Optional);
            // The names in every language as sentences ("STYCKEGODS" becomes "Styckegods", "Gas (LPG)" is kept); required names
            // stay text, optional ones are null when empty.
            cargo.EN = cargo.EN.Corrected(TextCase.Sentence);
            cargo.DE = cargo.DE.Corrected(TextCase.Sentence);
            cargo.DA = cargo.DA.Corrected(TextCase.Sentence);
            cargo.SV = cargo.SV.Corrected(TextCase.Sentence);
            cargo.NB = cargo.NB.Corrected(TextCase.Sentence);
            cargo.NL = cargo.NL.Corrected(TextCase.Sentence, Optional);
            cargo.FR = cargo.FR.Corrected(TextCase.Sentence, Optional);
            cargo.IT = cargo.IT.Corrected(TextCase.Sentence, Optional);
            cargo.PL = cargo.PL.Corrected(TextCase.Sentence, Optional);
        }
    }

    extension(Module module)
    {
        public void CorrectInput()
        {
            module.FullName = module.FullName.Corrected(TextCase.InitialCapital);
            (module.RepresentsFromYear, module.RepresentsUptoYear) = YearPeriod.Corrected(module.RepresentsFromYear, module.RepresentsUptoYear);
        }
    }

    extension(Group group)
    {
        public void CorrectInput()
        {
            group.ShortName = group.ShortName.Corrected(TextCase.InitialCapital); // An abbreviation: "MEC", "KdMJK".
            group.FullName = group.FullName.Corrected(TextCase.Organisation);
            group.CityName = group.CityName.Corrected(TextCase.Place);
        }
    }

    extension(Meeting meeting)
    {
        public void CorrectInput() => meeting.CityName = meeting.CityName.Corrected(TextCase.Place);
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
            vehicle.PrototypeManufactureYear = vehicle.PrototypeManufactureYear.OrNullIfZeroOrNegative();
            vehicle.EnginePower = vehicle.EnginePower.OrNullIfZero();
            vehicle.PrototypeLength = vehicle.PrototypeLength.OrNullIfZero();
            vehicle.PrototypeWeight = vehicle.PrototypeWeight.OrNullIfZero();
        }
    }

    extension(StationTrack track)
    {
        public void CorrectInput() => track.PlatformLength = track.PlatformLength.OrNullIfZero();
    }

    extension(WiFredThrottle throttle)
    {
        public void CorrectInput()
        {
            throttle.MacAddress = throttle.MacAddress.AsMacAddress();
            throttle.Name = throttle.Name.Corrected(TextCase.Sentence);
            throttle.LocoAddress1 = throttle.LocoAddress1.OrNullIfZero();
            throttle.LocoAddress2 = throttle.LocoAddress2.OrNullIfZero();
            throttle.LocoAddress3 = throttle.LocoAddress3.OrNullIfZero();
            throttle.LocoAddress4 = throttle.LocoAddress4.OrNullIfZero();
        }
    }

    extension(Person person)
    {
        /// <summary>The names (<see cref="PersonExtensions.CorrectNames"/>), the email addresses and the reserved loco addresses.</summary>
        public void CorrectInput()
        {
            person.CorrectNames();
            person.EmailAddresses = person.EmailAddresses.AsEmailAddresses().OrNullIfEmpty;
            person.FremoReservedAdresses = person.FremoReservedAdresses.AsLocoAddresses().OrNullIfEmpty;
        }
    }
}