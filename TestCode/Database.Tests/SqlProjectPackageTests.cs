namespace ModulesRegistry.Database.Tests;

[TestClass]
public class SqlProjectPackageTests
{
    [TestMethod]
    public void ProjectHasBuildFiles()
    {
        var files = SqlProjectPackage.BuildFiles(SqlProjectPackage.ProjectFile());
        Assert.IsGreaterThan(50, files.Count);
        Assert.IsTrue(files.All(File.Exists), "Every Build item of the project exists.");
    }
}