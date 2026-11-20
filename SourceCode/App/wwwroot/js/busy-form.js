// A message while a slow form is posted on a static SSR page (Migration step 3.6), such as generating waybills:
//   <form method="post" data-busy="id-of-message"> ... </form>  and  <div id="id-of-message" hidden>...</div>
// On submit the message is shown and the form marked busy, and a second submit of it is ignored. The page that comes back has the
// message hidden again (rendered by the server), so nothing needs to be reset. Without JavaScript the form posts as usual, only
// without the message. Runs in the capture phase, before Blazor's enhanced form handling.
(function () {
    'use strict';
    document.addEventListener('submit', function (event) {
        const form = event.target;
        if (!(form instanceof HTMLFormElement) || !form.dataset.busy) return;
        if (form.classList.contains('form-busy')) {
            event.preventDefault();
            event.stopImmediatePropagation();
            return;
        }
        form.classList.add('form-busy');
        form.setAttribute('aria-busy', 'true');
        const message = document.getElementById(form.dataset.busy);
        if (message) message.hidden = false;
    }, true);
})();