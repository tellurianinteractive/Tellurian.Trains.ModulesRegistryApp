using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Waybills;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The rules of <see cref="StationWaybillGenerator"/> (Migration step 5.2), case by case; the comparison with the former procedure
/// is in Database.Tests (StationWaybillGeneratorTests).
/// </summary>
[TestClass]
public class StationWaybillGeneratorTests
{
    private const int Sweden = 1, Norway = 2;
    private static readonly CargoRow Timber = new() { Id = 1, EN = "Timber", SV = "Timmer", DefaultClasses = "Ks" };

    private static CargoFlowRow Flow(int id, int stationId, bool isSupply, int countryId = Sweden, bool isInternational = false,
        CargoRow? cargo = null, bool isModuleStation = true, int? representativeStationId = 31) => new()
        {
            FlowId = id,
            CustomerId = stationId * 10,
            StationId = stationId,
            StationName = $"Station {stationId}",
            CustomerName = $"Customer {id}",
            CountryId = countryId,
            Languages = "sv",
            IsSupply = isSupply,
            IsInternational = isInternational,
            Cargo = cargo ?? Timber,
            IsModuleStation = isModuleStation,
            Quantity = 2,
            QuantityUnitId = 1,
            QuantityUnitSingular = "Ton",
            QuantityUnitPlural = "Tons",
            PackageUnitSingular = "Pallet",
            PackageUnitPlural = "Pallets",
            RepresentativeStationId = representativeStationId,
            RepresentativeStationName = "Umeå",
            RegionName = "Norrland",
        };

    private static IReadOnlyList<Models.Waybill> Generate(int stationId, StoredWaybill stored, IEnumerable<CargoFlowRow> moduleFlows,
        IEnumerable<CargoFlowRow>? externalFlows = null, bool sending = true, bool receiving = true, int? customerId = null) =>
        StationWaybillGenerator.Generate(stationId, customerId, sending, receiving, [stored],
            moduleFlows.ToDictionary(f => f.FlowId), (externalFlows ?? []).ToDictionary(f => f.FlowId));

    [TestMethod]
    public void ModuleWaybillGoesFromSenderToReceiver()
    {
        var receiver = Flow(101, 1, isSupply: false);
        var sender = Flow(102, 2, isSupply: true);
        var waybill = Generate(1, new StoredWaybill { Id = 7, FlowId = 101, OtherFlowId = 102, PrintCount = 2 }, [receiver, sender]).Single();
        Assert.AreEqual(7, waybill.Id);
        Assert.AreEqual(2, waybill.Origin.StationId);
        Assert.AreEqual(1, waybill.Destination.StationId);
        Assert.AreEqual("Timmer", waybill.Destination.CargoName);
        Assert.AreEqual(2, waybill.PrintCount);
    }

    [TestMethod]
    public void ReceivingWaybillIsNotGivenWhenOnlySending()
    {
        var waybills = Generate(1, new StoredWaybill { FlowId = 101, OtherFlowId = 102 }, [Flow(101, 1, isSupply: false), Flow(102, 2, isSupply: true)], receiving: false);
        Assert.IsEmpty(waybills);
    }

    [TestMethod]
    public void SameDirectionsGiveNoWaybill()
    {
        var waybills = Generate(1, new StoredWaybill { FlowId = 101, OtherFlowId = 102 }, [Flow(101, 1, isSupply: false), Flow(102, 2, isSupply: false)]);
        Assert.IsEmpty(waybills);
    }

    [TestMethod]
    public void OtherCountryNeedsBothInternational()
    {
        StoredWaybill stored = new() { FlowId = 101, OtherFlowId = 102 };
        Assert.IsEmpty(Generate(1, stored, [Flow(101, 1, isSupply: false), Flow(102, 2, isSupply: true, Norway)]));
        Assert.IsEmpty(Generate(1, stored, [Flow(101, 1, isSupply: false), Flow(102, 2, isSupply: true, Norway, isInternational: true)]));
        Assert.HasCount(1, Generate(1, stored, [Flow(101, 1, isSupply: false, isInternational: true), Flow(102, 2, isSupply: true, Norway, isInternational: true)]));
    }

    [TestMethod]
    public void WaybillOfAnotherStationOrCustomerIsLeftOut()
    {
        StoredWaybill stored = new() { FlowId = 101, OtherFlowId = 102 };
        CargoFlowRow[] flows = [Flow(101, 1, isSupply: false), Flow(102, 2, isSupply: true)];
        Assert.IsEmpty(Generate(2, stored, flows));
        Assert.IsEmpty(Generate(1, stored, flows, customerId: 99));
        Assert.HasCount(1, Generate(1, stored, flows, customerId: 10));
    }

    [TestMethod]
    public void MissingFlowGivesNoWaybill()
    {
        Assert.IsEmpty(Generate(1, new StoredWaybill { FlowId = 101, OtherFlowId = 102 }, [Flow(101, 1, isSupply: false)]));
    }

    [TestMethod]
    public void CargoMustExistInSendersYears()
    {
        var grain = Timber with { FromYear = 1900, UptoYear = 1960 };
        StoredWaybill stored = new() { FlowId = 101, OtherFlowId = 102 };
        var receiver = Flow(101, 1, isSupply: false);
        Assert.IsEmpty(Generate(1, stored, [receiver, Flow(102, 2, isSupply: true, cargo: grain) with { FromYear = 1970 }]));
        Assert.IsEmpty(Generate(1, stored, [receiver, Flow(102, 2, isSupply: true, cargo: grain) with { UptoYear = 1890 }]));
        Assert.HasCount(1, Generate(1, stored, [receiver, Flow(102, 2, isSupply: true, cargo: grain) with { FromYear = 1950, UptoYear = 1990 }]));
    }

    [TestMethod]
    public void CargoMustExistWithinTheWindowOfBothFlows()
    {
        var grain = Timber with { FromYear = 1900, UptoYear = 1960 };
        StoredWaybill stored = new() { FlowId = 101, OtherFlowId = 102 };
        var sender = Flow(102, 2, isSupply: true, cargo: grain) with { FromYear = 1940, UptoYear = 1970 };
        Assert.IsEmpty(Generate(1, stored, [Flow(101, 1, isSupply: false) with { FromYear = 1965, UptoYear = 1990 }, sender]), "The window 1965-1970 is after the cargo.");
        Assert.HasCount(1, Generate(1, stored, [Flow(101, 1, isSupply: false) with { FromYear = 1950, UptoYear = 1990 }, sender]), "The window 1950-1970 has the cargo.");
    }

    [TestMethod]
    public void FlowsWithoutOverlappingYearsGiveNoWaybill()
    {
        StoredWaybill stored = new() { FlowId = 101, OtherFlowId = 102 };
        Assert.IsEmpty(Generate(1, stored, [Flow(101, 1, isSupply: false) with { FromYear = 1980 }, Flow(102, 2, isSupply: true) with { UptoYear = 1979 }]));
        Assert.HasCount(1, Generate(1, stored, [Flow(101, 1, isSupply: false) with { FromYear = 1980 }, Flow(102, 2, isSupply: true) with { UptoYear = 1980 }]), "The up-to year is included.");
    }

    [TestMethod]
    public void ShadowYardWindowIsTheOwnFlowsYears()
    {
        var grain = Timber with { UptoYear = 1960 };
        Assert.IsEmpty(Generate(1, new StoredWaybill { FlowId = 101 }, [Flow(101, 1, isSupply: false, cargo: grain) with { FromYear = 1970 }]));
        Assert.HasCount(1, Generate(1, new StoredWaybill { FlowId = 101 }, [Flow(101, 1, isSupply: false, cargo: grain) with { FromYear = 1950 }]));
    }

    [TestMethod]
    public void ExternalWaybillNeedsTheSameCargo()
    {
        StoredWaybill stored = new() { FlowId = 101, OtherExternalFlowId = 201 };
        var receiver = Flow(101, 1, isSupply: false);
        var oil = new CargoRow { Id = 4, EN = "Oil" };
        Assert.IsEmpty(Generate(1, stored, [receiver], [Flow(201, 32, isSupply: true, cargo: oil, isModuleStation: false)]));
        var waybill = Generate(1, stored, [receiver], [Flow(201, 32, isSupply: true, isModuleStation: false)]).Single();
        Assert.AreEqual(32, waybill.Origin.StationId);
    }

    [TestMethod]
    public void ExternalSenderInOtherCountryNeedsInternationalReceiver()
    {
        StoredWaybill stored = new() { FlowId = 101, OtherExternalFlowId = 201 };
        var external = Flow(201, 33, isSupply: true, Norway, isModuleStation: false);
        Assert.IsEmpty(Generate(1, stored, [Flow(101, 1, isSupply: false)], [external]));
        Assert.HasCount(1, Generate(1, stored, [Flow(101, 1, isSupply: false, isInternational: true)], [external]));
    }

    [TestMethod]
    public void ShadowYardIsTheRepresentativeStation()
    {
        var receiver = Flow(101, 1, isSupply: false) with { CustomerTrackOrArea = "Spår 3" };
        var waybill = Generate(1, new StoredWaybill { FlowId = 101 }, [receiver]).Single();
        Assert.AreEqual(31, waybill.Origin.StationId);
        Assert.AreEqual("Umeå", waybill.Origin.StationName);
        Assert.AreEqual("", waybill.Origin.Name);
        Assert.AreEqual("", waybill.Origin.TrackOrArea);
        Assert.IsFalse(waybill.Origin.IsModuleStation);
        Assert.AreEqual("Spår 3", waybill.Destination.TrackOrArea);
    }

    [TestMethod]
    public void NoShadowYardWithoutRepresentativeStation()
    {
        Assert.IsEmpty(Generate(1, new StoredWaybill { FlowId = 101 }, [Flow(101, 1, isSupply: false, representativeStationId: null)]));
    }

    [TestMethod]
    public void SendingToShadowYard()
    {
        var waybill = Generate(1, new StoredWaybill { FlowId = 101 }, [Flow(101, 1, isSupply: true)]).Single();
        Assert.AreEqual(1, waybill.Origin.StationId);
        Assert.AreEqual(31, waybill.Destination.StationId);
    }

    [TestMethod]
    public void QuantityTextsAreTheReceivers()
    {
        var receiver = Flow(101, 1, isSupply: false) with { Quantity = 1, QuantityUnitSingular = "Wagon", QuantityIsBearer = true };
        var waybill = Generate(1, new StoredWaybill { FlowId = 101, OtherFlowId = 102 }, [receiver, Flow(102, 2, isSupply: true)]).Single();
        Assert.AreEqual("Wagon", waybill.Origin.QuantityUnitResourceKey);
        Assert.AreEqual("Wagon", waybill.Destination.QuantityUnitResourceKey);
        Assert.AreEqual(1, waybill.Quantity);
    }

    [TestMethod]
    public void ModuleStationPiecesAreSingular()
    {
        var pieces = Flow(101, 1, isSupply: false) with { QuantityUnitId = CargoQuantityUnits.Piece, Quantity = 5 };
        Assert.AreEqual("Ton", pieces.QuantityUnitResourceCode);
        Assert.AreEqual("Pallet", pieces.PackagingUnitResourceCode);
        Assert.AreEqual("Tons", pieces.AsShadowYard().QuantityUnitResourceCode);
        Assert.AreEqual("Pallets", pieces.AsShadowYard().PackagingUnitResourceCode);
    }

    [TestMethod]
    public void CargoNameIsInFirstLanguageOrEnglish()
    {
        Assert.AreEqual("Timmer", Timber.NameFor("sv,en"));
        Assert.AreEqual("Timber", Timber.NameFor("hu"));
        Assert.AreEqual("Timber", Timber.NameFor(null));
        Assert.IsNull(Timber.NameFor("de"));
    }

    [TestMethod]
    public void EmptyTextsBecomeDefaults()
    {
        var receiver = Flow(101, 1, isSupply: false) with { Languages = " ", PackagingPreposition = null };
        var waybill = Generate(1, new StoredWaybill { FlowId = 101, OtherFlowId = 102 }, [receiver, Flow(102, 2, isSupply: true)]).Single();
        Assert.AreEqual("en", waybill.Destination.Languages);
        Assert.AreEqual("", waybill.Destination.PackagingPrepositionResourceCode);
        Assert.AreEqual("In", waybill.Origin.PackagingPrepositionResourceCode);
    }
}