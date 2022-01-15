using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Extensions;
using System.Security.Claims;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class ModuleOwnerRefTests
{
    [TestMethod]
    public void IsPerson()
    {
        var target = ModuleOwnershipRef.Person(10);
        Assert.IsTrue(target.IsAny);
        Assert.IsTrue(target.IsPerson);
        Assert.IsFalse(target.IsGroup);
        Assert.IsFalse(target.IsPersonInGroup);
        Assert.IsFalse(target.IsNone);
        Assert.AreEqual(10, target.PersonId);
        Assert.AreEqual(0, target.GroupId);
    }

    [TestMethod]
    public void IsGroup()
    {
        var target = ModuleOwnershipRef.Group(10);
        Assert.IsTrue(target.IsAny);
        Assert.IsFalse(target.IsPerson);
        Assert.IsTrue(target.IsGroup);
        Assert.IsFalse(target.IsPersonInGroup);
        Assert.IsFalse(target.IsNone);
        Assert.AreEqual(0, target.PersonId);
        Assert.AreEqual(10, target.GroupId);
    }

    [TestMethod]
    public void IsNone()
    {
        var target = ModuleOwnershipRef.None;
        Assert.IsFalse(target.IsAny);
        Assert.IsFalse(target.IsPerson);
        Assert.IsFalse(target.IsGroup);
        Assert.IsFalse(target.IsPersonInGroup);
        Assert.IsTrue(target.IsNone);
        Assert.AreEqual(0, target.PersonId);
        Assert.AreEqual(0, target.GroupId);
    }

    [TestMethod]
    public void IsPersonInGroup()
    {
        var target = ModuleOwnershipRef.PersonInGroup(10, 11);
        Assert.IsTrue(target.IsAny);
        Assert.IsFalse(target.IsPerson);
        Assert.IsFalse(target.IsGroup);
        Assert.IsTrue(target.IsPersonInGroup);
        Assert.IsFalse(target.IsNone);
        Assert.AreEqual(10, target.PersonId);
        Assert.AreEqual(11, target.GroupId);
    }

    [TestMethod]
    public void IsPersonWithClaimsPrincipalOnly()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(AppClaimTypes.PersonId, "11") }));
        var target = principal.AsModuleOwnershipRef();
        Assert.IsTrue(target.IsAny);
        Assert.IsTrue(target.IsPerson);
        Assert.IsFalse(target.IsGroup);
        Assert.IsFalse(target.IsPersonInGroup);
        Assert.IsFalse(target.IsNone);
        Assert.AreEqual(11, target.PersonId);
        Assert.AreEqual(0, target.GroupId);
    }

    [TestMethod]
    public void IsGroupWithClaimsPrincipal()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(AppClaimTypes.PersonId, "11") }));
        var target = ModuleOwnershipRef.Group(10);
        target = principal.UpdateFrom(target);
        Assert.IsTrue(target.IsAny);
        Assert.IsFalse(target.IsPerson);
        Assert.IsTrue(target.IsGroup);
        Assert.IsFalse(target.IsPersonInGroup);
        Assert.IsFalse(target.IsNone);
        Assert.AreEqual(0, target.PersonId);
        Assert.AreEqual(10, target.GroupId);
    }
}
