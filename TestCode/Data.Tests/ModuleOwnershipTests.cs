using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rationals;

namespace ModulesRegistry.Data.Tests;

[TestClass]
public class ModuleOwnershipTestsKD
{
    [TestMethod]
    public void FullTransferOfOwnership()
    {
        var original = OriginalFullOwnership;
        var transfer = new ModuleOwnershipTransfer(original.PersonId!.Value, 20, 1);
        var result = OriginalFullOwnership.Transfer(transfer);
        AssertModuleId(result);
        Assert.AreEqual(0, result.From.OwnedShare);
        Assert.AreEqual(1, result.To.OwnedShare);
        Assert.AreEqual(20, result.To.PersonId);
    }

    [TestMethod]
    public void Only25PercentTransferOfOwnership()
    {
        var original = OriginalFullOwnership;
        var transfer = new ModuleOwnershipTransfer(original.PersonId!.Value, 20, .25);
        var result = original.Transfer(transfer);
        AssertModuleId(result);
        Assert.AreEqual(.75, result.From.OwnedShare);
        Assert.AreEqual(.25, result.To.OwnedShare);
        Assert.AreEqual(20, result.To.PersonId);
    }

    [TestMethod]
    public void AddAssistantOnly()
    {
        var original = OriginalFullOwnership;
        var transfer = new ModuleOwnershipTransfer(original.PersonId!.Value, 20, 0);
        var result = original.Transfer(transfer);
        AssertModuleId(result);
        Assert.AreEqual(1, result.From.OwnedShare);
        Assert.AreEqual(0, result.To.OwnedShare);
        Assert.AreEqual(20, result.To.PersonId);
        Assert.IsTrue(result.To.IsAssistantOnly());
    }

    [TestMethod]
    public void TransferFractioneShare()
    {
        var original = new ModuleOwnership() { PersonId = 9, ModuleId = 55, OwnedShare = 0.25 };
        var transfer = new ModuleOwnershipTransfer(original.PersonId!.Value, 20, 0.25);
        var result = original.Transfer(transfer);
        AssertModuleId(result); 
        Assert.AreEqual(0, result.From.OwnedShare);
        Assert.AreEqual(.25, result.To.OwnedShare);
        Assert.AreEqual(20, result.To.PersonId);
    }

    [TestMethod]
    public void TransferToLargeShare()
    {
        var original = new ModuleOwnership() { PersonId = 9, ModuleId = 55, OwnedShare = 0.25 };
        var transfer = new ModuleOwnershipTransfer(original.PersonId!.Value, 20, .50);
        var result = original.Transfer(transfer);
        AssertModuleId(result);
        Assert.AreEqual(0, result.From.OwnedShare);
        Assert.AreEqual(.25, result.To.OwnedShare);
        Assert.AreEqual(20, result.To.PersonId);
    }

    [TestMethod]
    public void GiveOwnershipPartBackToOneOwner()
    {
        var original = new ModuleOwnership() { PersonId = 9, ModuleId = 55, OwnedShare = 0.25 };
        var transfer = new ModuleOwnershipTransfer(20, original.PersonId!.Value, .50);
        var result = original.Transfer(transfer);
        AssertModuleId(result);
        Assert.AreEqual(0.75, result.From.OwnedShare);
        Assert.AreEqual(0, result.To.OwnedShare);
        Assert.AreEqual(original.PersonId, result.From.PersonId);
        Assert.AreEqual(transfer.NewOwnerRef.PersonId, result.To.PersonId);

    }

    private static void AssertModuleId((ModuleOwnership from, ModuleOwnership to) transfer)
    {
        Assert.IsFalse(transfer.from.ModuleId == 0);
        Assert.IsTrue(transfer.to.ModuleId == transfer.from.ModuleId);

    }

    private static ModuleOwnership OriginalFullOwnership =>
        new()
        {
            Id = 999,
            PersonId = 10,
            ModuleId = 11,
            OwnedShare = 1,
        };
}
