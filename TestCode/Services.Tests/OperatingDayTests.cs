using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Extensions;
using System.Globalization;

namespace ModulesRegistry.Services.Tests;

[TestClass]
public class OperatingDayTests
{
    [TestMethod]
    public void DaysIsConsectutive()
    {
        const byte flags = 0b_0001_1111; // M-F
        Assert.IsTrue(flags.GetDays().IsConsectutive());
    }

    [TestMethod]
    public void DaysWithSundayFirstIsConsectutive()
    {
        const byte flags = 0b_0101_1111; //M-F,S
        Assert.IsTrue(flags.GetDays(true).IsConsectutive(true));
    }

    [TestMethod]
    public void SundayToThuesdayWorks()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        const byte flags = 0b_0100_0011;
        var days = flags.GetDays(true);
        Assert.AreEqual(7, days[0].Number);
        Assert.IsTrue(days.IsConsectutive(true));
        Assert.AreEqual("Su-Tu", flags.OperationDays(true).ShortName);
    }

    [TestMethod]
    public void OnDemandWorks()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        const byte flags = 0b_1111_1111;
        var days = flags.GetDays(true);
        Assert.AreEqual(1, days.Length);
        Assert.AreEqual(0, days[0].Number);
        Assert.AreEqual("On demand", flags.OperationDays(true).ShortName);

    }
}
