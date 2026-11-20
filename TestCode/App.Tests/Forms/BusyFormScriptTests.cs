using Jint;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// <c>wwwroot/js/busy-form.js</c> (Migration step 3.6): submitting a form marked <c>data-busy</c> shows its message, marks the form
/// busy and ignores a second submit; other forms are left alone. The script runs in Jint against a minimal stand-in for the DOM.
/// </summary>
[TestClass]
public sealed class BusyFormScriptTests
{
    private const string Dom = """
        class HTMLFormElement {
            constructor(busy) { this.dataset = busy ? { busy: busy } : {}; this.classes = []; this.attributes = {};
                this.classList = { add: c => this.classes.push(c), contains: c => this.classes.includes(c) }; }
            setAttribute(name, value) { this.attributes[name] = value; }
        }
        const message = { hidden: true };
        let handler = null, capture = false;
        const document = {
            addEventListener: (type, listener, useCapture) => { if (type === 'submit') { handler = listener; capture = useCapture; } },
            getElementById: id => id === 'updating' ? message : null,
        };
        function submit(form) {
            const event = { target: form, prevented: false, stopped: false,
                preventDefault() { this.prevented = true; }, stopImmediatePropagation() { this.stopped = true; } };
            handler(event);
            return event;
        }
        """;

    private static Engine Load()
    {
        var engine = new Engine();
        engine.Execute(Dom);
        engine.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Forms", "busy-form.js")));
        return engine;
    }

    [TestMethod]
    public void ListensInTheCapturePhaseBeforeBlazor() => Assert.IsTrue(Load().Evaluate("capture").AsBoolean());

    [TestMethod]
    public void SubmittingABusyFormShowsItsMessageAndMarksItBusy()
    {
        var engine = Load();
        engine.Execute("const form = new HTMLFormElement('updating'); const first = submit(form);");

        Assert.IsFalse(engine.Evaluate("message.hidden").AsBoolean());
        Assert.IsTrue(engine.Evaluate("form.classList.contains('form-busy')").AsBoolean());
        Assert.AreEqual("true", engine.Evaluate("form.attributes['aria-busy']").AsString());
        Assert.IsFalse(engine.Evaluate("first.prevented").AsBoolean(), "The first submit goes through.");
    }

    [TestMethod]
    public void ASecondSubmitIsIgnored()
    {
        var engine = Load();
        engine.Execute("const form = new HTMLFormElement('updating'); submit(form); const second = submit(form);");

        Assert.IsTrue(engine.Evaluate("second.prevented").AsBoolean());
        Assert.IsTrue(engine.Evaluate("second.stopped").AsBoolean(), "Blazor's enhanced form handling doesn't post it again.");
    }

    [TestMethod]
    public void OtherFormsAreLeftAlone()
    {
        var engine = Load();
        engine.Execute("const form = new HTMLFormElement(null); const result = submit(form); submit(form);");

        Assert.IsTrue(engine.Evaluate("message.hidden").AsBoolean());
        Assert.IsFalse(engine.Evaluate("form.classList.contains('form-busy')").AsBoolean());
        Assert.IsFalse(engine.Evaluate("result.prevented").AsBoolean());
    }
}