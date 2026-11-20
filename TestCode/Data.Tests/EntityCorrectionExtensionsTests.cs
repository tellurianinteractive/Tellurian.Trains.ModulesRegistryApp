namespace ModulesRegistry.Data.Extensions.Tests;

/// <summary>
/// The corrections each entity gets when saved (Migration step 3.5, decided 2026-11-19), replacing the interactive validators'
/// capitalisation and year rules: the kind of name per field, year periods, zero as "not given" and common notations.
/// </summary>
[TestClass]
public sealed class EntityCorrectionExtensionsTests
{
    [TestMethod]
    public void Station()
    {
        var station = new Station { FullName = "nedre Ullerud C", Signature = "nu", StationTracks = [new StationTrack { PlatformLength = 0 }] };
        station.CorrectInput(new TextCasings());
        Assert.AreEqual("Nedre Ullerud C", station.FullName, "Only the first letter: the rest may be abbreviations.");
        Assert.AreEqual("Nu", station.Signature, "A country without a rule: only the first letter.");
        Assert.IsNull(station.StationTracks.Single().PlatformLength);
    }

    [TestMethod]
    [DataRow("CST", TextCase.Capitalized, "Cst", DisplayName = "Swedish")]
    [DataRow("ff", TextCase.UpperCase, "FF", DisplayName = "German")]
    [DataRow(" Hbf ", TextCase.UpperCase, "HBF", DisplayName = "German, trimmed")]
    public void TheSignatureFollowsTheRuleOfTheCountry(string signature, TextCase rule, string expected)
    {
        var casings = new TextCasings { [TextKinds.StationSignature] = rule };
        var station = new Station { FullName = "Station", Signature = signature };
        var external = new ExternalStation { FullName = "Station", Signature = signature };
        station.CorrectInput(casings);
        external.CorrectInput(casings);
        Assert.AreEqual(expected, station.Signature);
        Assert.AreEqual(expected, external.Signature);
    }

    [TestMethod]
    public void SignatureInCapitalsIsKept()
    {
        var station = new ExternalStation { FullName = "Frankfurt Hbf", InternationalName = "frankfurt", Signature = "FF", OpenedYear = 1912, ClosedYear = 1888 };
        station.CorrectInput(new TextCasings());
        Assert.AreEqual("FF", station.Signature);
        Assert.AreEqual("Frankfurt", station.InternationalName);
        Assert.AreEqual((short)1888, station.OpenedYear, "Reversed years are swapped.");
        Assert.AreEqual((short)1912, station.ClosedYear);
    }

    [TestMethod]
    public void StationCustomerAndItsCargoFlows()
    {
        var customer = new StationCustomer { CustomerName = "ica Maxi", OpenedYear = 0, ClosedYear = 1990, TrackOrAreaColor = "f80", Cargos = [new StationCustomerCargo { FromYear = 1995, UptoYear = 1970, TrackOrAreaColor = "#00ff00" }] };
        customer.CorrectInput();
        Assert.AreEqual("Ica Maxi", customer.CustomerName);
        Assert.IsNull(customer.OpenedYear, "Zero is not given.");
        Assert.AreEqual((short)1990, customer.ClosedYear);
        Assert.AreEqual("#FF8800", customer.TrackOrAreaColor);
        var cargo = customer.Cargos.Single();
        Assert.AreEqual((short)1970, cargo.FromYear);
        Assert.AreEqual((short)1995, cargo.UptoYear);
        Assert.AreEqual("#00FF00", cargo.TrackOrAreaColor);
    }

    [TestMethod]
    public void ExternalStationCustomerAndItsCargoFlows()
    {
        var customer = new ExternalStationCustomer { CustomerName = "stora Enso", ExternalStationCustomerCargos = [new ExternalStationCustomerCargo { FromYear = -1, UptoYear = 2012 }] };
        customer.CorrectInput();
        Assert.AreEqual("Stora Enso", customer.CustomerName);
        Assert.IsNull(customer.ExternalStationCustomerCargos.Single().FromYear);
    }

    [TestMethod]
    public void GroupHasAnAbbreviationAnOrganisationNameAndACity()
    {
        var group = new Group { ShortName = "KdMJK", FullName = "modellbahnfreunde e.v.", CityName = "frankfurt am main" };
        group.CorrectInput();
        Assert.AreEqual("KdMJK", group.ShortName);
        Assert.AreEqual("Modellbahnfreunde e.V.", group.FullName);
        Assert.AreEqual("Frankfurt am Main", group.CityName);
    }

    [TestMethod]
    public void ModuleMeetingLayoutAndCargo()
    {
        var module = new Module { FullName = "hällsjö", RepresentsFromYear = 1965, RepresentsUptoYear = 1950 };
        module.CorrectInput();
        Assert.AreEqual("Hällsjö", module.FullName);
        Assert.AreEqual((short)1950, module.RepresentsFromYear);

        var meeting = new Meeting { CityName = "cortina d'ampezzo" };
        meeting.CorrectInput();
        Assert.AreEqual("Cortina d'Ampezzo", meeting.CityName);

        var layout = new Layout { FirstYear = 0, LastYear = 0 };
        layout.CorrectInput();
        Assert.IsNull(layout.FirstYear);
        Assert.IsNull(layout.LastYear);

        var cargo = new Cargo { FromYear = 2000, UptoYear = 1910 };
        cargo.CorrectInput();
        Assert.AreEqual((short)1910, cargo.FromYear);
    }

    [TestMethod]
    public void Vehicle()
    {
        var vehicle = new Vehicle { ThisEmbodiementFromYear = 1980, ThisEmbodiementUptoYear = 1970, PrototypeManufactureYear = 0, EnginePower = 0, PrototypeLength = 0m, PrototypeWeight = 12.5m };
        vehicle.CorrectInput();
        Assert.AreEqual((short)1970, vehicle.ThisEmbodiementFromYear);
        Assert.IsNull(vehicle.PrototypeManufactureYear);
        Assert.IsNull(vehicle.EnginePower);
        Assert.IsNull(vehicle.PrototypeLength);
        Assert.AreEqual(12.5m, vehicle.PrototypeWeight);
    }

    [TestMethod]
    public void WiFredThrottle()
    {
        var throttle = new WiFredThrottle { MacAddress = "aabb.ccdd.eeff", LocoAddress1 = 0, LocoAddress2 = 3 };
        throttle.CorrectInput();
        Assert.AreEqual("AA:BB:CC:DD:EE:FF", throttle.MacAddress);
        Assert.IsNull(throttle.LocoAddress1);
        Assert.AreEqual((short)3, throttle.LocoAddress2);
    }

    [TestMethod]
    public void PersonAddresses()
    {
        var person = new Person { FirstName = "anna", LastName = "berg", EmailAddresses = "Anna@Example.se, anna@example.se; b@c.se", FremoReservedAdresses = "15-10; 3" };
        person.CorrectInput();
        Assert.AreEqual("Anna", person.FirstName);
        Assert.AreEqual("anna@example.se;b@c.se", person.EmailAddresses);
        Assert.AreEqual("3,10-15", person.FremoReservedAdresses);

        var withoutAddresses = new Person { FirstName = "Anna", LastName = "Berg", EmailAddresses = " ", FremoReservedAdresses = "" };
        withoutAddresses.CorrectInput();
        Assert.IsNull(withoutAddresses.EmailAddresses);
        Assert.IsNull(withoutAddresses.FremoReservedAdresses);
    }
}