using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Resources;
using ModulesRegistry.Validators;

namespace ModulesRegistry.AppTests.Validators;

/// <summary>
/// The interactive forms' rules since Migration step 3.5 (decided 2026-11-19): what the service corrects when saving is not
/// refused. Names aren't checked for capitals, and the year, MAC address and DCC address rules check the value
/// as it will be after the correction.
/// </summary>
[TestClass]
public sealed class CorrectedInputRulesTests
{
    private static readonly IStringLocalizer<Strings> Localizer = new KeyLocalizer();

    private static string[] InvalidMembers<T>(FluentValidation.AbstractValidator<T> validator, T item) =>
        [.. validator.Validate(item).Errors.Select(e => e.PropertyName)];

    [TestMethod]
    public void NamesAreNotCheckedForCapitals()
    {
        var invalid = InvalidMembers(new StationValidator(Localizer), new Station { FullName = "nedre ullerud", Signature = "nu", IsShadow = true });
        CollectionAssert.DoesNotContain(invalid, nameof(Station.FullName));
        CollectionAssert.DoesNotContain(invalid, nameof(Station.Signature));
    }

    [TestMethod]
    [DataRow(null, 1900, true)]
    [DataRow((short)0, 1900, true, DisplayName = "Zero is saved as empty")]
    [DataRow((short)1900, 1900, true)]
    [DataRow((short)2100, 1900, true)]
    [DataRow((short)2101, 1900, false)]
    [DataRow((short)1899, 1900, false)]
    [DataRow((short)1850, 1850, true, DisplayName = "A vehicle's manufacture year")]
    public void YearOrEmpty(short? year, int minimum, bool expected) => Assert.AreEqual(expected, year.IsYearOrEmpty(minimum));

    [TestMethod]
    public void ReversedYearsAreAcceptedSinceTheyAreSwappedWhenSaving()
    {
        var invalid = InvalidMembers(new LayoutValidator(Localizer), new Layout { FirstYear = 1970, LastYear = 1950 });
        CollectionAssert.DoesNotContain(invalid, nameof(Layout.FirstYear));
        CollectionAssert.DoesNotContain(invalid, nameof(Layout.LastYear));
    }

    private sealed class KeyLocalizer : IStringLocalizer<Strings>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => this[name];
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}