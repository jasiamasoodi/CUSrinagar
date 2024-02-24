window.oncontextmenu = function () {
    return false;
}
$(document).ready(function ($) {
    $("#frmAdmitCard").submit(function (e) {
        if ($(this).valid()) {
            $("#jsAdmitCard").prop("disabled", true).html('<i class="fa fa-download"></i> Working on it...');
            return true;
        }
        $("#jsAdmitCard").prop("disabled", false).html('<i class="fa fa-download"></i> Download Admit Card');
        return false;
    });
});