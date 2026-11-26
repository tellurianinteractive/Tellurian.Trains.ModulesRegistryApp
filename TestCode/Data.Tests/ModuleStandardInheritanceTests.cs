namespace ModulesRegistry.Data.Tests;

/// <summary>
/// A value a standard doesn't have itself comes from the standard it extends, directly or through others (2026-11-26).
/// </summary>
[TestClass]
public sealed class ModuleStandardInheritanceTests
{
    private static readonly ModuleStandard Base = new() { Id = 2, ShortName = "FREMO/H0-RE", TrackSystem = "Code 75-83", PreferredTheme = "Epoch III" };
    private static readonly ModuleStandard Extension = new() { Id = 10, ShortName = "FREMO/QS", ExtendsStandardId = 2, PreferredTheme = "  " };
    private static readonly ModuleStandard ExtensionOfExtension = new() { Id = 11, ShortName = "QS-X", ExtendsStandardId = 10, TrackSystem = "Code 70" };
    private static readonly Dictionary<int, ModuleStandard> ById = new[] { Base, Extension, ExtensionOfExtension }.ToDictionary(ms => ms.Id);

    [TestMethod]
    public void TheOwnValueComesFromItself() =>
        Assert.AreEqual(("Code 70", ExtensionOfExtension), ExtensionOfExtension.InheritedValue(ById, ms => ms.TrackSystem));

    [TestMethod]
    public void AnEmptyValueComesFromTheExtendedStandard() =>
        Assert.AreEqual(("Epoch III", Base), Extension.InheritedValue(ById, ms => ms.PreferredTheme));

    [TestMethod]
    public void AValueIsInheritedThroughOtherStandards() =>
        Assert.AreEqual(("Epoch III", Base), ExtensionOfExtension.InheritedValue(ById, ms => ms.PreferredTheme));

    [TestMethod]
    public void NoValueAnywhereIsNone() =>
        Assert.AreEqual((null, null), ExtensionOfExtension.InheritedValue(ById, _ => null));

    [TestMethod]
    public void ALoopEndsTheSearch()
    {
        var a = new ModuleStandard { Id = 1, ExtendsStandardId = 3 };
        var b = new ModuleStandard { Id = 3, ExtendsStandardId = 1 };
        Assert.AreEqual((null, null), a.InheritedValue(new Dictionary<int, ModuleStandard> { [1] = a, [3] = b }, _ => null));
    }
}