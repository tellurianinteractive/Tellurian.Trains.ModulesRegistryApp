using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.StationCustomers;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// A freight customer's waybill form (Migration step 3.6): only the print settings are edited; the rest of the waybill stays.
/// </summary>
[TestClass]
public sealed class StationCustomerWaybillFormTests
{
    private static StationCustomerWaybill Existing() => new()
    {
        Id = 20, StationCustomerId = 7, StationCustomerCargoId = 13, OtherStationCustomerCargoId = 14, OtherRegionId = 3, OperatingDayId = 8,
        SequenceNumber = 2, IsManuallyCreated = false, HasEmptyReturn = true, HasSameCargoReturn = false, HideLoadingTimes = true,
        PrintPerOperatingDay = true, PrintCount = 2,
    };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in typeof(StationCustomerWaybill).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void OnlyThePrintSettingsChange()
    {
        var form = Existing().AsFormData();
        form.PrintCount = 5;
        form.HasSameCargoReturn = true;

        var saved = form.AsEntity(Existing());

        Assert.AreEqual(5, saved.PrintCount);
        Assert.IsTrue(saved.HasSameCargoReturn);
        Assert.AreEqual(13, saved.StationCustomerCargoId);
        Assert.AreEqual(14, saved.OtherStationCustomerCargoId);
        Assert.IsTrue(saved.HideLoadingTimes, "Not a field of the form.");
    }

    [TestMethod]
    public void AValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(Existing().AsFormData()));

    [TestMethod]
    public void TheDaysMustBeSelected() => CollectionAssert.AreEqual(new[] { "OperatingDayId" }, InvalidMembers(Existing().AsFormData().With(f => f.OperatingDayId = 0)));

    [TestMethod]
    [DataRow(-1)]
    [DataRow(101)]
    public void ThePrintCountIsFromZeroToAHundred(int count) =>
        CollectionAssert.AreEqual(new[] { "PrintCount" }, InvalidMembers(Existing().AsFormData().With(f => f.PrintCount = count)));

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}