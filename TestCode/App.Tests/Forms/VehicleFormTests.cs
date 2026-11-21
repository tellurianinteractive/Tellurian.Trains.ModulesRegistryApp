using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Vehicles;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The vehicle form model (Migration step 3.6): the mapping to and from the entity, and its DataAnnotations run as the server runs them.
/// </summary>
[TestClass]
public sealed class VehicleFormTests
{
    private static Vehicle Existing() => new()
    {
        Id = 7, OwningPersonId = 3, VehicleOperatorId = 2, VehicleInteroperabilityCode = 11, ModelImageHref = "model.png",
        KeeperCountryId = 1, KeeperSignature = "SJ", VehicleClass = "Rc6", VehicleNumber = "1323", Theme = "SJ", PrototypeManufacturerName = "ASEA",
        PrototypeManufactureYear = 1985, TractionFeatureId = 204, PrototypeLength = 15.52m, PrototypeWeight = 78m, EnginePower = 3600,
        PrototypeImageHref = "https://example.com/rc6.jpg", ThisEmbodiementFromYear = 1985, ThisEmbodiementUptoYear = 2010,
        InventoryNumber = 12, ScaleId = 3, ModelManufacturerName = "Roco", ModelNumber = "73412", CouplingFeatureId = 101, WheelsFeatureId = 2,
        DecoderType = "ESU LokSound 5", DccAddress = 1323, HasSound = true, HasRemoteCouplings = false, IsWeathered = true, Note = "Röd",
        F0 = "Light", F1 = "Sound", F29 = "Horn",
    };

    private static VehicleFormData ValidForm() => Existing().AsFormData();

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());

        foreach (var property in typeof(Vehicle).GetProperties().Where(p => p.CanWrite && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void EditKeepsWhatTheFormDoesNotOwn()
    {
        var existing = Existing();
        var form = ValidForm();
        form.VehicleNumber = "1324";

        Assert.AreSame(existing, form.AsEntity(existing, owningPersonId: 99));
        Assert.AreEqual(7, existing.Id);
        Assert.AreEqual(3, existing.OwningPersonId);
        Assert.AreEqual(2, existing.VehicleOperatorId);
        Assert.AreEqual("1324", existing.VehicleNumber);
    }

    [TestMethod]
    public void CreateStartsNewForTheGivenOwner()
    {
        var created = ValidForm().AsEntity(owningPersonId: 5);
        Assert.AreEqual(0, created.Id);
        Assert.AreEqual(5, created.OwningPersonId);
    }

    [TestMethod]
    public void ValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(ValidForm()));

    [TestMethod]
    [DataRow(nameof(VehicleFormData.KeeperSignature))]
    [DataRow(nameof(VehicleFormData.VehicleClass))]
    [DataRow(nameof(VehicleFormData.VehicleNumber))]
    [DataRow(nameof(VehicleFormData.Theme))]
    [DataRow(nameof(VehicleFormData.ModelManufacturerName))]
    public void RequiredTextsMustBeGiven(string member) => AssertInvalid(f => typeof(VehicleFormData).GetProperty(member)!.SetValue(f, ""), member);

    [TestMethod]
    public void OptionalFieldsMayBeEmpty() => Assert.IsEmpty(InvalidMembers(Change(f =>
    {
        f.KeeperCountryId = null; f.PrototypeManufacturerName = null; f.PrototypeManufactureYear = null; f.TractionFeatureId = null;
        f.PrototypeLength = null; f.PrototypeWeight = null; f.EnginePower = null; f.PrototypeImageHref = null;
        f.ThisEmbodiementFromYear = null; f.ThisEmbodiementUptoYear = null; f.ModelNumber = null; f.CouplingFeatureId = null;
        f.WheelsFeatureId = null; f.DecoderType = null; f.DccAddress = null; f.Note = null; f.F0 = null; f.F29 = null;
    })));

    [TestMethod]
    public void TheScaleMustBeSelected() => AssertInvalid(f => f.ScaleId = 0, nameof(VehicleFormData.ScaleId));

    [TestMethod]
    [DataRow(0)]
    [DataRow(10000)]
    public void InventoryNumberIsFrom1To9999(int number) => AssertInvalid(f => f.InventoryNumber = number, nameof(VehicleFormData.InventoryNumber));

    [TestMethod]
    public void AManufactureYearMayBeBeforeOtherYears()
    {
        Assert.IsEmpty(InvalidMembers(Change(f => f.PrototypeManufactureYear = YearPeriod.MinVehicleYear)));
        AssertInvalid(f => f.PrototypeManufactureYear = YearPeriod.MinVehicleYear - 1, nameof(VehicleFormData.PrototypeManufactureYear));
    }

    [TestMethod]
    public void LengthWeightAndPowerHaveRanges()
    {
        AssertInvalid(f => f.PrototypeLength = 100.5m, nameof(VehicleFormData.PrototypeLength));
        AssertInvalid(f => f.PrototypeWeight = -1m, nameof(VehicleFormData.PrototypeWeight));
        AssertInvalid(f => f.EnginePower = 10001, nameof(VehicleFormData.EnginePower));
    }

    [TestMethod]
    public void FunctionNamesHaveAMaximumLength() => AssertInvalid(f => f.F12 = new string('a', 13), nameof(VehicleFormData.F12));

    [TestMethod]
    public void TextsAreOrdinaryText() => AssertInvalid(f => f.Note = "Локомотив", nameof(VehicleFormData.Note));

    [TestMethod]
    public void ADccAddressOutsideTheRangeIsInvalid() => AssertInvalid(f => f.DccAddress = 10000, nameof(VehicleFormData.DccAddress));

    private static VehicleFormData Change(Action<VehicleFormData> change)
    {
        var form = ValidForm();
        change(form);
        return form;
    }

    private static void AssertInvalid(Action<VehicleFormData> change, string member) =>
        CollectionAssert.AreEqual(new[] { member }, InvalidMembers(Change(change)));

    private static string[] InvalidMembers(VehicleFormData form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}