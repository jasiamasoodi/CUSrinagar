function blockBrowser() {
    var BrowserAgent = navigator.userAgent + "";
    if (BrowserAgent.indexOf("UBrowser/") >= 0 || BrowserAgent.indexOf("UCBrowser/") >= 0) {
        location.href = "/Error/BrowserBlocked";
    }
}
blockBrowser();

$(document).ready(function ($) {
    $("#frmSignIn").submit(function (e) {
        if ($(this).valid()) {
            if ($("#JSSignIn").val() == "SignIn") {
                $("#JSSignIn").val("Signing In......").prop("disabled", true);
            }
            for (var name in localStorage) {
                if (localStorage.length == 0) break;
                    removeLocalStorage(name);
            }
            return true;
        } else {
            $("#JSSignIn").prop("disabled", false).val("SignIn");
            return false;
        }
    });
});