$(document).ready(function () {
    //loadpagertableWithDefaultParams();

    $("#PrintProgramme").change(function () {
        var $select = $(this);
        var selectedValue = $select.find('option:selected').val();
        $select.closest('.js-pager-table').data('otherparam1', selectedValue);
        $(".js-exportToCSV").data('otherparam1', 'printProgramme=' + selectedValue);
    });

    $(document).on('click keypress', '.js-accept-frm', function (e) {
        if (e.type == "click" && $(e.target).is('button') || e.which == "13") {
            var $form = $(this).closest('form');
            if (validateForm($form) && !$form.hasClass('under-process')) {
                $form.addClass('under-process');
                postStudentExamForm($form);
            }
        }
    });

    $(document).on('keypress', '#SearchRegNoFormNumber', function (e) {
        if (e.which == "13") {
            var value = $(this).val();
            if (value.length > 0) {
                $(".js-search").trigger('click');
            }
        }
    });

});

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
        //$(".jsTotalStudentInProcess").html(data.InProcess);
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