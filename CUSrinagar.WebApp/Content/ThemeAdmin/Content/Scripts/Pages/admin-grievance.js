var isAdmin = $("#IsAdmin").val() == "True";
$(document).ready(function () {
    bindDetailViewEvents(true);
});

function bindDetailViewEvents() {
    $("#jstdUsers").on('change', 'select', function () {
        var $select = $(this);
        var val = $select.find('option:selected').val();
        var $checkbox = $select.closest('tr').find('.jsBatchChkBox');
        if (!isNullOrEmpty(val)) {
            $checkbox.prop('checked', 'checked').attr('checked', 'checked').trigger('change');
        } else {
            $checkbox.removeProp('checked').removeAttr('checked').trigger('change');
        }
    });
    $("#Message").keypress(function (e) {
        if (e.which == "13")
            $(".jsPostReply").trigger('click');
    });
    $(".jsPostReply").keypress(function () {
        if (e.which == "13")
            $(".jsPostReply").trigger('click');
    });
    $(".jsDiscardQuery").keypress(function () {
        if (e.which == "13")
            $(".jsDiscardQuery").trigger('click');
    });

    $(".jsPostReply").click(function () {
        if ($(this).hasClass('disabled')) return;
        var message = $(":input[id=Message]").val();
        if (isNullOrEmpty(message)) {
            showErrorMessage("Reply dialog box is empty");
            return;
        }
        $(this).addClass('disabled');
        postReplay(false);
    });

    $(".jsDiscardQuery").click(function () {
        if ($(this).hasClass('disabled')) return;
        var message = $(":input[id=Message]").val();
        if (isNullOrEmpty(message)) {
            showErrorMessage("Please add the reason for discarding the query");
            $(":input[id=Message]").focus();
            return;
        }
        $(this).addClass('disabled');
        postReplay(true);
    });

    function postReplay(isDiscarded) {        
        var message = $(":input[id=Message]").val();
        showWaitingDialog("Please wait ... posting your request");
        var reply = { Grievance_ID: $("#Grievance_ID").val(), Message: message, AllowViewPublic: $("#AllowViewPublic").is(":checked"), IsDiscarded: isDiscarded };
        var _url = (isAdmin ? getBaseUrlAdmin() : getBaseUrlCollege()) + "Grievance/PostGrievanceReply";
        var $customForm = createFormFromModel(reply, _url);
        $('body').append($customForm);
        $customForm.submit();
    }


    $(".jsDiscardQuery1").click(function () {
        var msg = '<h5>Are you sure you want to discard this query.</h5>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                window.location =( isAdmin ? getBaseUrlAdmin() : getBaseUrlCollege()) + "Grievance/Discard/" + $(".jsDetailViewGrievance_ID").val();
            } else if ($btn.data('response') == 'no') {
                //$(".js-update-formDetails,.js-register-student").removeClass('disabled');
            } else {
                //$(".js-update-formDetails,.js-register-student").removeClass('disabled');
            }
        });

    });
    $('.jsShowBatchUpdateDialog').click(function () {

        var entitylist = $(".jsDetailViewGrievance_ID").length > 0 ? [$(".jsDetailViewGrievance_ID").val()] : getEntityList();

        if ($("#cusUsers").find('option').length < 3)
            fillUserDLL();

        if (entitylist.length == 0) {
            showAlertDialog('Please choose atleast one record...');
            return false;
        }
        $("#BatchUpdateGrievanceDailog").modal('show');
    });

    $('.jsBatchUpdate').click(function () {
        var $batchDialog = $("#BatchUpdateGrievanceDailog");
        var selectedOperations = $batchDialog.find(".jsBatchChkBox:checked");
        if (selectedOperations.length == 0) {
            showAlertDialog('Please check the checbox for the operation to update...');
            return false;
        }
        $("#BatchUpdateGrievanceDailog").modal('hide');
        var entitylist = $(".jsDetailViewGrievance_ID").length > 0 ? [$(".jsDetailViewGrievance_ID").val()] : getEntityList();
        var batchUpdate = { Grievance_IDs: entitylist, UpdateUserAssigned: true, UserAssigned_ID: $("#BatchUpdateGrievanceDailog #cusUsers").find('option:selected').val() };
        BatchUpdate(batchUpdate);
    });

}

function BatchUpdate(batchUpdate) {
    var _url = (isAdmin ? getBaseUrlAdmin() : getBaseUrlCollege() )+ "Grievance/BatchUpdate";
    var model = JSON.parse(JSON.stringify(batchUpdate));
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(model),
        dataType: "json",
        success: function (data) {
            showSuccessMessage(data.NumberOfRecordsEffected + ' record(s) updated successfully.');
            if ($(".jsDetailViewGrievance_ID").length > 0)
                window.location.reload();
            else
                $(".js-search").click();
        },
        beforeSend: function () { showLoader(); },
        complete: function () {
            hideLoader();
        },
        error: function (xhr, error, msg) {
        }
    });
}

function fillUserDLL() {
    var _url =  "/CUSrinagarAdminPanel/General/SelectListAppUser";
    var _param = new Parameter();
    _param.PageInfo.PageNumber = _param.PageInfo.PageSize = -1;
    _param.SortInfo.ColumnName = "FullName";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(_param),
        success: function (data) {
            var $select = $("#cusUsers");
            clearDDOptions($select);
            $select.append('<option value="">-- Select --</option>');
            if (data != null) {
                $.each(data, function (index, item) {
                    $select.append('<option value="' + item.Value + '">' + item.Text.trim() + '</option>');
                });
            }
            $select.closest('.jsDDLContainer').empty().html($select);
            $('.chosen-select').chosen({ allow_single_deselect: true });
            resizechosen($select);
        },
        error: function (xhr, error, msg) {
            var $select = $("#cusUsers");
            clearDDOptions($select);
            $select.append('<option value="">-- Select --</option>');
            $('.chosen-select').chosen({ allow_single_deselect: true });
            resizechosen($select);
            showErrorMessage(msg);
        }
    });
}
