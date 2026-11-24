// A field that is enabled only while one of some checkboxes is checked (AppInputText with EnabledBy, 2026-11-24). Markup:
//   <input data-enabled-by="Form.A Form.B" disabled>
// The page renders the field disabled or not from the posted or stored values, and the service keeps the stored value when none
// of the checkboxes is checked, since a disabled field isn't posted. This script only follows the checkboxes while editing;
// without JavaScript, the field keeps the state it was rendered with.
(function () {
    'use strict';

    function apply(input) {
        const form = input.closest('form');
        if (!form) return;
        input.disabled = !input.getAttribute('data-enabled-by').split(' ').filter(Boolean).some(function (name) {
            const box = form.querySelector('input[type="checkbox"][name="' + CSS.escape(name) + '"]');
            return box !== null && box.checked;
        });
    }

    function applyAll() {
        document.querySelectorAll('[data-enabled-by]').forEach(apply);
    }

    function changed(event) {
        const name = event.target && event.target.name;
        if (!name) return;
        document.querySelectorAll('[data-enabled-by]').forEach(function (input) {
            if (input.getAttribute('data-enabled-by').split(' ').includes(name)) apply(input);
        });
    }

    document.addEventListener('change', changed);
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', applyAll);
    else applyAll();
    if (window.Blazor && window.Blazor.addEventListener) window.Blazor.addEventListener('enhancedload', applyAll);
})();