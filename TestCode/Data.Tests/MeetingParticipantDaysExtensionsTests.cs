namespace ModulesRegistry.Data.Tests;

[TestClass]
public class MeetingParticipantDaysExtensionsTests
{
    [TestMethod]
    public void FirstAndLastDayAreZeroWithoutParticipation()
    {
        var target = new MeetingParticipant();

        Assert.AreEqual((0, 0), (target.FirstParticipationDayNumber(), target.LastParticipationDayNumber()));
    }

    [TestMethod]
    public void FirstAndLastCheckedDay()
    {
        var target = new MeetingParticipant { ParticipateDay2 = true, ParticipateDay5 = true };

        Assert.AreEqual((2, 5), (target.FirstParticipationDayNumber(), target.LastParticipationDayNumber()));
    }

    [TestMethod]
    public void FillChecksTheDaysBetweenFirstAndLast()
    {
        var target = new MeetingParticipant { ParticipateDay1 = true, ParticipateDay4 = true };

        target.FillParticipationDaysBetween();

        CollectionAssert.AreEqual(new[] { true, true, true, true, false, false }, Days(target));
    }

    [TestMethod]
    public void FillDoesNothingWithOneOrNoCheckedDay()
    {
        var single = new MeetingParticipant { ParticipateDay3 = true };
        var none = new MeetingParticipant();

        single.FillParticipationDaysBetween();
        none.FillParticipationDaysBetween();

        CollectionAssert.AreEqual(new[] { false, false, true, false, false, false }, Days(single));
        CollectionAssert.AreEqual(new[] { false, false, false, false, false, false }, Days(none));
    }

    [TestMethod]
    [DataRow(1, new[] { true, false, false, false, false, false })]
    [DataRow(4, new[] { true, true, true, true, false, false })]
    [DataRow(6, new[] { true, true, true, true, true, true })]
    [DataRow(8, new[] { true, true, true, true, true, true })]
    public void AllDaysChecksTheMeetingsDays(int daysCount, bool[] expected)
    {
        var target = new MeetingParticipant();

        target.ParticipateAllDays(daysCount);

        CollectionAssert.AreEqual(expected, Days(target));
    }

    [TestMethod]
    public void SetAndGetByDayNumber()
    {
        var target = new MeetingParticipant();

        target.SetParticipatesDay(6, true);
        target.SetParticipatesDay(7, true); // Ignored: only six days.

        Assert.IsTrue(target.ParticipatesDay(6));
        Assert.IsTrue(target.ParticipateDay6);
        Assert.IsFalse(target.ParticipatesDay(7));
    }

    private static bool[] Days(MeetingParticipant participant) =>
        [.. Enumerable.Range(1, MeetingParticipantDaysExtensions.MaxDays).Select(participant.ParticipatesDay)];
}