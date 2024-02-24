$(document).ready(function () {
    loadpagertableWithDefaultParams();


    $(document).on("change", ".jsAllowDownloadForm", function () {
        var checked = $(this).is(":checked");
        var _examformSetting = createModelFromForm($(this).closest('tr'));
        if (checked) {
            $("#AllowFormsModal").modal('show');
            $("#AllowFormsModal").data('examformSetting', _examformSetting);
        } else {
            _examformSetting.AllowDownloadForm = false;
            UpdateNotificationParam(_examformSetting);
        }
    });

    $(document).on("click", ".jscancelModel", function () {
        var _examformSetting = $("#AllowFormsModal").data('examformSetting');
        var $element = $(`[value=${_examformSetting.ARGExamForm_ID}]`).closest('tr').find('.jsAllowDownloadForm');
        $element.removeAttr('checked').removeProp('checked');
        $("#Model_RegularBatch").val('');
    });
    $(document).on("click", ".jsfeeCancelModel", function () {
        var _examformSetting = $("#HardCodeFeeModal").data('examformSetting');
        var $element = $(`[value=${_examformSetting.ARGExamForm_ID}]`).closest('tr').find('.jsIsHardCodeFeeSet');
        $element.removeAttr('checked').removeProp('checked');
    });

    $('#AllowFormsModalBtn').click(function () {
        var _examformSetting = $("#AllowFormsModal").data('examformSetting');
        _examformSetting.AllowDownloadForm = true;
        _examformSetting.StartDate = $('#AllowFormsModal #Model_StartDate').val();
        _examformSetting.EndDate = $('#AllowFormsModal #Model_EndDate').val();
        _examformSetting.Notification_ID = $('#AllowFormsModal #Model_Notification_ID').val();
        _examformSetting.RegularBatch = $('#AllowFormsModal #Model_RegularBatch').val();
        $("#err").text('');
        if (_examformSetting.RegularBatch === "" || Number($('#AllowFormsModal #Model_RegularBatch').val()) < 2016) {
            $("#err").text("Regular Batch is Invalid")
            return;
        }
        UpdateNotificationParam(_examformSetting);
        $("#Model_RegularBatch").val('');
    });


    $(document).on("change", ".jsIsHardCodeFeeSet", function () {
        var checked = $(this).is(":checked");
        var _examformSetting = createModelFromForm($(this).closest('tr'));
        if (checked) {
            $("#HardCodeFeeModal").modal('show');
            $("#HardCodeFeeModal").data('examformSetting', _examformSetting);
        } else {
            _examformSetting.IsHardCodeFeeSet = false;
            UpdateNotificationParam(_examformSetting);
        }
    });
    $('#HardCodeFeeModalBtn').click(function () {
        var _examformSetting = $("#HardCodeFeeModal").data('examformSetting');
        _examformSetting.HardCodeFeeAmount = $('#HardCodeFeeModal #Model_HardCodeFeeAmount').val();
        _examformSetting.IsHardCodeFeeSet = true;
        UpdateNotificationParam(_examformSetting);
    });

    $(document).on("change", ".jsAllowDownloadAdmitCards", function () {
        var checked = $(this).is(":checked");
        var _examformSetting = createModelFromForm($(this).closest('tr'));
        _examformSetting.AllowDownloadAdmitCards = checked;
        UpdateNotificationParam(_examformSetting);
    });

    $(document).on("change", ".jsAllowInCenterAllotment", function () {
        var checked = $(this).is(":checked");
        var _examformSetting = createModelFromForm($(this).closest('tr'));
        _examformSetting.AllowInCenterAllotment = checked;
        UpdateNotificationParam(_examformSetting);
    });

});

function UpdateNotificationParam(_ExaminationSetting) {
    var _url = getBaseUrlAdmin() + "ExamFormSetting/UpdateNotificationParam";
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        dataType: "json",
        data: JSON.stringify(_ExaminationSetting),
        success: function (data) {
            if (data.IsSuccess) {
                showSuccessAlertMessage(data.NumberOfRecordsEffected + " " + data.SuccessMessage, 300);
                var $element = $(`[value=${data.ResponseObject.ARGExamForm_ID}]`).closest('tr').find('.jsAllowDownloadForm');
                $element.val(data.ResponseObject.AllowDownloadForm);
                if (data.ResponseObject.AllowDownloadForm) {
                    $element.attr('checked', 'checked').prop('checked', 'checked');
                } else {
                    $element.removeAttr('checked').removeProp('checked');
                }
                var $element = $(`[value=${data.ResponseObject.ARGExamForm_ID}]`).closest('tr').find('.jsIsHardCodeFeeSet');
                $element.val(data.ResponseObject.IsHardCodeFeeSet);
                if (data.ResponseObject.IsHardCodeFeeSet) {
                    $element.attr('checked', 'checked').prop('checked', 'checked');
                } else {
                    $element.removeAttr('checked').removeProp('checked');
                }
                var $element = $(`[value=${data.ResponseObject.ARGExamForm_ID}]`).closest('tr').find('.lblhardcodefeeamount');
                $element.val(data.ResponseObject.HardCodeFeeAmount);

                var $element = $(`[value=${data.ResponseObject.ARGExamForm_ID}]`).closest('tr').find('.jsAllowDownloadAdmitCards');
                $element.val(data.ResponseObject.AllowDownloadAdmitCards);
                if (data.ResponseObject.AllowDownloadAdmitCards) {
                    $element.attr('checked', 'checked').prop('checked', 'checked');
                } else {
                    $element.removeAttr('checked').removeProp('checked');
                }
                $(`[value=${data.ResponseObject.ARGExamForm_ID}]`).closest('tr').find('.regBatch').text('').text(_ExaminationSetting.RegularBatch);
            } else {
                var $element = $(`[value=${data.ResponseObject.ARGExamForm_ID}]`).closest('tr').find('.jsAllowDownloadForm');
                $element.removeAttr('checked').removeProp('checked');
            }
            showErrorMessage(data.ErrorMessage, 10000);
            //$(".js-search").click();
        },
        beforeSend: function () { showLoader(); },
        complete: function () {
            hideLoader();
            $("#AllowFormsModal").modal('hide');
            $("#HardCodeFeeModal").modal('hide');
        },
        error: function (xhr, error, msg) {
        }
    });
}