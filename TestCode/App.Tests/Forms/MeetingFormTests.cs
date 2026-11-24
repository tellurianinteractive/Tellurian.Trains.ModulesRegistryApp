using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Meetings;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The meeting form model (Migration step 3.6): the mapping to and from the entity, and its DataAnnotations run as the server runs them.
/// </summary>
[TestClass]
public sealed class MeetingFormTests
{
    private static readonly DateTime Today = new(2026, 11, 21);

    private static Meeting Existing() => new()
    {
        Id = 4, OrganiserGroupId = 2, IsOrganiserInternal = true, GroupDomainId = 3, CityName = "Karlstad", VenueName = "Hall", VenueLocalityId = 6, Name = "Autumn meeting",
        StartDate = new DateTime(2027, 3, 1), EndDate = new DateTime(2027, 3, 4), LatestArrivalTimeWithModules = new TimeOnly(10, 0),
        EarliestDepartureTimeWithModules = new TimeOnly(14, 0), Status = (int)MeetingStatus.Planned, MeetingType = (int)MeetingType.ModuleMeeting,
        Details = "# Details", Accomodation = "Hotel", Food = "Lunch", ExternalLink = "https://example.com", AccessKey = Guid.NewGuid(),
    };

    private static MeetingFormData ValidForm()
    {
        var existing = Existing();
        var form = existing.AsFormData();
        form.StoredStartDate = existing.StartDate;
        form.Today = Today;
        return form;
    }

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in typeof(Meeting).GetProperties().Where(p => p.CanWrite && p.Name != nameof(Meeting.AccessKey) && (p.PropertyType.IsValueType || p.PropertyType == typeof(string))))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void EditKeepsTheAccessKey()
    {
        var existing = Existing();
        var key = existing.AccessKey;
        Assert.AreSame(existing, ValidForm().AsEntity(existing));
        Assert.AreEqual(key, existing.AccessKey);
    }

    [TestMethod]
    public void TheCityIsRequiredOnlyWithoutAVenue()
    {
        var form = ValidForm();
        form.CityName = " ";
        Assert.IsEmpty(InvalidMembers(form));
        form.VenueLocalityId = null;
        CollectionAssert.AreEqual(new[] { nameof(MeetingFormData.CityName) }, InvalidMembers(form));
    }

    [TestMethod]
    public void ValidFormHasNoErrors() => Assert.IsEmpty(InvalidMembers(ValidForm()));

    [TestMethod]
    public void AModuleMeetingNeedsArrivalAndDepartureTimes()
    {
        var form = ValidForm();
        form.LatestArrivalTimeWithModules = null;
        form.EarliestDepartureTimeWithModules = null;
        CollectionAssert.AreEquivalent(new[] { nameof(MeetingFormData.LatestArrivalTimeWithModules), nameof(MeetingFormData.EarliestDepartureTimeWithModules) }, InvalidMembers(form));
        form.MeetingType = (int)MeetingType.ClubEvent;
        Assert.IsEmpty(InvalidMembers(form));
    }

    [TestMethod]
    public void AChangedStartDateIsNotMoreThanAWeekAgo()
    {
        var form = ValidForm();
        form.StartDate = Today.AddDays(-8);
        CollectionAssert.AreEqual(new[] { nameof(MeetingFormData.StartDate) }, InvalidMembers(form));
        form.StartDate = Today.AddDays(-7);
        Assert.IsEmpty(InvalidMembers(form));
    }

    [TestMethod]
    public void APastMeetingCanStillBeSavedWithItsDates()
    {
        var form = ValidForm();
        form.StoredStartDate = Today.AddYears(-1);
        form.StartDate = Today.AddYears(-1);
        Assert.IsEmpty(InvalidMembers(form));
    }

    [TestMethod]
    public void OrganiserAndStatusMustBeSelected()
    {
        var form = ValidForm();
        form.OrganiserGroupId = 0;
        form.Status = 0;
        CollectionAssert.AreEquivalent(new[] { nameof(MeetingFormData.OrganiserGroupId), nameof(MeetingFormData.Status) }, InvalidMembers(form));
    }

    [TestMethod]
    public void DetailsHaveAMaximumLength()
    {
        var form = ValidForm();
        form.Details = new string('a', MeetingFormData.DetailsMaxLength + 1);
        CollectionAssert.AreEqual(new[] { nameof(MeetingFormData.Details) }, InvalidMembers(form));
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}