$(document).ready(function ($) {
    $("#frmSFS").submit(function (e) {       
        if ($(this).valid()) {
            $(".JSprogress").empty().html("Working...");
            return true;
        }
        return false;
    });

    $("#jsSFReprint").click(function () {
        $(".JSprogress").empty().html('<b class="btn btn-success">Working...</b>');
    });
});