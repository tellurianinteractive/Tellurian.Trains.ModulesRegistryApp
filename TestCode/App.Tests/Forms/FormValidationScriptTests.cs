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
        ["Gro\u00DFe \u201EStra\u00DFe\u201C", "Große \"Straße\"", true],
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

    [TestMethod]
    public void TheAttributeEmitsTheRuleTheScriptRegisters()
    {
        var registered = new List<string>();
        var engine = new Engine();
        engine.SetValue("register", new Action<string>(registered.Add));
        engine.Execute("globalThis.Blazor = { formValidation: { addValidator: (name, validator) => register(name) } };");
        engine.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Forms", "form-validation.js")));

        CollectionAssert.Contains(registered, new OrdinaryTextAttribute().GetClientValidationRules().Single().Name);
    }
}