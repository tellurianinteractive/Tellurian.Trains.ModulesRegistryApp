// One-time messages after a POST on static SSR pages (Migration step 3.3), rendered by FlashMessage in the layout:
//   <div class="alert flash-message">...</div>
// With JavaScript the message moves into the page heading, just left of its help and back icons (the slot in PageHeading),
// or is a pop-up under the top bar on a page without one, and fades out after a few seconds; without, it stays as a banner
// until the next page.
(function () {
    'use strict';
    const showMilliseconds = 5000;
    const fadeMilliseconds = 500;

    function handle() {
        document.querySelectorAll('.flash-message:not(.flash-message-popup):not(.flash-message-inline)').forEach(function (message) {
            const slot = document.querySelector('.flash-message-slot');
            if (slot) {
                slot.appendChild(message);
                message.classList.add('flash-message-inline');
            } else {
                message.classList.add('flash-message-popup');
            }
            setTimeout(function () {
                message.classList.add('flash-message-hidden');
                setTimeout(function () { message.remove(); }, fadeMilliseconds);
            }, showMilliseconds);
        });
    }

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', handle);
    else handle();
    if (window.Blazor && window.Blazor.addEventListener) window.Blazor.addEventListener('enhancedload', handle);
})();