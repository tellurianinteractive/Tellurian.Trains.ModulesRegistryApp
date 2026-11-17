using ModulesRegistry.Data;
using ModulesRegistry.Pages.WiFredThrottles;

namespace ModulesRegistry.AppTests.Pages;

/// <summary>
/// The throttles list's filter from the query string (Migration step 3.3). It matches the start of the values, as before.
/// </summary>
[TestClass]
public sealed class ThrottlesFilterTests
{
    private static readonly WiFredThrottle Anna = new() { Id = 1, MacAddress = "AB:CD:01", Name = "Anna 1", OwningPerson = new Person { FirstName = "Anna", LastName = "Svensson" } };
    private static readonly WiFredThrottle Bernd = new() { Id = 2, MacAddress = "ab:ef:02", Name = "Bernd 1", OwningPerson = new Person { FirstName = "Bernd", LastName = "Sander" } };
    private static readonly WiFredThrottle NoOwner = new() { Id = 3, MacAddress = "CD:00:03", Name = "Spare" };
    private static readonly WiFredThrottle[] All = [Anna, Bernd, NoOwner];

    [TestMethod]
    public void NoFilterGivesAll()
    {
        var filter = new ThrottlesFilter { MacAddress = " ", OwnerName = null };

        Assert.IsFalse(filter.HasFilter);
        CollectionAssert.AreEqual(All, filter.Apply(All).ToArray());
    }

    [TestMethod]
    public void MacAddressMatchesItsStartIgnoringCase()
    {
        CollectionAssert.AreEqual(new[] { Anna, Bernd }, new ThrottlesFilter { MacAddress = "ab:" }.Apply(All).ToArray());
        Assert.IsEmpty(new ThrottlesFilter { MacAddress = "01" }.Apply(All));
    }

    [TestMethod]
    public void OwnerMatchesTheStartOfTheFirstOrLastName()
    {
        CollectionAssert.AreEqual(new[] { Anna, Bernd }, new ThrottlesFilter { OwnerName = "s" }.Apply(All).ToArray());
        CollectionAssert.AreEqual(new[] { Bernd }, new ThrottlesFilter { OwnerName = "BERND" }.Apply(All).ToArray());
        Assert.IsEmpty(new ThrottlesFilter { OwnerName = "nna" }.Apply(All));
    }

    [TestMethod]
    public void ThrottleWithoutOwnerDoesNotMatchAnOwnerFilter()
    {
        Assert.IsEmpty(new ThrottlesFilter { MacAddress = "CD", OwnerName = "a" }.Apply(All));
    }

    [TestMethod]
    public void BothMustMatch()
    {
        CollectionAssert.AreEqual(new[] { Anna }, new ThrottlesFilter { MacAddress = " ab ", OwnerName = "anna" }.Apply(All).ToArray());
        Assert.IsEmpty(new ThrottlesFilter { MacAddress = "CD", OwnerName = "Anna" }.Apply(All));
    }
}