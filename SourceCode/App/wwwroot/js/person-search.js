// Person search: a text input with a <datalist>, filled from the /app/persons/search endpoint.
// Markup (rendered by AppSearchPerson.razor):
//   <input data-person-search="/app/persons/search?countryId=..." data-min-length="3" list="..." data-value-input="id-of-hidden" data-not-found="id-of-element">
//   <datalist id="..."></datalist>
//   <input type="hidden" id="id-of-hidden">
// When the text matches a suggestion, the person's id is written to the hidden input and a 'change' event is raised,
// which Blazor handles; in a static SSR form the hidden input is posted.
(function () {
    'use strict';
    const debounceMilliseconds = 250;
    const timers = new WeakMap();
    const latestQueries = new WeakMap();

    document.addEventListener('input', function (event) {
        const input = event.target;
        if (!(input instanceof HTMLInputElement) || !input.dataset.personSearch) return;
        selectMatchingPerson(input);
        clearTimeout(timers.get(input));
        timers.set(input, setTimeout(function () { search(input); }, debounceMilliseconds));
    });

    async function search(input) {
        const query = input.value.trim();
        const list = input.list;
        const minLength = parseInt(input.dataset.minLength || '3', 10);
        if (!list) return;
        if (query.length < minLength || isSelected(input)) {
            if (query.length < minLength) replaceOptions(list, []);
            showNotFound(input, false);
            return;
        }
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
        if (latestQueries.get(input) !== query) return; // A newer search has started.
        replaceOptions(list, persons);
        showNotFound(input, persons.length === 0);
        selectMatchingPerson(input);
    }

    function replaceOptions(list, persons) {
        const options = persons.map(function (person) {
            const option = document.createElement('option');
            option.value = person.name;
            option.dataset.id = person.id;
            return option;
        });
        list.replaceChildren(...options);
    }

    function isSelected(input) {
        const hidden = document.getElementById(input.dataset.valueInput);
        return hidden !== null && hidden.value !== '' && hidden.value !== '0';
    }

    function selectMatchingPerson(input) {
        const hidden = document.getElementById(input.dataset.valueInput);
        if (!hidden || !input.list) return;
        const match = Array.from(input.list.options).find(function (option) { return option.value === input.value; });
        const id = match ? match.dataset.id : '0';
        if (hidden.value === id) return;
        hidden.value = id;
        hidden.dispatchEvent(new Event('change', { bubbles: true }));
        if (match) showNotFound(input, false);
    }

    function showNotFound(input, visible) {
        const element = document.getElementById(input.dataset.notFound);
        if (element) element.hidden = !visible;
    }
})();