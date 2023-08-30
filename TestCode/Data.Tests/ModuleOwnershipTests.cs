using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rationals;

namespace ModulesRegistry.Data.Tests;

[TestClass]
public class ModuleOwnershipTests
{
    [TestMethod]
    public void FullTransferOfOwnership()
    {
        var full = Rational.One;
        var newOwnership = new ModuleOwnership() { PersonId = 20, OwnedShare = (double)full};
        var result = OriginalFullOwnership.TransferTo(newOwnership);
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(0, result[0].OwnedShare);
        Assert.AreEqual(1, result[1].OwnedShare);
        Assert.AreEqual(20, result[1].PersonId);
        Assert.AreEqual(11, result[0].ModuleId);
        Assert.AreEqual(11, result[1].ModuleId);
    }

    [TestMethod]
    public void Only25PercentTransferOfOwnership()
    {
        var part = new Rational(1, 4);
        var newOwnership = new ModuleOwnership() { PersonId = 20, OwnedShare=(double)part };
        var result = OriginalFullOwnership.TransferTo(newOwnership);
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(0.75, result[0].OwnedShare, 0.001);
        Assert.AreEqual(0.25, result[1].OwnedShare, 0.001);
        Assert.AreEqual(20, result[1].PersonId);
    }

    [TestMethod] 
    public void AddAssistantOnly()
    {
        var none = Rational.Zero;
        var newOwnership = new ModuleOwnership() { PersonId = 20, OwnedShare = (double)none };
        var result = OriginalFullOwnership.TransferTo(newOwnership);
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(1, result[0].OwnedShare);
        Assert.AreEqual(0, result[1].OwnedShare);
        Assert.AreEqual(20, result[1].PersonId);
        Assert.IsTrue(result[1].IsAssistantOnly());
    }

    [TestMethod]
    public void TransferFractioneShare()
    {
        var partialOwnership = new ModuleOwnership() { PersonId = 9, ModuleId = 55, OwnedShare = 0.25 };
        var result = partialOwnership.TransferTo(new ModuleOwnership() { PersonId = 10, OwnedShare=0.25});
        Assert.AreEqual(2, result.Length);
        Assert.AreEqual(0, result.First().OwnedShare);
        Assert.AreEqual(0.25, result.Last().OwnedShare);
    }

    [TestMethod]
    public void TransferToLargeShare()
    {
        var partialOwnership = new ModuleOwnership() { PersonId = 9, ModuleId = 55, OwnedShare = 0.25 };
        var result = partialOwnership.TransferTo(new ModuleOwnership() { PersonId = 10, OwnedShare = 0.50 });
        Assert.AreEqual(1, result.Length);
    }

    private static ModuleOwnership OriginalFullOwnership =>
        new()
        {
            PersonId = 10,
            ModuleId = 11,
            OwnedShare = 1,
        };
}
