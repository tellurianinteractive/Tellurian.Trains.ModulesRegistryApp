// A list whose value follows another list and is locked while that list has a value (AppInputSelect with LockedBy, 2026-11-25),
// such as the scale of an end profile, which is the scale of its standard. Markup:
//   <select name="Form.ScaleId" data-locked-by="Form.ModuleStandardId" data-locked-values='{"2":3}' disabled>
//   <input type="hidden" name="Form.ScaleId" value="3" data-locked-copy>
// A disabled list isn't posted, so a hidden copy of its value is, while it is locked. The page renders the list locked or not from
// the posted or stored values, and the service sets the value from the other list anyway. This script only follows the other list
// while editing; without JavaScript, the list keeps the state it was rendered with.
(function () {
    'use strict';

    function apply(select) {
        const form = select.closest('form');
        if (!form) return;
        const source = form.querySelector('select[name="' + CSS.escape(select.getAttribute('data-locked-by')) + '"]');
        const values = JSON.parse(select.getAttribute('data-locked-values') || '{}');
        const value = source && source.value ? values[source.value] : undefined;
        let copy = select.parentNode.querySelector('input[data-locked-copy]');
        if (value === undefined) {
            select.disabled = false;
            if (copy) copy.remove();
            return;
        }
        select.value = String(value);
        select.disabled = true;
        if (!copy) {
            copy = document.createElement('input');
            copy.type = 'hidden';
            copy.name = select.name;
            copy.setAttribute('data-locked-copy', '');
            select.parentNode.appendChild(copy);
        }
        copy.value = String(value);
    }

    function applyAll() {
        document.querySelectorAll('select[data-locked-by]').forEach(apply);
    }

    function changed(event) {
        const name = event.target && event.target.name;
        if (!name) return;
        document.querySelectorAll('select[data-locked-by]').forEach(function (select) {
            if (select.getAttribute('data-locked-by') === name) apply(select);
        });
    }

    document.addEventListener('change', changed);
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', applyAll);
    else applyAll();
    if (window.Blazor && window.Blazor.addEventListener) window.Blazor.addEventListener('enhancedload', applyAll);
})();