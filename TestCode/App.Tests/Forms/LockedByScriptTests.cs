using Jint;

namespace ModulesRegistry.AppTests.Forms;

/// <summary>
/// <c>wwwroot/js/locked-by.js</c>: a list with <c>data-locked-by</c> follows the other list, is locked while it has a value that
/// decides this one, and is then posted through a hidden copy; without such a value it is unlocked and the copy removed. The script
/// runs in Jint against a minimal stand-in for the DOM: an end profile's scale (2) locked by its standard.
/// </summary>
[TestClass]
public sealed class LockedByScriptTests
{
    private const string Dom = """
        const CSS = { escape: s => s };
        class Element {
            constructor(tag, attributes) { this.tagName = tag; this.attributes = attributes || {}; this.children = []; this.parentNode = null; this.value = ''; this.disabled = false; this.name = this.attributes.name || ''; }
            getAttribute(name) { return Object.prototype.hasOwnProperty.call(this.attributes, name) ? this.attributes[name] : null; }
            setAttribute(name, value) { this.attributes[name] = value; }
            appendChild(child) { child.parentNode = this; this.children.push(child); return child; }
            remove() { const siblings = this.parentNode.children; siblings.splice(siblings.indexOf(this), 1); this.parentNode = null; }
            closest(tag) { let e = this; while (e && e.tagName !== tag) e = e.parentNode; return e; }
            all() { return this.children.flatMap(c => [c, ...c.all()]); }
            querySelector(selector) {
                if (selector === 'input[data-locked-copy]') return this.all().find(e => e.tagName === 'input' && e.getAttribute('data-locked-copy') !== null) || null;
                const name = /select\[name="(.*)"\]/.exec(selector)[1];
                return this.all().find(e => e.tagName === 'select' && e.name === name) || null;
            }
        }
        const form = new Element('form');
        const standard = form.appendChild(new Element('select', { name: 'Form.ModuleStandardId' }));
        const wrapper = form.appendChild(new Element('span'));
        const scale = wrapper.appendChild(new Element('select', { name: 'Form.ScaleId', 'data-locked-by': 'Form.ModuleStandardId', 'data-locked-values': '{"2":3,"5":4}' }));
        scale.value = '1';
        let handler = null;
        const document = {
            readyState: 'complete',
            addEventListener: (type, listener) => { if (type === 'change') handler = listener; },
            querySelectorAll: selector => ({ forEach: f => form.all().filter(e => e.getAttribute('data-locked-by') !== null).forEach(f) }),
            createElement: tag => new Element(tag),
        };
        const window = {};
        function choose(value) { standard.value = value; handler({ target: standard }); }
        function copy() { return wrapper.querySelector('input[data-locked-copy]'); }
        """;

    private static Engine Load()
    {
        var engine = new Engine();
        engine.Execute(Dom);
        engine.Execute(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Forms", "locked-by.js")));
        return engine;
    }

    [TestMethod]
    public void AStandardSetsAndLocksTheScaleAndPostsACopy()
    {
        var engine = Load();
        engine.Execute("choose('2');");

        Assert.AreEqual("3", engine.Evaluate("scale.value").AsString());
        Assert.IsTrue(engine.Evaluate("scale.disabled").AsBoolean());
        Assert.AreEqual("Form.ScaleId", engine.Evaluate("copy().name").AsString());
        Assert.AreEqual("3", engine.Evaluate("copy().value").AsString());
    }

    [TestMethod]
    public void AnotherStandardChangesTheCopy()
    {
        var engine = Load();
        engine.Execute("choose('2'); choose('5');");

        Assert.AreEqual("4", engine.Evaluate("copy().value").AsString());
        Assert.AreEqual(1, (int)engine.Evaluate("wrapper.children.filter(c => c.tagName === 'input').length").AsNumber(), "One copy.");
    }

    [TestMethod]
    public void NoStandardUnlocksTheScaleAndRemovesTheCopy()
    {
        var engine = Load();
        engine.Execute("choose('2'); choose('');");

        Assert.IsFalse(engine.Evaluate("scale.disabled").AsBoolean());
        Assert.IsTrue(engine.Evaluate("copy() === null").AsBoolean());
        Assert.AreEqual("3", engine.Evaluate("scale.value").AsString(), "The scale it had stays chosen.");
    }

    [TestMethod]
    public void OtherListsAreIgnored()
    {
        var engine = Load();
        engine.Execute("handler({ target: { name: 'Form.Other' } });");

        Assert.IsFalse(engine.Evaluate("scale.disabled").AsBoolean());
        Assert.AreEqual("1", engine.Evaluate("scale.value").AsString());
    }
}