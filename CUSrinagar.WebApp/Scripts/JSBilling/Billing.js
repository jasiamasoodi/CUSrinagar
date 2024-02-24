


$(document).on('click', '.JSIsActive', function (e) {
    var _url = getBaseUrlAdmin() + "Bill/UpdatePaperSettingStatus";
    var ans = confirm("Are you sure ,you want to Change Status.");
    if (ans) {
        $.post(_url, { id: $(this).attr("id") },
            function (response) {
                var $pagetable = $(".js-pager-table");
                loadpagertable($pagetable);
                showSuccessAlertMessage(response.msg, 1000);

            }).fail(function () {
                showErrorMessage("Some error occured", 1000);
            });
        return true;
    } else {
        return false;
    }
});
$(document).on('click', '.JSDeleteBill', function () {
    var ans = confirm("Are you sure you want to delete this Bill?");
    var _url = getBaseUrlAdmin() + "Bill/DeleteBill";
    if (ans) {
        $.post(_url, { BillNo: $(this).attr("id") },
            function (response) {
                var $pagetable = $(".js-pager-table");
                loadpagertable($pagetable);
                showSuccessAlertMessage(response.msg, 1000);

            }).fail(function () {
                showErrorMessage("Some error occured", 1000);
            });
        return true;
    } else {
        return false;
    }
});
$(document).on('click', '.JSRemoveBill', function () {
    var ans = confirm("Are you sure you want to remove this paper?");
    var _url = getBaseUrlAdmin() + "Bill/RemoveBill";
    if (ans) {
        $.post(_url, { id: $(this).attr("id") },
            function (response) {
                var $pagetable = $(".js-pager-table");
                loadpagertable($pagetable);
                showSuccessAlertMessage(response.msg, 1000);

            }).fail(function () {
                showErrorMessage("Some error occured", 1000);
            });
        return true;
    } else {
        return false;
    }
});
$(document).on('click', '.JSChangeStatus', function () {
    $('#ModalBillStatus').modal('show');
    $(".JSChangeBillStatus").attr("id", $(this).attr("id"));
});
$(document).on('click', '.JSChangeBillStatus', function (e) {
    var _url = getBaseUrlAdmin() + "Bill/ChangeBillStatus";
    var ans = confirm("Are you sure ,you want to Change Status.");
    var bilno = $(this).attr("id");
    var billstatus = $("#ModalBillStatus #BillStatus").val();
    var _param = new Parameter();
    if (bilno != "") {
        var filter = { Column: "BillNo", Operator: "Equal", Value: bilno };
        _param.Filters.push(filter);
    }
    else {
        _param = $(".js-pager-table").data('parameter');
    }
    if (ans) {
        $.post(_url, { parameter: _param, BillStatus: billstatus },
            function (response) {
                showSuccessAlertMessage(response.msg, 1000);
                window.location.reload();
            }).fail(function () {
                showErrorMessage("Some error occured", 1000);
            });
        return false;
    } else {
        return false;
    }
});

$(document).ready(function () {

    $(document).on('click', '.js-delete-row', function () {
        var msg = '<h4>Are you sure you want to delete this Paper Setting ?</h4>';
        showConfirmationDialog(msg);
        var $tr = $(this).closest('tr');
        var _id = $tr.find(".jsRow_ID").val();
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                DeleteRow(_id, $tr);
            }
        });
    });



    function DeleteRow(_id, $tr) {
        var _url = getBaseUrlAdmin() + "Bill/DeletePaperSetting";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { id: _id },
            success: function (responseData) {
                console.log(responseData);
                if (responseData.status == true) {
                    showSuccessMessage(responseData.msg);
                    $tr.remove();
                } else {
                    showErrorMessage(responseData.msg);
                }
            },
            error: function (xhr, error, msg2) {
                showErrorMessage(msg2);
            }
        });
    }

});

$(document).on('click', '[href="#ModalStamp"]', function () {
    $(".JSChnageRevenueStampCharges").attr("billno-data", $(this).attr("data-billno"));
});
$(document).on('click', '.JSChnageRevenueStampCharges', function () {
    var newAmount = $("#ModalStamp #NewAmount").val();
    if (newAmount == "") {
        $("#ModalStamp #NewAmount").focus();
        return false;
    } else {

        var ans = confirm("Are you sure you want to proceed?");
        var _url = getBaseUrlAdmin() + "Bill/ChangeRevenueStampCharges";
        if (ans) {
            $.post(_url, { BillNo: $(this).attr("billno-data"), NewAmount: newAmount },
                function (response) {
                    $(".fa-search-btn").trigger("click");
                    $(".modal-backdrop").hide();
                    showSuccessAlertMessage(response.msg, 1000);
                }).fail(function () {
                    showErrorMessage("Some error occured", 1000);
                });
            return false;
        } else {
            return false;
        }
    }
});

$(document).on('submit', '#frmFileCSV', function (e) {
    var TxnDate = $("#ModalPaidCSV #TransactionDate").val();
    var fileCSV = $('#CSVFile')[0].files[0];
    if (TxnDate == "") {
        $("#ModalPaidCSV #TransactionDate").focus();
        return false;
    } else if (fileCSV == "") {
        $("#ModalPaidCSV #CSVFile").focus();
        return false;
    } else {

        var ans = confirm("Please make sure all bills status is\nDispatched to Accounts Section and are already not Paid.\n\n Are you sure you want to proceed?");
        if (ans) {
            var arrList = fileCSV.name.split('.');
            if (arrList[arrList.length - 1].toLowerCase() != "csv") {
                alert("Only CSV file is allowed");
                return false;
            } else {
                $("#ModalPaidCSV .JSPaidClosed").prop("disabled", true);
                $("#ModalPaidCSV #JSClose").prop("disabled", true);
                return true;
            }
        } else {
            return false;
        }
    }
});
