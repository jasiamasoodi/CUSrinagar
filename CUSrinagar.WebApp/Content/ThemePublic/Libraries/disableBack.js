history.pushState(null, document.title, location.href);
window.addEventListener('popstate', function (event) { history.pushState(null, document.title, location.href); });
window.oncontextmenu = function () {
    return false;
}
jQuery(document).ready(function ($) {
    jQuery(document).keydown(function (event) {
        if (event.keyCode == 123) { // Prevent F12
            return false;
        } else if (event.ctrlKey && event.shiftKey && event.keyCode == 73) { // Prevent Ctrl+Shift+I        
            return false;
        }
    });
});


