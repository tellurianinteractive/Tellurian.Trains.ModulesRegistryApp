using System.ComponentModel.DataAnnotations;
using ModulesRegistry.Data;
using ModulesRegistry.Forms.Stations;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The station and station track form models (Migration step 3.6): the mapping to and from the entities, and their DataAnnotations
/// run as the server runs them.
/// </summary>
[TestClass]
public sealed class StationFormTests
{
    private static Station ExistingStation() => new()
    {
        Id = 5, PrimaryModuleId = 9, PdfInstructionId = 3, TypeOfSwitchgearId = 2, BoosterTypes = "DCC", HasTurntable = true,
        RegionId = 4, FullName = "Nedre Ullerud", Signature = "Nu", HasCargoCustomers = true, IsJunction = true, IsTerminus = false,
        IsShadow = false, IsHarbour = true, IsManned = false, IsIndustrialArea = true, LockKeyName = "A", OperationInstructionsMarkdown = "# Instructions",
    };

    private static StationTrack ExistingTrack() => new()
    {
        Id = 8, StationId = 5, Designation = "2a", DisplayOrder = 2, DirectionId = (int)StationTrackDirection.EastWest, MaxTrainLength = 3.2,
        PlatformLength = 2.5, SpeedLimit = 40, UsageNote = "Freight", IsThroughTrack = true, IsSiding = false,
    };

    [TestMethod]
    public void StationRoundTripChangesNothing() => AssertSameValues(ExistingStation(), ExistingStation().AsFormData().AsEntity(ExistingStation()));

    [TestMethod]
    public void TrackRoundTripChangesNothing() => AssertSameValues(ExistingTrack(), ExistingTrack().AsFormData().AsEntity(ExistingTrack()));

    [TestMethod]
    public void StationCreateStartsNew()
    {
        var created = ExistingStation().AsFormData().AsEntity();
        Assert.AreEqual(0, created.Id);
        Assert.IsNull(created.PrimaryModuleId);
        Assert.IsNull(created.PdfInstructionId);
    }

    [TestMethod]
    public void TrackCreateBelongsToTheStation()
    {
        var created = ExistingTrack().AsFormData().AsEntity(stationId: 7);
        Assert.AreEqual(0, created.Id);
        Assert.AreEqual(7, created.StationId);
    }

    [TestMethod]
    public void ValidFormsHaveNoErrors()
    {
        Assert.IsEmpty(InvalidMembers(ExistingStation().AsFormData()));
        Assert.IsEmpty(InvalidMembers(ExistingTrack().AsFormData()));
    }

    [TestMethod]
    public void NamesAreNotCheckedForCapitals() => Assert.IsEmpty(InvalidMembers(Station(f => { f.FullName = "nedre ullerud"; f.Signature = "nu"; })));

    [TestMethod]
    public void AStationNeedsARegionUnlessItIsAShadowStation()
    {
        CollectionAssert.AreEqual(new[] { nameof(StationFormData.RegionId) }, InvalidMembers(Station(f => f.RegionId = null)));
        Assert.IsEmpty(InvalidMembers(Station(f => { f.RegionId = null; f.IsShadow = true; })));
    }

    [TestMethod]
    public void AStationNeedsAModule() => CollectionAssert.AreEqual(new[] { nameof(StationFormData.ModuleId) }, InvalidMembers(Station(f => f.ModuleId = 0)));

    [TestMethod]
    [DataRow("Ab")]
    [DataRow("")]
    public void AStationNameHasAtLeastThreeLetters(string name) =>
        CollectionAssert.AreEqual(new[] { nameof(StationFormData.FullName) }, InvalidMembers(Station(f => f.FullName = name)));

    [TestMethod]
    public void ASignatureHasAtMostFiveCharacters() =>
        CollectionAssert.AreEqual(new[] { nameof(StationFormData.Signature) }, InvalidMembers(Station(f => f.Signature = "Abcdef")));

    [TestMethod]
    [DataRow(0)]
    [DataRow(31)]
    public void DisplayOrderIsFrom1To30(int order) =>
        CollectionAssert.AreEqual(new[] { nameof(StationTrackFormData.DisplayOrder) }, InvalidMembers(Track(f => f.DisplayOrder = order)));

    [TestMethod]
    [DataRow(0.1)]
    [DataRow(10.1)]
    public void MaxTrainLengthIsFrom02To10(double length) =>
        CollectionAssert.AreEqual(new[] { nameof(StationTrackFormData.MaxTrainLength) }, InvalidMembers(Track(f => f.MaxTrainLength = length)));

    [TestMethod]
    public void NoPlatformAndNoSpeedLimitAreValid() => Assert.IsEmpty(InvalidMembers(Track(f => { f.PlatformLength = 0; f.SpeedLimit = null; })));

    [TestMethod]
    public void TheNotScheduledDirectionIsValid() => Assert.IsEmpty(InvalidMembers(Track(f => f.DirectionId = (int)StationTrackDirection.NotScheduled)));

    [TestMethod]
    public void ATrackNumberIsRequired() =>
        CollectionAssert.AreEqual(new[] { nameof(StationTrackFormData.Designation) }, InvalidMembers(Track(f => f.Designation = "")));

    private static StationFormData Station(Action<StationFormData> change)
    {
        var form = ExistingStation().AsFormData();
        change(form);
        return form;
    }

    private static StationTrackFormData Track(Action<StationTrackFormData> change)
    {
        var form = ExistingTrack().AsFormData();
        change(form);
        return form;
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