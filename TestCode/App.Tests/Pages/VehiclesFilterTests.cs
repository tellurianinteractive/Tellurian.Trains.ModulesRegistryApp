using ModulesRegistry.Data;
using ModulesRegistry.Pages.Vehicles;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The vehicle lists' filter from the query string (Migration step 3.3), which replaced the filter kept in local storage.
/// </summary>
[TestClass]
public sealed class VehiclesFilterTests
{
    private static readonly Vehicle Rc = new() { Id = 1, KeeperSignature = "SJ", VehicleClass = "Rc6", VehicleNumber = "1323", ModelManufacturerName = "Roco", ModelNumber = "73400", OwningPerson = new Person { FirstName = "Anna", LastName = "Svensson" } };
    private static readonly Vehicle Br = new() { Id = 2, KeeperSignature = "DB", VehicleClass = "BR 216", VehicleNumber = "001", ModelManufacturerName = "Märklin", ModelNumber = "37771", OwningPerson = new Person { FirstName = "Bernd", LastName = "Müller" } };
    private static readonly Vehicle NoOwner = new() { Id = 3, KeeperSignature = "SJ", VehicleClass = "Z65", VehicleNumber = "1", ModelManufacturerName = "Heljan", ModelNumber = "1" };
    private static readonly Vehicle[] All = [Rc, Br, NoOwner];

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new VehiclesFilter { OwnerName = " ", PrototypeInfo = "", ModelInfo = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
    }

    [TestMethod]
    public void EachFieldMatchesPartOfItsValueIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Rc, NoOwner }, new VehiclesFilter { PrototypeInfo = "sj" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Br }, new VehiclesFilter { ModelInfo = "MÄRKLIN" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Br }, new VehiclesFilter { OwnerName = "müller" }.Apply(All).ToArray());
    }

    [TestMethod]
    public void VehicleWithoutOwnerDoesNotMatchAnOwnerFilter()
    {
        Assert.IsEmpty(new VehiclesFilter { OwnerName = "a", ModelInfo = "Heljan" }.Apply(All));
    }

    [TestMethod]
    public void ValuesAreTrimmedAndAllMustMatch()
    {
        CollectionAssert.AreEqual(new[] { Rc }, new VehiclesFilter { PrototypeInfo = " rc6 ", ModelInfo = "roco" }.Apply(All).ToArray());
        Assert.IsEmpty(new VehiclesFilter { PrototypeInfo = "Rc6", ModelInfo = "Märklin" }.Apply(All));
    }

    [TestMethod]
    public void TextShowsTheGivenValues()
    {
        Assert.AreEqual("Anna Rc6", new VehiclesFilter { OwnerName = "Anna", PrototypeInfo = "Rc6", ModelInfo = " " }.ToString());
    }
}