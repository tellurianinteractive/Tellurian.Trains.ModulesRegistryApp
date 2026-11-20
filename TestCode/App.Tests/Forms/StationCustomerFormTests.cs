using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.StationCustomers;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// A module station's freight customer form and its cargo flow form (Migration step 3.6): the mapping to and from the entities,
/// and their DataAnnotations run as the server runs them.
/// </summary>
[TestClass]
public sealed class StationCustomerFormTests
{
    private static StationCustomer ExistingCustomer() => new()
    {
        Id = 7, StationId = 3, LayoutId = 11, CustomerName = "Ica", Comment = "Groceries", TrackOrArea = "3", TrackOrAreaColor = RegionColors.Orange,
        OpenedYear = 1950, ClosedYear = 1995, Cargos = [new StationCustomerCargo { Id = 12, StationCustomerId = 7, CargoId = 5 }],
    };

    private static StationCustomerCargo ExistingCargo() => new()
    {
        Id = 12, StationCustomerId = 7, CargoId = 5, PackageUnitId = 2, OperatingDayId = 8, DirectionId = 2, Quantity = 4, QuantityUnitId = 4,
        ReadyTimeId = 3, MaxTrainsetLength = 6, SpecificWagonClass = "Gbs", SpecialCargoName = "Paper rolls", TrackOrArea = "4",
        TrackOrAreaColor = RegionColors.Green, FromYear = 1960, UptoYear = 1990,
    };

    [TestMethod]
    public void CustomerRoundTripChangesNothing() => AssertSameValues(ExistingCustomer(), ExistingCustomer().AsFormData().AsEntity(ExistingCustomer()));

    [TestMethod]
    public void CargoRoundTripChangesNothing() => AssertSameValues(ExistingCargo(), ExistingCargo().AsFormData().AsEntity(ExistingCargo()));

    [TestMethod]
    public void AnEditedCustomerKeepsItsStationLayoutAndCargoFlows()
    {
        var existing = ExistingCustomer();
        var form = existing.AsFormData();
        form.CustomerName = "Coop";

        var saved = form.AsEntity(existing, stationId: 99);

        Assert.AreSame(existing, saved);
        Assert.AreEqual(3, saved.StationId, "The station of a saved customer isn't changed by the form.");
        Assert.AreEqual(11, saved.LayoutId);
        Assert.HasCount(1, saved.Cargos);
    }

    [TestMethod]
    public void AnEditedCargoFlowKeepsItsSpecialName() =>
        Assert.AreEqual("Paper rolls", ExistingCargo().AsFormData().AsEntity(ExistingCargo()).SpecialCargoName, "Not a field of the form (it was hidden before).");

    [TestMethod]
    public void NewOnesBelongToTheStationAndCustomerOfTheAddress()
    {
        Assert.AreEqual(3, ExistingCustomer().AsFormData().AsEntity(stationId: 3).StationId);
        Assert.AreEqual(7, ExistingCargo().AsFormData().AsEntity(customerId: 7).StationCustomerId);
        Assert.AreEqual(0, ExistingCargo().AsFormData().AsEntity(customerId: 7).Id);
    }

    [TestMethod]
    [DataRow(null, RegionColors.White)]
    [DataRow("", RegionColors.White)]
    [DataRow("#ffff00", RegionColors.Yellow, DisplayName = "A permitted colour in lower case")]
    [DataRow("#FA0000", RegionColors.Red)]
    [DataRow("#050505", RegionColors.Black)]
    [DataRow("#0080FF", RegionColors.Blue)]
    public void AColourOutsideThePaletteIsShownAsTheNearestInIt(string? stored, string expected)
    {
        var customer = ExistingCustomer();
        customer.TrackOrAreaColor = stored;
        var cargo = ExistingCargo();
        cargo.TrackOrAreaColor = stored;
        Assert.AreEqual(expected, customer.AsFormData().TrackOrAreaColor);
        Assert.AreEqual(expected, cargo.AsFormData().TrackOrAreaColor);
    }

    [TestMethod]
    public void ValidFormsHaveNoErrors()
    {
        Assert.IsEmpty(InvalidMembers(ExistingCustomer().AsFormData()));
        Assert.IsEmpty(InvalidMembers(ExistingCargo().AsFormData()));
        Assert.IsEmpty(InvalidMembers(new StationCustomer().AsFormData().With(f => f.CustomerName = "Ica")), "A new customer with a name.");
        Assert.IsEmpty(InvalidMembers(StationCustomerCargo.Default(7).AsFormData().With(f => f.CargoId = 5)), "A new flow with a cargo type.");
    }

    [TestMethod]
    public void TheCustomerNameMustBeGiven() => CollectionAssert.AreEqual(new[] { "CustomerName" }, InvalidMembers(ExistingCustomer().AsFormData().With(f => f.CustomerName = "")));

    [TestMethod]
    public void TheCustomerTextsHaveMaximumLengths()
    {
        CollectionAssert.AreEqual(new[] { "Comment" }, InvalidMembers(ExistingCustomer().AsFormData().With(f => f.Comment = new string('c', 51))));
        CollectionAssert.AreEqual(new[] { "TrackOrArea" }, InvalidMembers(ExistingCustomer().AsFormData().With(f => f.TrackOrArea = new string('t', 21))));
    }

    [TestMethod]
    public void AColourMustBeOneOfThePalette()
    {
        CollectionAssert.AreEqual(new[] { "TrackOrAreaColor" }, InvalidMembers(ExistingCustomer().AsFormData().With(f => f.TrackOrAreaColor = "#FA0000")));
        CollectionAssert.AreEqual(new[] { "TrackOrAreaColor" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.TrackOrAreaColor = "red")));
    }

    [TestMethod]
    [DataRow(nameof(StationCustomerCargoFormData.CargoId))]
    [DataRow(nameof(StationCustomerCargoFormData.OperatingDayId))]
    [DataRow(nameof(StationCustomerCargoFormData.DirectionId))]
    [DataRow(nameof(StationCustomerCargoFormData.QuantityUnitId))]
    public void ListsMustBeSelected(string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(ExistingCargo().AsFormData().With(f => typeof(StationCustomerCargoFormData).GetProperty(member)!.SetValue(f, 0))));

    [TestMethod]
    public void PackagingUnitReadyTimeAndMaxTrainsetLengthAreOptional() =>
        Assert.IsEmpty(InvalidMembers(ExistingCargo().AsFormData().With(f => { f.PackageUnitId = 0; f.ReadyTimeId = 0; f.MaxTrainsetLength = null; })));

    [TestMethod]
    [DataRow(0)]
    [DataRow(1001)]
    public void TheQuantityIsFromOneToAThousand(int quantity) =>
        CollectionAssert.AreEqual(new[] { "Quantity" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.Quantity = quantity)));

    [TestMethod]
    public void TheMaxTrainsetLengthIsAtMostAHundredWagons() =>
        CollectionAssert.AreEqual(new[] { "MaxTrainsetLength" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.MaxTrainsetLength = 101)));

    [TestMethod]
    public void TheOtherFieldsHaveMaximumLengths()
    {
        CollectionAssert.AreEqual(new[] { "SpecificWagonClass" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.SpecificWagonClass = new string('G', 11))));
        CollectionAssert.AreEqual(new[] { "TrackOrArea" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.TrackOrArea = new string('t', 21))));
    }

    private static void AssertSameValues<T>(T expected, T actual)
    {
        foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite && p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}