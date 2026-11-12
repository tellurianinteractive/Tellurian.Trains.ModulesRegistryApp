// Person search: an editable combobox with a list of suggestions from the /app/persons/search endpoint
// (WAI-ARIA combobox pattern with a listbox popup). Markup (rendered by AppSearchPerson.razor):
//   <input role="combobox" aria-expanded="false" aria-controls="list-id" data-person-search="/app/persons/search?countryId=..."
//          data-min-length="3" data-list="list-id" data-value-input="hidden-id" data-not-found="id" data-searching="id">
//   <ul id="list-id" role="listbox" hidden></ul>
//   <input type="hidden" id="hidden-id">
// The server does the matching (every word must match), so suggestions are shown as returned, without browser filtering.
// Choosing a suggestion writes the person's id to the hidden input and raises a 'change' event, which Blazor handles;
// in a static SSR form the hidden input is posted. Typing after a choice clears it.
// Keys: ArrowDown/ArrowUp move (ArrowDown opens), Enter chooses, Escape closes, Tab closes. Clicking an option chooses it.
(function () {
    'use strict';
    const debounceMilliseconds = 250;
    const timers = new WeakMap();
    const latestQueries = new WeakMap();

    document.addEventListener('input', function (event) {
        const input = searchInput(event.target);
        if (!input) return;
        if (isSelected(input) && input.value !== input.dataset.selectedName) setPerson(input, '0', '');
        const willSearch = isSearchable(input);
        clearTimeout(timers.get(input));
        show(input.dataset.searching, willSearch);
        show(input.dataset.notFound, false);
        if (willSearch) {
            timers.set(input, setTimeout(function () { search(input); }, debounceMilliseconds));
        } else {
            latestQueries.delete(input);
            renderOptions(input, []);
        }
    });

    document.addEventListener('keydown', function (event) {
        const input = searchInput(event.target);
        if (!input) return;
        const list = listOf(input);
        const isOpen = list !== null && !list.hidden;
        switch (event.key) {
            case 'ArrowDown':
                event.preventDefault();
                if (!isOpen) open(input);
                moveActive(input, 1);
                break;
            case 'ArrowUp':
                if (!isOpen) return;
                event.preventDefault();
                moveActive(input, -1);
                break;
            case 'Enter': {
                if (!isOpen) return;
                event.preventDefault(); // Don't submit the form while choosing.
                const active = activeOption(input);
                if (active) choose(input, active);
                break;
            }
            case 'Escape':
                if (!isOpen) return;
                event.preventDefault();
                close(input);
                break;
            case 'Tab':
                close(input);
                break;
        }
    });

    // mousedown (not click) so the input keeps focus; preventDefault stops the focus from moving to the list.
    document.addEventListener('mousedown', function (event) {
        const option = event.target instanceof Element ? event.target.closest('[data-person-id]') : null;
        if (!option) return;
        const input = document.querySelector(`[data-list="${option.parentElement.id}"]`);
        if (!input) return;
        event.preventDefault();
        choose(input, option);
    });

    document.addEventListener('focusout', function (event) {
        const input = searchInput(event.target);
        if (input) close(input);
    });

    async function search(input) {
        if (!isSearchable(input)) return;
        const query = input.value.trim();
        latestQueries.set(input, query);
        let persons = [];
        try {
            const url = input.dataset.personSearch + (input.dataset.personSearch.includes('?') ? '&' : '?') + 'q=' + encodeURIComponent(query);
            const response = await fetch(url, { credentials: 'same-origin', headers: { 'Accept': 'application/json' } });
            const contentType = response.headers.get('Content-Type') || '';
            if (response.ok && contentType.includes('application/json')) persons = await response.json();
        } catch {
            persons = [];
        }
        if (latestQueries.get(input) !== query) return; // A newer search has started; keep showing 'searching'.
        show(input.dataset.searching, false);
        show(input.dataset.notFound, persons.length === 0);
        renderOptions(input, persons);
        if (persons.length > 0 && document.activeElement === input) open(input);
    }

    function renderOptions(input, persons) {
        const list = listOf(input);
        if (!list) return;
        const options = persons.map(function (person, index) {
            const option = document.createElement('li');
            option.id = `${list.id}-${index}`;
            option.className = 'list-group-item list-group-item-action';
            option.setAttribute('role', 'option');
            option.setAttribute('aria-selected', 'false');
            option.dataset.personId = person.id;
            option.textContent = person.name;
            return option;
        });
        list.replaceChildren(...options);
        if (options.length === 0) close(input);
        setActive(input, null);
    }

    function open(input) {
        const list = listOf(input);
        if (!list || list.children.length === 0) return;
        list.hidden = false;
        input.setAttribute('aria-expanded', 'true');
    }

    function close(input) {
        const list = listOf(input);
        if (list) list.hidden = true;
        input.setAttribute('aria-expanded', 'false');
        setActive(input, null);
    }

    function moveActive(input, step) {
        const list = listOf(input);
        if (!list || list.children.length === 0) return;
        const options = Array.from(list.children);
        const current = options.indexOf(activeOption(input));
        const next = current < 0 ? (step > 0 ? 0 : options.length - 1) : (current + step + options.length) % options.length;
        setActive(input, options[next]);
    }

    function setActive(input, option) {
        const list = listOf(input);
        if (list) {
            for (const item of list.children) {
                item.classList.remove('active');
                item.setAttribute('aria-selected', 'false');
            }
        }
        if (option) {
            option.classList.add('active');
            option.setAttribute('aria-selected', 'true');
            option.scrollIntoView({ block: 'nearest' });
            input.setAttribute('aria-activedescendant', option.id);
        } else {
            input.removeAttribute('aria-activedescendant');
        }
    }

    function activeOption(input) {
        const id = input.getAttribute('aria-activedescendant');
        return id ? document.getElementById(id) : null;
    }

    function choose(input, option) {
        input.value = option.textContent;
        setPerson(input, option.dataset.personId, option.textContent);
        clearTimeout(timers.get(input));
        latestQueries.delete(input);
        show(input.dataset.searching, false);
        show(input.dataset.notFound, false);
        close(input);
    }

    function setPerson(input, id, name) {
        input.dataset.selectedName = name;
        const hidden = document.getElementById(input.dataset.valueInput);
        if (!hidden || hidden.value === id) return;
        hidden.value = id;
        hidden.dispatchEvent(new Event('change', { bubbles: true }));
    }

    function isSearchable(input) {
        const minLength = parseInt(input.dataset.minLength || '3', 10);
        return input.value.trim().length >= minLength && !isSelected(input);
    }

    function isSelected(input) {
        const hidden = document.getElementById(input.dataset.valueInput);
        return hidden !== null && hidden.value !== '' && hidden.value !== '0';
    }

    function searchInput(target) {
        return target instanceof HTMLInputElement && target.dataset.personSearch ? target : null;
    }

    function listOf(input) {
        return input.dataset.list ? document.getElementById(input.dataset.list) : null;
    }

    function show(elementId, visible) {
        const element = elementId ? document.getElementById(elementId) : null;
        if (element) element.hidden = !visible;
    }
})();