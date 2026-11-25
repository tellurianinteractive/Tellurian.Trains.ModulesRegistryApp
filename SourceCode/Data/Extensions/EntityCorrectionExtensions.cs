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
    // An optional text: ordinary text, null when empty.
    private const TextCleanup Optional = TextCleanup.OrdinaryText | TextCleanup.NullIfEmpty;

    extension(Station station)
    {
        /// <param name="countryCasings">The casing rules of the station's country (<c>AppLanguages.CasingsOf</c>).</param>
        public void CorrectInput(TextCasings countryCasings)
        {
            station.FullName = station.FullName.Corrected(TextCase.InitialCapital);
            station.Signature = station.Signature.Corrected(countryCasings.Of(TextKinds.StationSignature, TextCase.InitialCapital));
            // The key's name only when a key is required; the hidden field is still posted.
            station.LockKeyName = station.IsKeyRequired ? station.LockKeyName.Corrected(TextCase.AsTyped, Optional) : null;
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
            customer.Comment = customer.Comment.Corrected(TextCase.AsTyped, Optional);
            customer.TrackOrArea = customer.TrackOrArea.Corrected(TextCase.AsTyped, Optional);
            (customer.OpenedYear, customer.ClosedYear) = YearPeriod.Corrected(customer.OpenedYear, customer.ClosedYear);
            customer.TrackOrAreaColor = RegionColors.Nearest(customer.TrackOrAreaColor); // One of the ten colours, since 1.15.22.
            foreach (var cargo in customer.Cargos ?? []) cargo.CorrectInput();
        }
    }

    extension(StationCustomerCargo cargo)
    {
        public void CorrectInput()
        {
            (cargo.FromYear, cargo.UptoYear) = YearPeriod.Corrected(cargo.FromYear, cargo.UptoYear);
            cargo.TrackOrArea = cargo.TrackOrArea.Corrected(TextCase.AsTyped, Optional);
            cargo.TrackOrAreaColor = RegionColors.Nearest(cargo.TrackOrAreaColor); // White is "as the customer".
            cargo.SpecificWagonClass = cargo.SpecificWagonClass.Corrected(TextCase.AsTyped, Optional);
            cargo.SpecialCargoName = cargo.SpecialCargoName.Corrected(TextCase.Sentence, Optional); // A cargo name, as the cargo types'.
            cargo.MaxTrainsetLength = cargo.MaxTrainsetLength.OrNullIfZero();
        }
    }

    extension(ModuleStandard standard)
    {
        public void CorrectInput()
        {
            standard.ShortName = standard.ShortName.Corrected(TextCase.AsTyped); // An abbreviation, "FREMO/H0-RE".
            standard.TrackSystem = standard.TrackSystem.Corrected(TextCase.AsTyped, Optional);
            standard.Wheelset = standard.Wheelset.Corrected(TextCase.AsTyped, Optional);
            standard.Couplings = standard.Couplings.Corrected(TextCase.AsTyped, Optional);
            standard.Electricity = standard.Electricity.Corrected(TextCase.AsTyped, Optional);
            standard.PreferredTheme = standard.PreferredTheme.Corrected(TextCase.AsTyped, Optional);
            if (!standard.MainThemeId.IsTheme) standard.MainThemeId = (int)Theme.Any; // The list offers Any, Europe and US.
            standard.AcceptedNorm = standard.AcceptedNorm.Corrected(TextCase.AsTyped, TextCleanup.Trimmed | TextCleanup.NullIfEmpty); // Often a link, kept as typed.
            standard.NormalGauge = standard.NormalGauge is > 0 ? standard.NormalGauge : null;
            standard.NarrowGauge = standard.NarrowGauge is > 0 ? standard.NarrowGauge : null;
            standard.ExtendsStandardId = standard.ExtendsStandardId.OrNullIfZero();
            standard.OnlyValidInCountryId = standard.OnlyValidInCountryId.OrNullIfZero();
            if (standard.Id > 0 && standard.ExtendsStandardId == standard.Id) standard.ExtendsStandardId = null; // Not an extension of itself.
        }
    }

    extension(ModuleEndProfile profile)
    {
        public void CorrectInput()
        {
            profile.Designation = profile.Designation.Corrected(TextCase.AsTyped);
            profile.Description = profile.Description.Corrected(TextCase.AsTyped, Optional);
            profile.ModuleStandardId = profile.ModuleStandardId.OrNullIfZero();
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
            module.ConfigurationLabel = module.ConfigurationLabel.Corrected(TextCase.AsTyped, Optional);
            module.PackageLabel = module.PackageLabel.Corrected(TextCase.AsTyped, Optional);
            module.Theme = module.Theme.Corrected(TextCase.AsTyped, Optional);
            module.Note = module.Note.Corrected(TextCase.AsTyped, Optional);
            module.FremoNumber = module.FremoNumber.OrNullIfZero();
            module.NumberOfSections = module.NumberOfSections.OrNullIfZero();
            module.SpeedLimit = module.SpeedLimit.OrNullIfZero();
            // A secondary theme is another theme than the primary one, or none.
            module.SecondaryThemeId = module.SecondaryThemeId is int secondary && secondary.IsTheme && secondary != module.PrimaryThemeId ? secondary : null;
            // The lists offer only valid values; anything else is the default.
            if (!Enum.IsDefined((ObjectVisibility)module.ObjectVisibilityId)) module.ObjectVisibilityId = (int)ObjectVisibility.GroupMembers;
            if (!Enum.IsDefined((ModuleFunctionalState)module.FunctionalState)) module.FunctionalState = default;
            if (!Enum.IsDefined((ModuleLandscapeState)module.LandscapeState)) module.LandscapeState = default;
            if (!Enum.IsDefined((LandscapeSeason)module.LandscapeSeason)) module.LandscapeSeason = default;
            if (!Enum.IsDefined((SignalFeature)module.SignalFeature)) module.SignalFeature = default;
            if (!Enum.IsDefined((OverheadLineFeature)module.OverheadLineFeature)) module.OverheadLineFeature = default;
        }
    }

    extension(ModuleExit exit)
    {
        public void CorrectInput()
        {
            exit.Label = exit.Label.Corrected(TextCase.AsTyped);
            if (!Enum.IsDefined((ModuleExitDirection)exit.Direction)) exit.Direction = default;
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

    extension(Locality locality)
    {
        public void CorrectInput()
        {
            locality.Name = locality.Name.Corrected(TextCase.InitialCapital) ?? string.Empty;
            locality.StreetAddress = locality.StreetAddress.Corrected(TextCase.InitialCapital, Optional);
            locality.PostalCode = locality.PostalCode.Corrected(TextCase.UpperCase, Optional);
            locality.CityName = locality.CityName.Corrected(TextCase.Place) ?? string.Empty;
            locality.Description = locality.Description.Corrected(TextCase.AsTyped, Optional);
            if (!locality.IsModuleStorage && !locality.IsMeetingVenue) locality.IsModuleStorage = true; // At least one kind.
            if (locality.Latitude is null || locality.Longitude is null) (locality.Latitude, locality.Longitude) = (null, null); // Both or none.
        }
    }

    extension(Meeting meeting)
    {
        public void CorrectInput()
        {
            meeting.CityName = meeting.CityName.Corrected(TextCase.Place);
            meeting.Name = meeting.Name.Corrected(TextCase.AsTyped);
            meeting.VenueName = meeting.VenueName.Corrected(TextCase.AsTyped, Optional);
            meeting.ExternalLink = meeting.ExternalLink.Corrected(TextCase.AsTyped, TextCleanup.Trimmed | TextCleanup.NullIfEmpty);
            meeting.Details = meeting.Details.OrNullIfEmpty;
            meeting.Accomodation = meeting.Accomodation.OrNullIfEmpty;
            meeting.Food = meeting.Food.OrNullIfEmpty;
            if (meeting.EndDate < meeting.StartDate) meeting.EndDate = meeting.StartDate; // Corrected, not refused.
            if (!Enum.IsDefined((MeetingType)meeting.MeetingType)) meeting.MeetingType = default;
            if (!Enum.IsDefined((MeetingStatus)meeting.Status)) meeting.Status = (int)MeetingStatus.Preliminary;
        }
    }

    extension(Layout layout)
    {
        public void CorrectInput()
        {
            (layout.FirstYear, layout.LastYear) = YearPeriod.Corrected(layout.FirstYear, layout.LastYear);
            layout.ShortName = layout.ShortName.Corrected(TextCase.AsTyped, Optional);
            layout.Theme = layout.Theme.Corrected(TextCase.AsTyped, Optional);
            layout.Details = layout.Details.OrNullIfEmpty;
            layout.MaxNumberOfParticipants = layout.MaxNumberOfParticipants.OrNullIfZero();
        }
    }

    extension(LayoutStation station)
    {
        public void CorrectInput()
        {
            station.OtherName = station.OtherName.Corrected(TextCase.InitialCapital, Optional);
            station.OtherSignature = station.OtherSignature.Corrected(TextCase.AsTyped, Optional);
        }
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
            vehicle.KeeperSignature = vehicle.KeeperSignature.Corrected(TextCase.UpperCase);
            vehicle.Theme = vehicle.Theme.Corrected(TextCase.UpperCase);
            vehicle.VehicleClass = vehicle.VehicleClass.Corrected(TextCase.AsTyped);
            vehicle.VehicleNumber = vehicle.VehicleNumber.Corrected(TextCase.AsTyped);
            vehicle.ModelManufacturerName = vehicle.ModelManufacturerName.Corrected(TextCase.AsTyped);
            vehicle.PrototypeManufacturerName = Typed(vehicle.PrototypeManufacturerName);
            vehicle.PrototypeImageHref = vehicle.PrototypeImageHref.Corrected(TextCase.AsTyped, TextCleanup.Trimmed | TextCleanup.NullIfEmpty);
            vehicle.ModelNumber = Typed(vehicle.ModelNumber);
            vehicle.DecoderType = Typed(vehicle.DecoderType);
            vehicle.Note = Typed(vehicle.Note);
            // The function keys' names, as typed.
            (vehicle.F0, vehicle.F1, vehicle.F2, vehicle.F3, vehicle.F4, vehicle.F5, vehicle.F6, vehicle.F7, vehicle.F8, vehicle.F9) =
                (Typed(vehicle.F0), Typed(vehicle.F1), Typed(vehicle.F2), Typed(vehicle.F3), Typed(vehicle.F4), Typed(vehicle.F5), Typed(vehicle.F6), Typed(vehicle.F7), Typed(vehicle.F8), Typed(vehicle.F9));
            (vehicle.F10, vehicle.F11, vehicle.F12, vehicle.F13, vehicle.F14, vehicle.F15, vehicle.F16, vehicle.F17, vehicle.F18, vehicle.F19) =
                (Typed(vehicle.F10), Typed(vehicle.F11), Typed(vehicle.F12), Typed(vehicle.F13), Typed(vehicle.F14), Typed(vehicle.F15), Typed(vehicle.F16), Typed(vehicle.F17), Typed(vehicle.F18), Typed(vehicle.F19));
            (vehicle.F20, vehicle.F21, vehicle.F22, vehicle.F23, vehicle.F24, vehicle.F25, vehicle.F26, vehicle.F27, vehicle.F28, vehicle.F29) =
                (Typed(vehicle.F20), Typed(vehicle.F21), Typed(vehicle.F22), Typed(vehicle.F23), Typed(vehicle.F24), Typed(vehicle.F25), Typed(vehicle.F26), Typed(vehicle.F27), Typed(vehicle.F28), Typed(vehicle.F29));

            static string? Typed(string? text) => text.Corrected(TextCase.AsTyped, Optional);
        }
    }

    extension(StationTrack track)
    {
        public void CorrectInput()
        {
            track.PlatformLength = track.PlatformLength.OrNullIfZero();
            track.Designation = track.Designation.Corrected(TextCase.AsTyped);
            track.UsageNote = track.UsageNote.Corrected(TextCase.AsTyped, Optional);
            if (!Enum.IsDefined((StationTrackDirection)track.DirectionId)) track.DirectionId = (int)StationTrackDirection.Bidirectional;
        }
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
        /// <summary>
        /// The names (<see cref="PersonExtensions.CorrectNames"/>), the email addresses, the FREMO prefix and reserved loco addresses,
        /// a FREMO member number of 0 as empty, and the user's area of responsibility.
        /// </summary>
        public void CorrectInput()
        {
            person.CorrectNames();
            person.EmailAddresses = person.EmailAddresses.AsEmailAddresses().OrNullIfEmpty;
            person.FremoOwnerSignature = person.FremoOwnerSignature.Corrected(TextCase.AsTyped, TextCleanup.OrdinaryText | TextCleanup.NullIfEmpty);
            person.FremoReservedAdresses = person.FremoReservedAdresses.AsLocoAddresses().OrNullIfEmpty;
            person.FremoMemberNumber = person.FremoMemberNumber > 0 ? person.FremoMemberNumber : null;
            if (person.User is not null)
                person.User.AdministratorAreaOfResposibility = person.User.AdministratorAreaOfResposibility.Corrected(TextCase.AsTyped, TextCleanup.OrdinaryText | TextCleanup.NullIfEmpty);
        }
    }
}