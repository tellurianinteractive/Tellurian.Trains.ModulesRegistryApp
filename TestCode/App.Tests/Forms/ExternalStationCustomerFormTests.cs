using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.ExternalStationCustomers;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The external station customer form and its cargo flow form (Migration step 3.6): the mapping to and from the entities, and
/// their DataAnnotations run as the server runs them.
/// </summary>
[TestClass]
public sealed class ExternalStationCustomerFormTests
{
    private static ExternalStationCustomer ExistingCustomer() => new()
    {
        Id = 7, ExternalStationId = 3, CustomerName = "Stora Enso", OpenedYear = 1950, ClosedYear = 1995,
        ExternalStationCustomerCargos = [new ExternalStationCustomerCargo { Id = 12, ExternalStationCustomerId = 7, CargoId = 5 }],
    };

    private static ExternalStationCustomerCargo ExistingCargo() => new()
    {
        Id = 12, ExternalStationCustomerId = 7, CargoId = 5, PackageUnitId = 0, OperatingDayId = 8, DirectionId = 2, Quantity = 4,
        QuantityUnitId = 4, SpecificWagonClass = "Gbs", SpecialCargoName = "Paper rolls", FromYear = 1960, UptoYear = 1990,
    };

    [TestMethod]
    public void CustomerRoundTripChangesNothing() => AssertSameValues(ExistingCustomer(), ExistingCustomer().AsFormData().AsEntity(ExistingCustomer()));

    [TestMethod]
    public void CargoRoundTripChangesNothing() => AssertSameValues(ExistingCargo(), ExistingCargo().AsFormData().AsEntity(ExistingCargo()));

    [TestMethod]
    public void AnEditedCustomerKeepsItsStationAndCargoFlows()
    {
        var existing = ExistingCustomer();
        var form = existing.AsFormData();
        form.CustomerName = "Holmen";

        var saved = form.AsEntity(existing, stationId: 99);

        Assert.AreSame(existing, saved);
        Assert.AreEqual(3, saved.ExternalStationId, "The station of a saved customer isn't changed by the form.");
        Assert.HasCount(1, saved.ExternalStationCustomerCargos);
    }

    [TestMethod]
    public void NewOnesBelongToTheStationAndCustomerOfTheAddress()
    {
        Assert.AreEqual(3, ExistingCustomer().AsFormData().AsEntity(stationId: 3).ExternalStationId);
        Assert.AreEqual(7, ExistingCargo().AsFormData().AsEntity(customerId: 7).ExternalStationCustomerId);
        Assert.AreEqual(0, ExistingCargo().AsFormData().AsEntity(customerId: 7).Id);
    }

    [TestMethod]
    public void ValidFormsHaveNoErrors()
    {
        Assert.IsEmpty(InvalidMembers(ExistingCustomer().AsFormData()));
        Assert.IsEmpty(InvalidMembers(ExistingCargo().AsFormData()));
        Assert.IsEmpty(InvalidMembers(ExternalStationCustomerCargo.Default(7).AsFormData().With(f => { f.CargoId = 5; f.Quantity = 1; })), "A new flow with a cargo type and a quantity.");
    }

    [TestMethod]
    public void TheCustomerNameMustBeGiven() => CollectionAssert.AreEqual(new[] { "CustomerName" }, InvalidMembers(ExistingCustomer().AsFormData().With(f => f.CustomerName = "")));

    [TestMethod]
    [DataRow(nameof(ExternalStationCustomerCargoFormData.CargoId))]
    [DataRow(nameof(ExternalStationCustomerCargoFormData.OperatingDayId))]
    [DataRow(nameof(ExternalStationCustomerCargoFormData.DirectionId))]
    [DataRow(nameof(ExternalStationCustomerCargoFormData.QuantityUnitId))]
    public void ListsMustBeSelected(string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(ExistingCargo().AsFormData().With(f => typeof(ExternalStationCustomerCargoFormData).GetProperty(member)!.SetValue(f, 0))));

    [TestMethod]
    public void ThePackagingUnitIsOptional() => Assert.IsEmpty(InvalidMembers(ExistingCargo().AsFormData().With(f => f.PackageUnitId = 0)));

    [TestMethod]
    [DataRow(0)]
    [DataRow(1001)]
    public void TheQuantityIsFromOneToAThousand(int quantity) =>
        CollectionAssert.AreEqual(new[] { "Quantity" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.Quantity = quantity)));

    [TestMethod]
    public void TheOtherFieldsHaveMaximumLengths()
    {
        CollectionAssert.AreEqual(new[] { "SpecificWagonClass" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.SpecificWagonClass = new string('G', 11))));
        CollectionAssert.AreEqual(new[] { "SpecialCargoName" }, InvalidMembers(ExistingCargo().AsFormData().With(f => f.SpecialCargoName = new string('p', 21))));
    }

    [TestMethod]
    public void TheOtherFieldsMayBeEmpty() => Assert.IsEmpty(InvalidMembers(ExistingCargo().AsFormData().With(f => { f.SpecificWagonClass = null; f.SpecialCargoName = null; })));

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

internal static class FormTestExtensions
{
    public static T With<T>(this T form, Action<T> change)
    {
        change(form);
        return form;
    }
}
