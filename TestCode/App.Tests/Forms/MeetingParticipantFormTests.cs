using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Meetings;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// A meeting participant's and a layout participant's form models (Migration step 3.6): at least one day, the times, the days between
/// the first and last checked day filled in, and that only the form's fields change.
/// </summary>
[TestClass]
public sealed class MeetingParticipantFormTests
{
    private static MeetingParticipant Existing() => new()
    {
        Id = 3, MeetingId = 1, PersonId = 2, ParticipateDay2 = true, ParticipateDay3 = true,
        LatestArrivalTime = new TimeOnly(18, 0), EarliestDepartureTime = new TimeOnly(14, 0),
        RegistrationTime = new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero),
    };

    [TestMethod]
    public void AtLeastOneDayMustBeChecked()
    {
        var form = Existing().AsFormData();
        form.Day2 = false;
        form.Day3 = false;
        CollectionAssert.AreEqual(new[] { nameof(MeetingParticipantFormData.Day1) }, InvalidMembers(form));
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(6)]
    public void OneCheckedDayIsEnough(int day)
    {
        var form = new MeetingParticipantFormData { LatestArrivalTime = new TimeOnly(18, 0), EarliestDepartureTime = new TimeOnly(14, 0) };
        typeof(MeetingParticipantFormData).GetProperty($"Day{day}")!.SetValue(form, true);
        Assert.IsEmpty(InvalidMembers(form));
    }

    [TestMethod]
    public void TheTimesAreRequired()
    {
        var form = Existing().AsFormData();
        form.LatestArrivalTime = null;
        form.EarliestDepartureTime = null;
        CollectionAssert.AreEquivalent(new[] { nameof(MeetingParticipantFormData.LatestArrivalTime), nameof(MeetingParticipantFormData.EarliestDepartureTime) }, InvalidMembers(form));
    }

    [TestMethod]
    public void TheDaysBetweenTheFirstAndLastAreFilledIn()
    {
        var form = Existing().AsFormData();
        form.Day2 = false;
        form.Day3 = false;
        form.Day1 = true;
        form.Day4 = true;
        var participant = form.AsEntity(Existing());
        Assert.IsTrue(participant.ParticipateDay1 && participant.ParticipateDay2 && participant.ParticipateDay3 && participant.ParticipateDay4);
        Assert.IsFalse(participant.ParticipateDay5);
    }

    [TestMethod]
    public void OnlyTheDaysAndTimesChange()
    {
        var existing = Existing();
        var participant = existing.AsFormData().AsEntity(existing);
        Assert.AreSame(existing, participant);
        Assert.AreEqual(3, participant.Id);
        Assert.AreEqual(2, participant.PersonId);
        Assert.AreEqual(new DateTimeOffset(2026, 10, 1, 12, 0, 0, TimeSpan.Zero), participant.RegistrationTime);
    }

    [TestMethod]
    public void AnEmptyNoteToTheOrganiserIsSavedAsEmpty()
    {
        var layoutParticipant = new LayoutParticipant { Id = 4, LayoutId = 5, MeetingParticipantId = 3, PersonId = 2, NoteToOrganiserMarkdown = "Old" };
        new LayoutParticipantFormData { NoteToOrganiserMarkdown = "  ", BringsModulesAfterAgreement = true }.AsEntity(layoutParticipant);
        Assert.IsNull(layoutParticipant.NoteToOrganiserMarkdown);
        Assert.IsTrue(layoutParticipant.BringsModulesAfterAgreement);
        Assert.AreEqual(5, layoutParticipant.LayoutId);
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}