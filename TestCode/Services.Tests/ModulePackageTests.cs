using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Data;
using ModulesRegistry.Services.Extensions;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class ModulePackageTests
{
    // The registration page posts a package's id and treats 0 as no package chosen, so no package may have id 0.
    [TestMethod]
    public void PackageIdsArePositive()
    {
        AvailableModule[] modules =
        [
            Module(1, "Alpha", null),
            Module(2, "Beta", "Package"),
            Module(3, "Gamma", "Package"),
        ];
        var packages = modules.AsModulePackages().ToArray();
        Assert.HasCount(2, packages);
        Assert.IsTrue(packages.All(p => p.Id > 0));
        Assert.AreEqual(packages.Length, packages.Select(p => p.Id).Distinct().Count());
    }

    static AvailableModule Module(int id, string name, string? packageLabel) =>
        new() { ModuleId = id, FullName = name, ScaleId = 1, PackageLabel = packageLabel, FirstName = "", LastName = "", CityName = "" };
}