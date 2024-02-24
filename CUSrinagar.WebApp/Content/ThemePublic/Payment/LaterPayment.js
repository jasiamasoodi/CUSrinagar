$(document).ready(function ($) {
    $("#frmLaterPayment").submit(function (e) {
        if ($(this).valid()) {
            $("#jsLaterPayment").prop("disabled", true).html('<i class="fa fa-money"></i> Working on it...');
            return true;
        }
        $("#jsLaterPayment").prop("disabled", false).html('<i class="fa fa-money"></i> Make Payment Online');
        return false;
    });
});