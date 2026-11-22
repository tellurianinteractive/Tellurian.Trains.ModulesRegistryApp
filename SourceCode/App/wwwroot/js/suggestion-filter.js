// Suggestions that depend on other fields (AppInputText with SuggestionsFilteredBy, 2026-11-22). Markup:
//   <input list="x" data-suggestions="x"> <datalist id="x" data-filter-by="Form.A Form.B"> <option value="v" data-filter="a|b"> ...
// The field gets a second datalist with only the options whose filter values match the named fields (an empty field matches all),
// without duplicates. The server's datalist is never changed, so it is always complete, also after an enhanced update; without
// JavaScript, the field uses it and all values are suggested.
(function () {
    'use strict';

    function apply(list) {
        const form = list.closest('form');
        if (!form) return;
        const input = form.querySelector('input[data-suggestions="' + CSS.escape(list.id) + '"]');
        if (!input) return;
        const values = list.getAttribute('data-filter-by').split(' ').filter(Boolean).map(function (name) {
            const field = form.elements.namedItem(name);
            return field && 'value' in field ? field.value.trim().toLowerCase() : '';
        });
        let filtered = document.getElementById(list.id + '-filtered');
        if (!filtered) {
            filtered = document.createElement('datalist');
            filtered.id = list.id + '-filtered';
            list.after(filtered);
        }
        const seen = new Set();
        const options = [];
        for (const option of list.options) {
            const filters = (option.getAttribute('data-filter') || '').split('|');
            if (!values.every(function (value, i) { return value === '' || (filters[i] || '').toLowerCase() === value; })) continue;
            const key = option.value.toLowerCase();
            if (seen.has(key)) continue;
            seen.add(key);
            const copy = document.createElement('option');
            copy.value = option.value;
            options.push(copy);
        }
        filtered.replaceChildren(...options);
        input.setAttribute('list', filtered.id);
    }

    function applyAll() {
        document.querySelectorAll('datalist[data-filter-by]').forEach(apply);
    }

    function changed(event) {
        const name = event.target && event.target.name;
        if (!name) return;
        document.querySelectorAll('datalist[data-filter-by]').forEach(function (list) {
            if (list.getAttribute('data-filter-by').split(' ').includes(name)) apply(list);
        });
    }

    document.addEventListener('input', changed);
    document.addEventListener('change', changed);
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', applyAll);
    else applyAll();
    if (window.Blazor && window.Blazor.addEventListener) window.Blazor.addEventListener('enhancedload', applyAll);
})();