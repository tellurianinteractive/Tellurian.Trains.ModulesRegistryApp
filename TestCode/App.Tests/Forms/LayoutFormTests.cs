using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Layouts;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The layout and layout station form models (Migration step 3.6): the mapping to and from the entities, and their DataAnnotations run
/// as the server runs them.
/// </summary>
[TestClass]
public sealed class LayoutFormTests
{
    private static Layout ExistingLayout() => new()
    {
        Id = 5, MeetingId = 2, OrganisingGroupId = 3, ContactPersonId = 7, PrimaryModuleStandardId = 1, IsRegistrationPermitted = true,
        RegistrationOpeningDate = new DateTime(2026, 9, 1), RegistrationClosingDate = new DateTime(2027, 1, 15), ModuleRegistrationClosingDate = new DateTime(2027, 2, 1),
        ShortName = "H0 Fremo", Theme = "Sweden 1970s", Details = "# Note", FirstYear = 1965, LastYear = 1980, MaxNumberOfParticipants = 40,
    };

    private static LayoutStation ExistingStation() => new()
    {
        Id = 4, LayoutParticipantId = 6, StationId = 8, OtherName = "Nedre Ullerud", OtherSignature = "Nu", OtherCountryId = 1,
    };

    [TestMethod]
    public void LayoutRoundTripChangesNothing() => AssertSameValues(ExistingLayout(), ExistingLayout().AsFormData().AsEntity(ExistingLayout()));

    [TestMethod]
    public void LayoutStationRoundTripChangesNothing() => AssertSameValues(ExistingStation(), ExistingStation().AsFormData().AsEntity(ExistingStation()));

    [TestMethod]
    public void ANewLayoutBelongsToTheMeeting()
    {
        var created = ExistingLayout().AsFormData().AsEntity(meetingId: 9);
        Assert.AreEqual(0, created.Id);
        Assert.AreEqual(9, created.MeetingId);
    }

    [TestMethod]
    public void EditKeepsTheMeetingAndTheStation()
    {
        var layout = ExistingLayout();
        Assert.AreSame(layout, ExistingLayout().AsFormData().AsEntity(layout, meetingId: 99));
        Assert.AreEqual(2, layout.MeetingId);
        var station = ExistingStation();
        station.AsFormData().AsEntity(station);
        Assert.AreEqual(8, station.StationId);
        Assert.AreEqual(6, station.LayoutParticipantId);
    }

    [TestMethod]
    public void ValidFormsHaveNoErrors()
    {
        Assert.IsEmpty(InvalidMembers(ExistingLayout().AsFormData()));
        Assert.IsEmpty(InvalidMembers(ExistingStation().AsFormData()));
    }

    [TestMethod]
    public void GroupAndStandardMustBeSelected()
    {
        var form = ExistingLayout().AsFormData();
        form.OrganisingGroupId = 0;
        form.PrimaryModuleStandardId = 0;
        CollectionAssert.AreEquivalent(new[] { nameof(LayoutFormData.OrganisingGroupId), nameof(LayoutFormData.PrimaryModuleStandardId) }, InvalidMembers(form));
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(201)]
    public void MaxNumberOfParticipantsIsFrom1To200(int number)
    {
        var form = ExistingLayout().AsFormData();
        form.MaxNumberOfParticipants = number;
        CollectionAssert.AreEqual(new[] { nameof(LayoutFormData.MaxNumberOfParticipants) }, InvalidMembers(form));
    }

    [TestMethod]
    public void NoMaximumAndAnOpenPeriodAreValid()
    {
        var form = ExistingLayout().AsFormData();
        form.MaxNumberOfParticipants = null;
        form.FirstYear = null;
        form.LastYear = null;
        form.ModuleRegistrationClosingDate = null;
        Assert.IsEmpty(InvalidMembers(form));
    }

    [TestMethod]
    public void AnOtherSignatureHasAtMostFiveCharacters()
    {
        var form = ExistingStation().AsFormData();
        form.OtherSignature = "Abcdef";
        CollectionAssert.AreEqual(new[] { nameof(LayoutStationFormData.OtherSignature) }, InvalidMembers(form));
    }

    private static void AssertSameValues<T>(T expected, T actual)
    {
        foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}