using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Localities;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>The locality form: the mapping to and from the entity, the coordinates as one field, and its validation.</summary>
[TestClass]
public sealed class LocalityFormTests
{
    private static Locality Existing() => new()
    {
        Id = 5, CountryId = 1, Name = "Sporthallen", IsModuleStorage = false, IsMeetingVenue = true, StreetAddress = "Storgatan 1",
        PostalCode = "331 30", CityName = "Värnamo", Latitude = 57.1829m, Longitude = 14.0474m, Description = "Lastkaj på baksidan",
        UsedByGroupId = 3, HallDrawingId = 9,
    };

    [TestMethod]
    public void RoundTripChangesNothing()
    {
        var expected = Existing();
        var actual = Existing().AsFormData().AsEntity(Existing());
        foreach (var property in typeof(Locality).GetProperties().Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string) || Nullable.GetUnderlyingType(p.PropertyType) is not null))
            Assert.AreEqual(property.GetValue(expected), property.GetValue(actual), property.Name);
    }

    [TestMethod]
    public void CoordinatesAreOneField()
    {
        Assert.AreEqual("57.182900, 14.047400", Existing().AsFormData().Coordinates);

        var saved = Existing().AsFormData().With(f => f.Coordinates = "56,1 13,2").AsEntity(Existing());

        Assert.AreEqual(56.1m, saved.Latitude);
        Assert.AreEqual(13.2m, saved.Longitude);
    }

    [TestMethod]
    public void EmptyCoordinatesClearThePosition()
    {
        var saved = Existing().AsFormData().With(f => f.Coordinates = " ").AsEntity(Existing());

        Assert.IsNull(saved.Latitude);
        Assert.IsNull(saved.Longitude);
    }

    [TestMethod]
    public void TheHallDrawingStays()
    {
        Assert.AreEqual(9, Existing().AsFormData().AsEntity(Existing()).HallDrawingId);
    }

    [TestMethod]
    public void UnreadableCoordinatesAreInvalid()
    {
        CollectionAssert.AreEqual(new[] { "Coordinates" }, InvalidMembers(Existing().AsFormData().With(f => f.Coordinates = "57.1829")));
        Assert.IsEmpty(InvalidMembers(Existing().AsFormData()));
    }

    [TestMethod]
    public void NameCityAndCountryAreRequired()
    {
        CollectionAssert.AreEquivalent(new[] { "Name", "CityName", "CountryId" }, InvalidMembers(new LocalityFormData()));
    }

    private static string[] InvalidMembers(object form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return [.. results.SelectMany(r => r.MemberNames).Distinct()];
    }
}