$(document).ready(function ($) {
    $("#frmSemAdmitCard").submit(function (e) {
        if ($(this).valid()) {
            $("#jsSemAdmitCard").prop("disabled", true).html('<i class="fa fa-download"></i> Working on it...');
            return true;
        }
        $("#jsSemAdmitCard").prop("disabled", false).html('<i class="fa fa-download"></i> Download Admit Card');
        return false;
    });
});