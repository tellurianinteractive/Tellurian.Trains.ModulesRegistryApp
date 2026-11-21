using Jint;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// <c>wwwroot/js/leave-guard.js</c> (Migration step 3.6): a static form is unsaved when a field differs from the value it was
/// rendered with, or when the server marks it so (a form shown again after a POST). Hidden fields, the antiforgery token and the
/// form handler don't count. The script runs in Jint against a minimal stand-in for the DOM.
/// </summary>
[TestClass]
public sealed class LeaveGuardScriptTests
{
    private const string Dom = """
        const window = globalThis;
        class Element { closest() { return null; } }
        const document = { addEventListener() {}, querySelectorAll() { return []; } };
        window.addEventListener = function () {};
        function text(name, value, defaultValue) { return { name: name, type: 'text', value: value, defaultValue: defaultValue }; }
        function check(name, checked, defaultChecked) { return { name: name, type: 'checkbox', checked: checked, defaultChecked: defaultChecked }; }
        function select(name, selected, defaultSelected) { return { name: name, type: 'select-one', options: [
            { selected: selected === 0, defaultSelected: defaultSelected === 0 }, { selected: selected === 1, defaultSelected: defaultSelected === 1 }] }; }
        function form(unsaved, elements) { return { dataset: { leaveGuard: 'Ignore?', leaveUnsaved: unsaved ? 'true' : 'false' }, elements: elements }; }
        function base() { return [ { name: '__RequestVerificationToken', type: 'hidden', value: 'a', defaultValue: 'b' },
            { name: '_handler', type: 'hidden', value: 'x', defaultValue: 'y' }, { name: 'Form.Id', type: 'hidden', value: '1', defaultValue: '2' } ]; }
        """;

    private static Engine Load()
    {
        var engine = new Engine();
        engine.Execute(Dom);
        engine.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Forms", "leave-guard.js")));
        return engine;
    }

    private static bool IsDirty(string elements, bool unsaved = false) =>
        Load().Evaluate($"LeaveGuard.isDirty(form({(unsaved ? "true" : "false")}, base().concat([{elements}])))").AsBoolean();

    [TestMethod]
    public void AnUntouchedFormIsSaved() => Assert.IsFalse(IsDirty("text('Form.Name', 'Ica', 'Ica'), check('Form.IsActive', true, true), select('Form.CountryId', 1, 1)"));

    [TestMethod]
    public void HiddenFieldsTheTokenAndTheHandlerDontCount() => Assert.IsFalse(IsDirty(""));

    [TestMethod]
    public void AChangedTextIsUnsaved() => Assert.IsTrue(IsDirty("text('Form.Name', 'Coop', 'Ica')"));

    [TestMethod]
    public void AChangedCheckBoxIsUnsaved() => Assert.IsTrue(IsDirty("check('Form.IsActive', false, true)"));

    [TestMethod]
    public void AChangedSelectionIsUnsaved() => Assert.IsTrue(IsDirty("select('Form.CountryId', 0, 1)"));

    [TestMethod]
    public void AListWithNoOptionMarkedSelectedShowsItsFirstOptionUnchanged() =>
        // As "In partnership with" when empty: the browser selects the first option itself (Migration step 3.6).
        Assert.IsFalse(IsDirty("{ name: 'Form.GroupDomainId', type: 'select-one', options: [{ selected: true, defaultSelected: false }, { selected: false, defaultSelected: false }] }"));

    [TestMethod]
    public void AListWithNoOptionMarkedSelectedIsChangedWhenAnotherIsChosen() =>
        Assert.IsTrue(IsDirty("{ name: 'Form.GroupDomainId', type: 'select-one', options: [{ selected: false, defaultSelected: false }, { selected: true, defaultSelected: false }] }"));

    [TestMethod]
    public void AFormShownAgainAfterAPostIsUnsaved() => Assert.IsTrue(IsDirty("text('Form.Name', 'Ica', 'Ica')", unsaved: true));
}