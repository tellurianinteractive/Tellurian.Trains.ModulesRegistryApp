using ModulesRegistry.Extensions;

namespace ModulesRegistry.AppTests;

/// <summary>
/// The back address in <c>?back=</c>: only an address within the app is used, and it is added to an address with or without a query.
/// </summary>
[TestClass]
public sealed class BackHrefExtensionsTests
{
    [TestMethod]
    [DataRow("Modules/My", "Modules/My")]
    [DataRow("Modules/Index?owner=Berg&scale=1", "Modules/Index?owner=Berg&scale=1")]
    [DataRow(null, null)]
    [DataRow("", null)]
    [DataRow("/Modules/My", null)]
    [DataRow("//evil.example", null)]
    [DataRow("\\\\evil.example", null)]
    [DataRow("https://evil.example", null)]
    [DataRow("javascript:alert(1)", null)]
    public void OnlyAnAddressWithinTheAppIsUsed(string? back, string? expected) => Assert.AreEqual(expected, back.AsLocalHref);

    [TestMethod]
    public void TheBackAddressIsAddedToTheQuery()
    {
        Assert.AreEqual("Modules/1/Edit/PersonOwned/2?back=Modules%2FMy", "Modules/1/Edit/PersonOwned/2".WithBack("Modules/My"));
        Assert.AreEqual("Modules/1/Edit?back=x&deleteExit=5", "Modules/1/Edit?back=x".WithQuery("deleteExit", "5"));
        Assert.AreEqual("Modules/1/Edit", "Modules/1/Edit".WithBack(null));
    }
}