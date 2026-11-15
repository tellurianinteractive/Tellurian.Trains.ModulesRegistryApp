using System.Reflection;

namespace ModulesRegistry.AppTests;

/// <summary>
/// The version in the menu comes from the assembly version. It must follow <c>&lt;Version&gt;</c> in Directory.Build.props;
/// release 1.14.12 showed 1.14.11 because a separate <c>&lt;AssemblyVersion&gt;</c> wasn't stepped up.
/// </summary>
[TestClass]
public sealed class AppVersionTests
{
    [TestMethod]
    public void ShownVersionIsTheReleaseVersion()
    {
        var assembly = typeof(AppService).Assembly;
        var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
        var releaseVersion = informationalVersion.Split('+')[0];

        Assert.AreEqual(releaseVersion, new AppService().AppVersion);
    }
}