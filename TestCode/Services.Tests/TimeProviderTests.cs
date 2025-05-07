using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class TimeProviderTests
{
    [TestMethod]
    public void TimeNowWorks()
    {
        var timeProvider = new SystemTimeProvider();
        var expected = DateTimeOffset.Now;
        var actual = timeProvider.Now;
        Assert.AreEqual(expected.ToString(), actual.ToString());
    }

    [TestMethod]
    public void LocalTimeWorks()
    {
        var timeProvider = new SystemTimeProvider();
        var expected = DateTimeOffset.Now.LocalDateTime;
        var actual = timeProvider.LocalTime;
        Assert.AreEqual(expected.ToString(), actual.ToString());
    }
}
