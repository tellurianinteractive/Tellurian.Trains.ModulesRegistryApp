using Microsoft.Extensions.Localization;
using ModulesRegistry.Data;
using ModulesRegistry.Resources;
using ModulesRegistry.Validators;

namespace ModulesRegistry.AppTests.Validators;

/// <summary>
/// The interactive forms' rules since Migration step 3.5 (decided 2026-11-19): what the service corrects when saving is not
/// refused. Names aren't checked for capitals, and the address, year, MAC address, DCC address and colour rules check the value
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
    [DataRow(null, true)]
    [DataRow("", true)]
    [DataRow("a@b.se; c@d.se", true)]
    [DataRow("Anna@Example.se, anna@example.se", true)]
    [DataRow("a@b.se;inte en adress", false)]
    public void EmailAddresses(string? value, bool expected) => Assert.AreEqual(expected, value.IsEmailAddressesWhenCorrected());

    [TestMethod]
    [DataRow(null, true)]
    [DataRow("15-10; 3 3", true)]
    [DataRow("10 - 15", true)]
    [DataRow("3, x", false)]
    [DataRow("10000", false)]
    public void LocoAddresses(string? value, bool expected) => Assert.AreEqual(expected, value.IsLocoAddressesWhenCorrected());

    [TestMethod]
    [DataRow("aabbccddeeff", true)]
    [DataRow("aa-bb-cc-dd-ee-ff", true)]
    [DataRow("AA:BB:CC:DD:EE:FF", true)]
    [DataRow("::::::::::::::::0", false, DisplayName = "Colons in the wrong places, accepted before")]
    [DataRow("aa:bb:cc:dd:ee", false)]
    [DataRow(null, false)]
    public void MacAddress(string? value, bool expected) => Assert.AreEqual(expected, value.IsMacAddressWhenCorrected());

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

    [TestMethod]
    public void ThrottleAddressesAsTheyAreCorrected()
    {
        var invalid = InvalidMembers(new WiThrottleValidator(Localizer), new WiFredThrottle { MacAddress = "aabb.ccdd.eeff", LocoAddress1 = 0, LocoAddress2 = 10000 });
        CollectionAssert.DoesNotContain(invalid, nameof(WiFredThrottle.MacAddress));
        CollectionAssert.DoesNotContain(invalid, nameof(WiFredThrottle.LocoAddress1));
        CollectionAssert.Contains(invalid, nameof(WiFredThrottle.LocoAddress2));
    }

    [TestMethod]
    public void ColourInShortForm()
    {
        var invalid = InvalidMembers(new StationCustomerValidator(Localizer), new StationCustomer { CustomerName = "Ica", TrackOrAreaColor = "f80" });
        CollectionAssert.DoesNotContain(invalid, nameof(StationCustomer.TrackOrAreaColor));
        invalid = InvalidMembers(new StationCustomerValidator(Localizer), new StationCustomer { CustomerName = "Ica", TrackOrAreaColor = "red" });
        CollectionAssert.Contains(invalid, nameof(StationCustomer.TrackOrAreaColor));
    }

    private sealed class KeyLocalizer : IStringLocalizer<Strings>
    {
        public LocalizedString this[string name] => new(name, name);
        public LocalizedString this[string name, params object[] arguments] => this[name];
        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }
}