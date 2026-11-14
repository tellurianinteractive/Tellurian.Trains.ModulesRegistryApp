using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Validators;
using ModulesRegistry.Resources;

namespace ModulesRegistry.AppTests.Validators;

[TestClass]
public sealed class MeetingParticipantValidatorTests
{
    private readonly MeetingParticipantValidator _target = new(new KeyLocalizer());

    [TestMethod]
    public void AtLeastOneDayMustBeChecked()
    {
        var participant = ValidParticipant();

        var result = _target.Validate(participant);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual("SelectAtLeastOneDay", result.Errors.Single().ErrorMessage);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(6)]
    public void OneCheckedDayIsEnough(int day)
    {
        var participant = ValidParticipant();
        participant.SetParticipatesDay(day, true);

        var result = _target.Validate(participant);

        Assert.IsTrue(result.IsValid, string.Join(", ", result.Errors.Select(e => e.ErrorMessage)));
    }

    private static MeetingParticipant ValidParticipant() => new()
    {
        MeetingId = 1,
        PersonId = 2,
        LatestArrivalTime = new TimeOnly(18, 0),
        EarliestDepartureTime = new TimeOnly(14, 0),
    };

    /// <summary>Returns the resource key as the text, so the tests can check which message is used.</summary>
    private sealed class KeyLocalizer : IStringLocalizer<Strings>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => this[name];
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}