// Filter as you type for static SSR list pages (Migration step 3.3). Markup:
//   <form method="get" data-auto-submit data-enhance> ... <input name="code"> ... </form>
// A pause in typing submits the form; with data-enhance, Blazor fetches the page and updates only what changed, so the
// page isn't reloaded. Without JavaScript, the form is submitted with Enter or its button as usual.
// After the update, the field typed in gets its focus and caret back (also after a full load, when the form isn't
// enhanced), and text typed while the request was on its way is put back and submitted, instead of being overwritten.
(function () {
    'use strict';
    const debounceMilliseconds = 400;
    const focusKey = 'filter-form-focus';
    const timers = new WeakMap();
    let pending = null; // { form id, field name, value } of the last field typed in, until the page shows that value.

    document.addEventListener('input', function (event) {
        const field = event.target;
        const form = field.form;
        if (!form || !form.hasAttribute('data-auto-submit') || !field.name) return;
        pending = { form: form.id, name: field.name, value: field.value };
        clearTimeout(timers.get(form));
        timers.set(form, setTimeout(function () { submit(form, field); }, debounceMilliseconds));
    });

    document.addEventListener('submit', function (event) {
        const form = event.target;
        if (form.hasAttribute && form.hasAttribute('data-auto-submit')) clearTimeout(timers.get(form));
    }, true);

    function submit(form, field) {
        try {
            sessionStorage.setItem(focusKey, JSON.stringify({ form: form.id, name: field.name, caret: field.selectionStart }));
        } catch { /* No storage: the focus is just not restored. */ }
        form.requestSubmit();
    }

    function restore() {
        let focus = null;
        try {
            focus = JSON.parse(sessionStorage.getItem(focusKey));
            sessionStorage.removeItem(focusKey);
        } catch { /* No storage. */ }
        if (pending) {
            const field = fieldOf(pending.form, pending.name);
            if (field && field.value !== pending.value) {
                // Typed while the request was on its way: keep the text and filter again.
                field.value = pending.value;
                focus = focus || { form: pending.form, name: pending.name, caret: pending.value.length };
                submit(field.form, field);
            } else {
                pending = null;
            }
        }
        if (!focus) return;
        const field = fieldOf(focus.form, focus.name);
        if (!field || document.activeElement === field) return;
        field.focus();
        const caret = Math.min(focus.caret ?? field.value.length, field.value.length);
        try { field.setSelectionRange(caret, caret); } catch { /* Not a text field. */ }
    }

    function fieldOf(formId, name) {
        const form = formId ? document.getElementById(formId) : document.querySelector('form[data-auto-submit]');
        return form ? form.elements.namedItem(name) : null;
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', restore);
    else restore();
    if (window.Blazor && window.Blazor.addEventListener) window.Blazor.addEventListener('enhancedload', restore);
})();