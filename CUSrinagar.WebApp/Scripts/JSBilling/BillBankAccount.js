$(document).on('change', '.jsBankisEditable', function () {
    var userid = $(this).attr("data-user");

    var _url = getBaseUrlAdmin() + "Bill/UpdateBankAccountStatus";

    $.post(_url, { User_ID: userid },
        function (response) {
            showSuccessAlertMessage(response.msg, 1000);
        }).fail(function () {
            showErrorMessage("Some error occured", 1000);
        });
});