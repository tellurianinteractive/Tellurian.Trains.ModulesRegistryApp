// Asks before leaving a static SSR form with unsaved changes (Migration step 3.6). EditTemplate renders, on a static form:
//   <form data-leave-guard="question" data-leave-yes="Yes" data-leave-no="No" data-leave-unsaved="true|false">
// A form is unsaved when a field differs from the value the page was rendered with (the browser keeps it as defaultValue,
// defaultChecked and defaultSelected), or when the server says so (a form shown again after a POST: a successful save redirects).
// - A click on a link (Blazor's enhanced navigation, or a full load) is stopped in the capture phase and a dialog asks the
//   question "stay on the page?" with Yes and No; Yes (the default, or Escape) stays, No follows the link and drops the changes.
// - Closing the tab, reloading or typing an address gets the browser's own warning (beforeunload); its text can't be set.
// - Not covered: the browser's back and forward buttons within enhanced navigation (Blazor handles them; they can't be cancelled).
(function () {
    'use strict';
    let leaving = false;

    function isChanged(element) {
        if (!element.name || element.name.startsWith('_')) return false; // _handler, __RequestVerificationToken
        const type = (element.type || '').toLowerCase();
        if (type === 'hidden' || type === 'submit' || type === 'button' || type === 'reset') return false;
        if (type === 'checkbox' || type === 'radio') return element.checked !== element.defaultChecked;
        if (element.options) {
            const options = Array.prototype.slice.call(element.options);
            // A single list with no option marked selected shows its first option: the browser's default, not a change.
            if (!element.multiple && options.length > 0 && !options.some(function (o) { return o.defaultSelected; })) return !options[0].selected;
            return options.some(function (o) { return o.selected !== o.defaultSelected; });
        }
        return element.value !== element.defaultValue;
    }

    function isDirty(form) {
        return form.dataset.leaveUnsaved === 'true' || Array.prototype.some.call(form.elements, isChanged);
    }

    function dirtyForm() {
        return Array.prototype.find.call(document.querySelectorAll('form[data-leave-guard]'), isDirty) || null;
    }

    function isNavigation(event, link) {
        if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return false;
        if (link.hasAttribute('download') || (link.target && link.target !== '_self' && link.target !== '_top')) return false;
        const href = link.getAttribute('href') || '';
        if (href === '' || href.startsWith('#') || href.startsWith('javascript:') || href.startsWith('mailto:') || href.startsWith('tel:')) return false;
        return true;
    }

    function ask(form, onLeave) {
        const dialog = document.createElement('dialog');
        dialog.className = 'leave-guard-dialog';
        const question = document.createElement('p');
        question.textContent = form.dataset.leaveGuard;
        const yes = document.createElement('button');
        yes.type = 'button';
        yes.className = 'btn btn-sm btn-primary';
        yes.textContent = form.dataset.leaveYes || 'Yes';
        const no = document.createElement('button');
        no.type = 'button';
        no.className = 'btn btn-sm btn-warning';
        no.textContent = form.dataset.leaveNo || 'No';
        const buttons = document.createElement('div');
        buttons.className = 'leave-guard-buttons';
        buttons.append(yes, no);
        dialog.append(question, buttons);
        document.body.appendChild(dialog);
        dialog.addEventListener('close', function () { dialog.remove(); });
        yes.addEventListener('click', function () { dialog.close(); });
        no.addEventListener('click', function () { dialog.close(); onLeave(); });
        dialog.showModal();
        yes.focus(); // Staying is the safe default.
    }

    document.addEventListener('click', function (event) {
        if (leaving) return;
        const link = event.target instanceof Element ? event.target.closest('a[href]') : null;
        if (!link || !isNavigation(event, link)) return;
        const form = dirtyForm();
        if (!form) return;
        event.preventDefault();
        event.stopImmediatePropagation();
        ask(form, function () {
            leaving = true;
            link.click(); // Followed as usual now, by Blazor or the browser.
        });
    }, true);

    window.addEventListener('beforeunload', function (event) {
        if (leaving || !dirtyForm()) return;
        event.preventDefault();
        event.returnValue = '';
    });

    // A new page (enhanced navigation keeps this script) starts guarded again.
    if (window.Blazor && window.Blazor.addEventListener) window.Blazor.addEventListener('enhancedload', function () { leaving = false; });

    window.LeaveGuard = { isDirty: isDirty }; // For the tests.
})();