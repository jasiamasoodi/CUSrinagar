$(document).ready(function () {

    bindDetailViewEvents();


});

function disableAllElements() {
    if ($("#DisableAllElement").val() == true) {
        $("#GrievanceForm :input").attr('disabled', 'disabled');
    }
}
function bindDetailViewEvents() {

    disableAllElements();


    $(".jsMainForm").submit(function (event) {
        var $form = $(this);
        if (!$form.valid()) return false;
        $form.find("#PhoneNumber").val($form.find("#PhoneNumber").cleanVal());
        return true;
    });


    $(".jsPostReply").keypress(function () {
        if (e.which == "13")
            $(".jsPostReply").trigger('click');
    });

    $(".jsPostReply").click(function () {
        if ($(".jsPostReply").hasClass('disabled')) return;
        var message = $(":input[id=Message]").val();
        if (isNullOrEmpty(message)) {

            return;
        }
        $(".jsPostReply").addClass('disabled');
        showWaitingDialog("Please wait ... posting your request");
        var reply = { Grievance_ID: $("#Grievance_ID").val(), Message: message };
        var _url = getBaseUrlWebApp() + "Grievance/PostGrievanceReply";
        var $customForm = createFormFromModel(reply, _url);
        $('body').append($customForm);
        $customForm.submit();

    });

    $("#Category").change(function () {
        var $category = $(this);
        var category = $category.find('option:selected').val();
        if (category == "5") {
            $("#College_ID").attr('required', 'required');
            $(".jsCollegeList").show();
        } else {
            $("#College_ID").removeAttr('required');
            $(".jsCollegeList").hide();
        }
    });


}

function validateForm($form) {
    var isValidForm = true;
    $form.find(':input').each(function (index, element) {
        var $element = $(element);
        var value = "";
        if ($element.attr('required')) {
            if ($element.is('select')) {
                if ($element.hasClass('multiselect')) {
                    var _ids = "";
                    $element.find('option:selected').each(function (index, option) {
                        if ($(option).val().trim().length > 0) {
                            _ids += "" + $(option).val().trim() + ",";
                        }
                    });
                    value = _ids.slice(0, -1)
                } else {
                    value = $element.find('option:selected').val();
                }
            } else if ($element.is('checkbox')) {
                value = $element.is('checked');
            } else if ($element.is('radio')) {
                value = $element.val();
            } else {
                value = $element.val();
            }
            if (value == "") {
                $element.addClass('error');
                isValidForm = false;
            }
        }
    });
    return isValidForm;
}

function examformlistloaded() {
    var _url = getBaseUrlCollege() + "Examination/GetExaminationSummary?otherParam1=" + $("#PrintProgramme").find('option:selected').val();
    var $pagertable = $('.js-pager-table');
    var _tableParam = getpagertableparameter($pagertable);
    var param = JSON.parse(JSON.stringify(_tableParam));
    param.Filters = param.Filters.filter(function (fltr, index) { return fltr.Column != "Status"; });
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(param),
        dataType: "json",
        success: function (data) {
            setSummaryInfoBars(data);
        },
        beforeSend: function () { },
        complete: function () {
        },
        error: function (xhr, error, msg) {
        }
    });
}

function setSummaryInfoBars(data) {
    if (data != null) {
        $(".jsTotalStudentLabel").html('Total ' + data.PrintProgrammeDescription + ' Students');
        $(".jsTotalStudent").html(data.TotalStudent);
        $(".jsTotalStudentAccepted").html(data.Accepted);
        $(".jsTotalStudentInProcess").html(data.InProcess);
        $(".jsTotalStudentNotDownloaded").html(data.NotDownloaded);
    }
}

function postStudentExamForm($form) {
    var studentExamForm = {};
    $form.find(':input').each(function (index, element) {
        var $element = $(element);
        var name = $element.attr('name');
        if ($element.is('select')) {
            if ($element.hasClass('multiselect')) {
                var _ids = "";
                $element.find('option:selected').each(function (index, option) {
                    if ($(option).val().trim().length > 0) {
                        _ids += "" + $(option).val().trim() + ",";
                    }
                });
                value = _ids.slice(0, -1)
            } else {
                value = $element.find('option:selected').val();
            }
        } else if ($element.is('checkbox')) {
            value = $element.is('checked');
        } else if ($element.is('radio')) {
            value = $element.val();
        } else {
            value = $element.val();
        }
        var value = $element.val();
        studentExamForm[name] = value;
    });

    var _url = getBaseUrlCollege() + "Examination/PostExamForm?printProgramme=" + $("#PrintProgramme").find('option:selected').val();
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(studentExamForm),
        dataType: "json",
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessMessage(responseData.NumberOfRecordsEffected + " " + responseData.SuccessMessage);
            } else {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        beforeSend: function () { showLoader(); $("#FormNumber").trigger('focus'); },
        complete: function () {
            hideLoader();
            $form.removeClass('under-process');
            hideConfirmationDialog();
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}