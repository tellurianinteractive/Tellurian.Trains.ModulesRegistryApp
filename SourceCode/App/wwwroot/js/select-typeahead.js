// Typeahead for a select list: a search field that filters the list's options as you type (WAI-ARIA combobox pattern with a
// listbox popup, as person-search.js). Markup (rendered by AppInputSelect.razor with Typeahead):
//   <select data-typeahead data-typeahead-placeholder="..." data-typeahead-not-found="...">
//     <option value="0">Select ...</option>
//     <option value="12" data-code="72031000" data-name="Steel coils">7203 Steel coils (Shimmns)</option>
//   </select>
// The script hides the select and shows a search field with a list of matching options. An option matches when the search
// is the start of its code (digits, spaces ignored) or is found anywhere in its name (the option's text when it has no name),
// ignoring case. Choosing an option selects it in the hidden select, which is what the form posts, and raises a 'change' event.
// The field always shows the selected option when it loses focus, so what's seen is what's saved. Without JavaScript the select
// works as usual.
// Keys: ArrowDown/ArrowUp move (ArrowDown opens), Enter chooses (also the only match), Escape closes, Tab closes.
(function () {
    'use strict';

    function normalized(text) {
        return (text || '').trim().toLowerCase();
    }

    // Exposed for the tests: does an option with this code and name match the search?
    function matches(code, name, query) {
        const search = normalized(query);
        if (search === '') return true;
        const digits = search.replace(/\s+/g, '');
        if (code && /^\d+$/.test(digits) && code.startsWith(digits)) return true;
        return normalized(name).includes(search);
    }

    function init() {
        document.querySelectorAll('select[data-typeahead]:not([data-typeahead-ready])').forEach(enhance);
    }

    function enhance(select) {
        select.setAttribute('data-typeahead-ready', '');
        const id = select.id || `typeahead-${Math.random().toString(36).slice(2)}`;
        const wrapper = document.createElement('div');
        wrapper.className = 'typeahead';
        const input = document.createElement('input');
        input.type = 'text';
        input.id = `${id}-typeahead`;
        input.className = 'form-control';
        input.autocomplete = 'off';
        input.spellcheck = false;
        input.placeholder = select.dataset.typeaheadPlaceholder || '';
        input.disabled = select.disabled;
        input.setAttribute('role', 'combobox');
        input.setAttribute('aria-autocomplete', 'list');
        input.setAttribute('aria-expanded', 'false');
        const list = document.createElement('ul');
        list.id = `${id}-typeahead-list`;
        list.className = 'typeahead-list list-group';
        list.setAttribute('role', 'listbox');
        list.hidden = true;
        input.setAttribute('aria-controls', list.id);
        const notFound = document.createElement('div');
        notFound.className = 'form-text';
        notFound.textContent = select.dataset.typeaheadNotFound || '';
        notFound.hidden = true;
        wrapper.append(input, list, notFound);
        select.insertAdjacentElement('afterend', wrapper);
        select.hidden = true;
        const label = select.id ? document.querySelector(`label[for="${select.id}"]`) : null;
        if (label) {
            label.htmlFor = input.id;
            list.setAttribute('aria-label', label.textContent.trim());
        }
        input.value = selectedText(select);
        input.defaultValue = input.value;

        input.addEventListener('input', function () { filter(select, input, list, notFound, input.value); });
        input.addEventListener('click', function () { if (list.hidden) openAll(select, input, list, notFound); });
        input.addEventListener('focusout', function () { close(input, list, notFound); input.value = selectedText(select); });
        input.addEventListener('keydown', function (event) {
            const isOpen = !list.hidden;
            switch (event.key) {
                case 'ArrowDown':
                    event.preventDefault();
                    if (!isOpen) openAll(select, input, list, notFound);
                    moveActive(input, list, 1);
                    break;
                case 'ArrowUp':
                    if (!isOpen) return;
                    event.preventDefault();
                    moveActive(input, list, -1);
                    break;
                case 'Enter': {
                    if (!isOpen) return;
                    event.preventDefault(); // Don't submit the form while choosing.
                    const option = activeOption(input) || (list.children.length === 1 ? list.children[0] : null);
                    if (option) choose(select, input, list, notFound, option);
                    break;
                }
                case 'Escape':
                    if (!isOpen) return;
                    event.preventDefault();
                    close(input, list, notFound);
                    input.value = selectedText(select);
                    break;
                case 'Tab':
                    close(input, list, notFound);
                    break;
            }
        });
        // mousedown (not click) so the field keeps focus; preventDefault stops the focus from moving to the list.
        list.addEventListener('mousedown', function (event) {
            const option = event.target instanceof Element ? event.target.closest('[data-value]') : null;
            if (!option) return;
            event.preventDefault();
            choose(select, input, list, notFound, option);
        });
    }

    // Opening with the selected option's text in the field shows all options; otherwise those matching the text.
    function openAll(select, input, list, notFound) {
        filter(select, input, list, notFound, input.value === selectedText(select) ? '' : input.value);
    }

    function filter(select, input, list, notFound, query) {
        const options = Array.prototype.filter.call(select.options, function (option) {
            return option.value !== '0' && option.value !== '' && matches(option.dataset.code, option.dataset.name || option.text, query);
        });
        const items = options.map(function (option, index) {
            const item = document.createElement('li');
            item.id = `${list.id}-${index}`;
            item.className = 'list-group-item list-group-item-action';
            item.setAttribute('role', 'option');
            item.setAttribute('aria-selected', 'false');
            item.dataset.value = option.value;
            item.textContent = option.text;
            return item;
        });
        list.replaceChildren(...items);
        setActive(input, list, null);
        notFound.hidden = items.length > 0;
        list.hidden = items.length === 0;
        input.setAttribute('aria-expanded', items.length > 0 ? 'true' : 'false');
    }

    function choose(select, input, list, notFound, item) {
        if (select.value !== item.dataset.value) {
            select.value = item.dataset.value;
            select.dispatchEvent(new Event('change', { bubbles: true }));
        }
        input.value = selectedText(select);
        close(input, list, notFound);
    }

    function close(input, list, notFound) {
        list.hidden = true;
        notFound.hidden = true;
        input.setAttribute('aria-expanded', 'false');
        setActive(input, list, null);
    }

    function moveActive(input, list, step) {
        if (list.children.length === 0) return;
        const items = Array.from(list.children);
        const current = items.indexOf(activeOption(input));
        const next = current < 0 ? (step > 0 ? 0 : items.length - 1) : (current + step + items.length) % items.length;
        setActive(input, list, items[next]);
    }

    function setActive(input, list, item) {
        for (const other of list.children) {
            other.classList.remove('active');
            other.setAttribute('aria-selected', 'false');
        }
        if (item) {
            item.classList.add('active');
            item.setAttribute('aria-selected', 'true');
            item.scrollIntoView({ block: 'nearest' });
            input.setAttribute('aria-activedescendant', item.id);
        } else {
            input.removeAttribute('aria-activedescendant');
        }
    }

    function activeOption(input) {
        const id = input.getAttribute('aria-activedescendant');
        return id ? document.getElementById(id) : null;
    }

    // The "Select ..." option (value 0) shows as an empty field with the placeholder.
    function selectedText(select) {
        const option = select.options[select.selectedIndex];
        return option && option.value !== '0' && option.value !== '' ? option.text : '';
    }

    window.SelectTypeahead = { matches: matches };

    if (typeof document.readyState === 'undefined') return; // The tests load the script without a page.
    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
    else init();
    if (window.Blazor && window.Blazor.addEventListener) window.Blazor.addEventListener('enhancedload', init);
})();