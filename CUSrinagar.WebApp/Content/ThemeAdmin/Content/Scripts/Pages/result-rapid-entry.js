$(document).ready(function () {
    //loadpagertableWithDefaultParams();


    $(document).on('change', '.jsNewResult', function () {
        var $element = $(this);
        var $td = $element.closest('td');
        if ($element.is(':checked')) {
            $td.find('[name=ResultNotification_ID]').val('');
            $td.find('[name=ExamForm_ID]').val('');
        } else {
            $td.find('[name=ResultNotification_ID]').val($td.find('.jsResultNotification_ID').val());
            $td.find('[name=ExamForm_ID]').val($td.find('.jsExamForm_ID]').val());
        }
    });

    $('#jsShowFileDialog').click(function () {
        var validateMessage = validateFilters();
        var $batchDialog = $("#jsFileDialog");

        if (!isNullOrEmpty(validateMessage)) {
            showErrorMessage(validateMessage);
            return;
        }        
        $batchDialog.modal('show');
    });


    $('#FromFileUpdator').submit(function () {
        var $batchDialog = $("#jsFileDialog");
        if (!$batchDialog.find('form').valid()) {
            return false;
        }
        showLoader();
        //var model = createModelFromForm($batchDialog);
        //var _url = getBaseUrlAdmin() + "Result/PostResultListItem";
        //$.ajax({
        //    url: _url,
        //    type: "POST",
        //    data: { model: _model },
        //    success: function (response) {
        //        if (response.IsSuccess) {
        //            bindModelPropertiesToForm($tr, response.ResponseObject);
        //            showSuccessAlertMessage(response.SuccessMessage);
        //            resolve($tr);
        //        } else {
        //            showErrorMessage(response.ErrorMessage);
        //            reject($tr);
        //        }
        //    },
        //    error: function (xhr, error, msg) {
        //        showErrorMessage(msg);
        //        reject($tr);
        //    },
        //    complete: function () { }
        //});
        setTimeout(function () {
            $batchDialog.modal('hide');
            hideLoader();
        }, 5000);
        return true;
    });

});

function saveRapidEntryRow($tr, resolve, reject) {
    var _model = createModelFromForm($tr);
    var _url = getBaseUrlAdmin() + "Result/PostResultListItem";
    $.ajax({
        url: _url,
        type: "POST",
        data: { model: _model },
        success: function (response) {
            if (response.IsSuccess) {
                bindModelPropertiesToForm($tr, response.ResponseObject);
                showSuccessAlertMessage(response.SuccessMessage,500);
                resolve($tr);
            } else {
                showErrorMessage(response.ErrorMessage);
                reject($tr);
            }
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
            reject($tr);
        },
        complete: function () { }
    });
}


function validateRows($pagertable) {
    if ($pagertable != null && $pagertable.length > 0 && $pagertable.find('form').length > 0) {
        generalFormValidation();
    }
}



function validateFilters() {

    var $programme = $(".js-pager-table .jsProgrammeDDL");
    var $semester = $(".js-pager-table .jsSemesterDDL");
    var $batch = $(".js-pager-table .jsBatchDDL");
    var $College = $(".js-pager-table .jsCollegeDDL");
    var $Course = $(".js-pager-table .jsCourseDDL ");
    var $Subject = $(".js-pager-table .jsVWSCSubjectDDL");
    $('#lblDailogPrintProgramme').html($programme.find('option:selected').text());
    var _printProgramme = getPrintProgrammeFromProgramme($programme.find('option:selected').val());
    $('#FileOfPrintProgramme').val(_printProgramme);

    $('#lblDailogSemester').html($semester.find('option:selected').text());
    $('#FileOfSemester').val($semester.find('option:selected').val());

    $('#lblDailogBatch').html($batch.find('option:selected').text());
    $('#FileOfBatch').val($batch.find('option:selected').val());

    $('#lblDailogCollege_ID').html($College.find('option:selected').text());
    $('#FileOfCollege_ID').val($College.find('option:selected').val());

    $('#lblDailogCourse_ID').html($Course.find('option:selected').text());
    $('#FileOfCourse_ID').val($Course.find('option:selected').val());

    $('#lblDailogSubject_ID').html($Subject.find('option:selected').text());
    $('#FileOfSubject_ID').val($Subject.find('option:selected').val());

    return null;
}

