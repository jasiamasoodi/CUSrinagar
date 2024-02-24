$(document).ready(function ($) {
    $("#frmSemAdmFrm").submit(function (e) {
        $("#jsDownloadSemAdmFrm").prop("disabled", true).html('<i class="fa fa-download"></i> Working on it...');
        $("#jsPayFee").prop("disabled", true).html('<i class="fa fa-money"></i> Working on it...');
        return true;
    });

    $("#jsDownloadSemAdmFrm").click(function () {
        $("#JsClicked").val("Download");
    });

    $("#jsPayFee").click(function () {
        $("#JsClicked").val("PayFee");
    });
});