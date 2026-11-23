using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModulesRegistry.Services.Extensions;
using ModulesRegistry.Services.Models;

namespace ModulesRegistry.Services.Tests;

/// <summary>
/// The days printed on a waybill: the sender's days; a return with the same cargo is sent on its own sender's days
/// (the outgoing waybill's receiver), an empty return daily.
/// </summary>
[TestClass]
public class WaybillDaysTests
{
    private const byte Monday = 0x01, MondayToFriday = 0x1F, Weekend = 0x60;

    private static Waybill Waybill(byte senderDays, byte receiverDays, bool emptyReturn = false, bool sameCargoReturn = false) =>
        new(new CargoCustomer { Languages = "en", StationName = "Berga", OperationDaysFlags = senderDays, QuantityUnitResourceKey = "Wagon" },
            new CargoCustomer { Languages = "en", StationName = "Alby", OperationDaysFlags = receiverDays, QuantityUnitResourceKey = "Wagon" })
        {
            Quantity = 1,
            QuantityIsBearer = true,
            PrintCount = 1,
            HasEmptyReturn = emptyReturn,
            HasSameCargoReturn = sameCargoReturn,
        };

    [TestMethod]
    public void WaybillHasSendersDays()
    {
        var printed = new[] { Waybill(senderDays: Monday, receiverDays: MondayToFriday) }.AsPrintableWaybills().Single();
        Assert.AreEqual(Monday, printed.OperatingDayFlag);
    }

    [TestMethod]
    public void SendersDaysAlsoWhenTheReceiverHasNoneOfThem()
    {
        var printed = new[] { Waybill(senderDays: Weekend, receiverDays: MondayToFriday) }.AsPrintableWaybills().Single();
        Assert.AreEqual(Weekend, printed.OperatingDayFlag);
    }

    [TestMethod]
    public void EmptyReturnIsDaily()
    {
        var printed = new[] { Waybill(senderDays: Monday, receiverDays: MondayToFriday, emptyReturn: true) }.AsPrintableWaybills().ToList();
        Assert.HasCount(2, printed);
        Assert.AreEqual(Monday, printed[0].OperatingDayFlag);
        Assert.IsTrue(printed[1].IsEmptyReturn);
        Assert.AreEqual(OperationDaysExtensions.DailyFlag, printed[1].OperatingDayFlag);
        Assert.AreEqual("Daily", printed[1].SendingDays());
    }

    [TestMethod]
    public void SameCargoReturnHasItsSendersDays()
    {
        var printed = new[] { Waybill(senderDays: Monday, receiverDays: Weekend, sameCargoReturn: true) }.AsPrintableWaybills().ToList();
        Assert.HasCount(2, printed);
        Assert.AreEqual("Alby", printed[1].Origin.StationName, "The return is sent by the outgoing waybill's receiver.");
        Assert.AreEqual(Weekend, printed[1].OperatingDayFlag);
    }
}