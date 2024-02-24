$(document).on('click', '.JSChangeStatus', function () {
    $('#ModalBillStatus').modal('show');
    $(".JSChangeBillStatus").attr("id", $(this).attr("id"));
});
$(document).on('click', '.JSChangeBillStatus', function (e) {
    var _url = getBaseUrlAdmin() + "BillSetter/ChangeBillStatus";
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
                if (response.status == false && response.msg.indexOf("Account Details not mentioned") != -1) {
                    alert(response.msg);
                    window.location.href = "/CUSrinagarAdminPanel/BillAccount/BillAccountDetails";
                } else {
                    showSuccessAlertMessage(response.msg, 3000);
                    window.location.reload();
                }
            }).fail(function () {
                showErrorMessage("Some error occured", 5000);
            });
        return false;
    } else {
        return false;
    }
});