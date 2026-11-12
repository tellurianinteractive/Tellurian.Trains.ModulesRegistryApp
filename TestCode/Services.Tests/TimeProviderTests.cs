namespace ModulesRegistry.Services.Tests;

[TestClass]
public class TimeProviderTests
{
    private static readonly TimeSpan Tolerance = TimeSpan.FromSeconds(2);

    [TestMethod]
    public void TimeNowWorks()
    {
        var timeProvider = new SystemTimeProvider();
        var expected = DateTimeOffset.Now;
        var actual = timeProvider.Now;
        Assert.IsLessThan(Tolerance, (actual - expected).Duration());
    }

    /// <summary>
    /// Local time is Central European time by default, independent of the server's time zone
    /// (the App Service and the CI runner both run in UTC).
    /// </summary>
    [TestMethod]
    public void LocalTimeIsCentralEuropeanTimeByDefault()
    {
        var timeProvider = new SystemTimeProvider();
        var expected = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"));
        var actual = timeProvider.LocalTime;
        Assert.IsLessThan(Tolerance, (actual - expected).Duration());
    }

    [TestMethod]
    public void LocalTimeUsesGivenTimeZone()
    {
        var timeProvider = new SystemTimeProvider(TimeZoneInfo.Utc);
        var expected = DateTime.UtcNow;
        var actual = timeProvider.LocalTime;
        Assert.IsLessThan(Tolerance, (actual - expected).Duration());
    }
}