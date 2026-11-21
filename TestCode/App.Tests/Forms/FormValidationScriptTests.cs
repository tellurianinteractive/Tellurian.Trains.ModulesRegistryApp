using Jint;
using ModulesRegistry.Data.Extensions;
using ModulesRegistry.Forms.Validation;
using System.Text.Json;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// The browser validators in <c>wwwroot/js/form-validation.js</c> must give the same results as the C# rules and corrections
/// (Migration step 3.5), so the browser neither blocks what the server accepts nor lets through what it refuses. Both run against
/// the same table of values; the script runs in Jint, a JavaScript engine in .NET.
/// </summary>
[TestClass]
public sealed class FormValidationScriptTests
{
    private static Engine Script = null!;

    [ClassInitialize]
    public static void LoadScript(TestContext _)
    {
        Script = new Engine();
        Script.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Forms", "form-validation.js")));
    }

    private static string Js(string? value) => JsonSerializer.Serialize(value);

    // Input, as corrected when saving, and whether [OrdinaryText] accepts it.
    public static IEnumerable<object?[]> OrdinaryTextValues =>
    [
        [null, null, true],
        ["", "", true],
        ["Malmö C, spår 1-3 (öst)", "Malmö C, spår 1-3 (öst)", true],
        ["Ærøskøbing & Łódź: 20 % «test»", "Ærøskøbing & Łódź: 20 % «test»", true],
        ["  Hällsjö  ", "Hällsjö", true],
        ["Kalle’s “bästa”", "Kalle's \"bästa\"", true],
        ["1950–1965 — slut", "1950-1965 - slut", true],
        ["Vänta…", "Vänta...", true],
        ["Nedre Ullerud", "Nedre Ullerud", true],
        ["Nedre Ullerud", "Nedre Ullerud", true],
        ["Gröndal", "Gröndal", true],
        ["Kors​berga﻿", "Korsberga", true],
        ["A\u0007B", "AB", true],
        ["Övre   Bäck\tgården", "Övre Bäck gården", true],
        ["Rad ett\r\nrad två", "Rad ett rad två", true],
        ["Москва", "Москва", false],
        ["Emoji 🚂", "Emoji 🚂", false],
        ["<script>", "<script>", false],
        ["e-post@exempel.se", "e-post@exempel.se", false],
        ["under_streck", "under_streck", false],
        ["STRAẞE", "STRAẞE", true],
        ["1º piano, 2ª", "1º piano, 2ª", true],
        ["Łódź, Žilina, Hőmérséklet, Œuvre, IJmuiden", "Łódź, Žilina, Hőmérséklet, Œuvre, IJmuiden", true],
        ["Große \u201EStraße\u201C", "Große \"Straße\"", true],
    ];

    [TestMethod]
    [DynamicData(nameof(OrdinaryTextValues))]
    public void CorrectionIsTheSame(string? input, string? corrected, bool _)
    {
        Assert.AreEqual(corrected, input.AsOrdinaryText(), "C#");
        Assert.AreEqual(corrected, Script.Evaluate($"modulesRegistryValidation.asOrdinaryText({Js(input)})").ToObject() as string, "JavaScript");
    }

    [TestMethod]
    [DynamicData(nameof(OrdinaryTextValues))]
    public void OrdinaryTextIsTheSame(string? input, string? _, bool valid)
    {
        Assert.AreEqual(valid, new OrdinaryTextAttribute().IsValid(input), "C#");
        Assert.AreEqual(valid, Script.Evaluate($"modulesRegistryValidation.validators.{OrdinaryTextAttribute.ClientRuleName}({{ value: {Js(input)} }}).success").AsBoolean(), "JavaScript");
    }

    // A year as typed, and whether [YearOrEmpty] accepts it (the C# value is the posted int?, or no number).
    public static IEnumerable<object?[]> YearValues =>
    [
        [null, true], ["", true], ["0", true], ["-5", true], ["1900", true], ["2100", true], ["1965", true],
        ["1899", false], ["2101", false], ["19650", false],
    ];

    [TestMethod]
    [DynamicData(nameof(YearValues))]
    public void YearOrEmptyIsTheSame(string? typed, bool valid)
    {
        int? year = typed is { Length: > 0 } ? int.Parse(typed, System.Globalization.CultureInfo.InvariantCulture) : null;
        Assert.AreEqual(valid, new YearOrEmptyAttribute().IsValid(year), "C#");
        Assert.AreEqual(valid, Script.Evaluate($"modulesRegistryValidation.validators.{WholeNumberOrEmptyAttribute.ClientRuleName}({{ value: {Js(typed)}, params: {{ min: '1900', max: '2100' }} }}).success").AsBoolean(), "JavaScript");
    }

    // A DCC address as typed, and whether [DccAddressOrEmpty] accepts it.
    public static IEnumerable<object?[]> DccAddressValues =>
    [
        [null, true], ["", true], ["0", true], ["1", true], ["3", true], ["9999", true], ["10000", false],
    ];

    [TestMethod]
    [DynamicData(nameof(DccAddressValues))]
    public void DccAddressOrEmptyIsTheSame(string? typed, bool valid)
    {
        int? address = typed is { Length: > 0 } ? int.Parse(typed, System.Globalization.CultureInfo.InvariantCulture) : null;
        var attribute = new DccAddressOrEmptyAttribute();
        var rule = attribute.GetClientValidationRules().Single();
        Assert.AreEqual(valid, attribute.IsValid(address), "C#");
        Assert.AreEqual(valid, Script.Evaluate($"modulesRegistryValidation.validators.{rule.Name}({{ value: {Js(typed)}, params: {{ min: '{rule.Parameters["min"]}', max: '{rule.Parameters["max"]}' }} }}).success").AsBoolean(), "JavaScript");
    }

    [TestMethod]
    public void TheJavaScriptRefusesWhatIsNoWholeNumber() =>
        Assert.IsFalse(Script.Evaluate("modulesRegistryValidation.isWholeNumberOrEmpty('19.5', 1900, 2100)").AsBoolean());

    // A MAC address as typed, and whether [MacAddress] accepts it (after the correction when saving).
    public static IEnumerable<object?[]> MacAddressValues =>
    [
        [null, true], ["", true], ["aabbccddeeff", true], ["aa-bb-cc-dd-ee-ff", true], ["aabb.ccdd.eeff", true], ["AA:BB:CC:DD:EE:FF", true], [" aa bb cc dd ee ff ", true],
        ["::::::::::::::::0", false], ["aa:bb:cc:dd:ee", false], ["gg:bb:cc:dd:ee:ff", false], ["aa:bb:cc:dd:ee:ff:00", false],
    ];

    [TestMethod]
    [DynamicData(nameof(MacAddressValues))]
    public void MacAddressIsTheSame(string? typed, bool valid)
    {
        Assert.AreEqual(valid, new MacAddressAttribute().IsValid(typed), "C#");
        Assert.AreEqual(valid, Script.Evaluate($"modulesRegistryValidation.validators.{MacAddressAttribute.ClientRuleName}({{ value: {Js(typed)} }}).success").AsBoolean(), "JavaScript");
    }

    // Email addresses as typed, and whether [EmailAddresses] accepts them (after the correction when saving).
    public static IEnumerable<object?[]> EmailAddressesValues =>
    [
        [null, true], ["", true], ["   ", true], ["anna@example.se", true], ["a@b.se; c@d.se", true], ["Anna@Example.se, anna@example.se", true],
        ["a@b.se\r\nc@d.se", true], ["a@b.se;;", true], ["Anna.Berg+modul@Example.co.uk", true],
        ["a@b.se;inte en adress", false], ["anna", false], ["anna@", false], ["anna@exam_ple.se", false], ["åsa@example.se", false], ["anna@example..se", false],
    ];

    [TestMethod]
    [DynamicData(nameof(EmailAddressesValues))]
    public void EmailAddressesIsTheSame(string? typed, bool valid)
    {
        Assert.AreEqual(valid, new EmailAddressesAttribute().IsValid(typed), "C#");
        Assert.AreEqual(valid, Script.Evaluate($"modulesRegistryValidation.validators.{EmailAddressesAttribute.ClientRuleName}({{ value: {Js(typed)} }}).success").AsBoolean(), "JavaScript");
    }

    // Loco addresses as typed, and whether [LocoAddresses] accepts them (after the correction when saving).
    public static IEnumerable<object?[]> LocoAddressesValues =>
    [
        [null, true], ["", true], ["3", true], ["3,10-15", true], ["15-10; 3 3", true], ["10 - 15", true], ["1-9999", true], [" 3 ", true],
        ["3, x", false], ["10000", false], ["0", false], ["-5", false], ["1-2-3", false], ["3.5", false], ["12345678901234567890", false],
    ];

    [TestMethod]
    [DynamicData(nameof(LocoAddressesValues))]
    public void LocoAddressesIsTheSame(string? typed, bool valid)
    {
        var attribute = new LocoAddressesAttribute();
        var rule = attribute.GetClientValidationRules().Single();
        Assert.AreEqual(valid, attribute.IsValid(typed), "C#");
        Assert.AreEqual(valid, Script.Evaluate($"modulesRegistryValidation.validators.{rule.Name}({{ value: {Js(typed)}, params: {{ min: '{rule.Parameters["min"]}', max: '{rule.Parameters["max"]}' }} }}).success").AsBoolean(), "JavaScript");
    }

    [TestMethod]
    public void TheAttributeEmitsTheRuleTheScriptRegisters()
    {
        var registered = new List<string>();
        var engine = new Engine();
        engine.SetValue("register", new Action<string>(registered.Add));
        engine.Execute("globalThis.Blazor = { formValidation: { addValidator: (name, validator) => register(name) } };");
        engine.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Forms", "form-validation.js")));

        CollectionAssert.Contains(registered, new OrdinaryTextAttribute().GetClientValidationRules().Single().Name);
        CollectionAssert.Contains(registered, new YearOrEmptyAttribute().GetClientValidationRules().Single().Name);
        CollectionAssert.Contains(registered, new MacAddressAttribute().GetClientValidationRules().Single().Name);
        CollectionAssert.Contains(registered, new EmailAddressesAttribute().GetClientValidationRules().Single().Name);
        CollectionAssert.Contains(registered, new LocoAddressesAttribute().GetClientValidationRules().Single().Name);
    }
}