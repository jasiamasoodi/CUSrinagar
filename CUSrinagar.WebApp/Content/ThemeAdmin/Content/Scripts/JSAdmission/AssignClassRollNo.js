window.oncontextmenu = function () {
    return false;
}
$(document).ready(function ($) {
    $(".JSAssignCRN").submit(function (e) {
        if ($(this).valid()) {
            var ConfirmMsg = "Are you sure, you want to Proceed?";
            if (confirm(ConfirmMsg)) {
                $(".JSUpdateClassRollNo").prop("disabled", true).html('<i class="fa fa-eye"></i>&nbsp; Working on it...');
                return true;
            } else {
                return false;
            }
        }
        $(".JSUpdateClassRollNo").prop("disabled", false).html('<i class="fa fa-eye"></i>&nbsp; Assign &amp; Save');
        return false;
    });
});