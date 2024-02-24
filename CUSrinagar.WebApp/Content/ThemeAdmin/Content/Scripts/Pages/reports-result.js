$(document).ready(function () {
    if ($("#SubjectWiseFormFilters").length > 0) {
        bindSubjectWiseScreenEvents();
    } else if ($("#Result-notification-list-admin").length > 0) {
        bindResultNotificationAdminScreenEvents();
    } else {
        bindResultListScreenEvents();
    }
});

function bindResultNotificationAdminScreenEvents() {
    loadpagertableWithDefaultParams();
    $(document).on('click', '.jsImportToMasterTable', function () {
        var $tr = $(this).closest('tr');
        var msg = '<h4>Are you sure you want to import the result to master table ?</h4>';
        msg += '<h5>After that result cannot be modified</h5>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var _resultNotification_ID = $tr.find('[id$=ResultNotification_ID]').val();
                var _url = getBaseUrlAdmin() + "ResultNotification/ImportToMasterTable";
                $.ajax({
                    url: _url,
                    type: "POST",
                    datatype: "Json",
                    beforeSend: function () { showProgressBar();},
                    data: { ResultNotification_ID: _resultNotification_ID },
                    success: function (responseData) {
                        if (responseData.IsSuccess) {
                            showSuccessMessage(responseData.NumberOfRecordsEffected + ' records imported successfully.');
                            $(".js-search").click();
                        } else {
                            showErrorMessage(responseData.ErrorMessage);
                        }
                    },
                    error: function (xhr, error, msg) {
                        showErrorMessage(msg);
                    },
                    complete: function () { hideProgressBar(); }
                });

            }
        });
    });
}

function bindSubjectWiseScreenEvents() {

    //loadpagertableWithDefaultParams();
    // getCourseDDL();

    //$("#CollegeDDL").change(function () {
    //    getCourseDDL();
    //});
    //$("#PrintProgrammeDDL").change(function () {
    //    getCourseDDL();
    //});
    //$(document).on('change', "#CourseDDL", function () {
    //    getSubjectWithTypeDDL();
    //});
    //$(document).on('change', "#SemesterDDL", function () {
    //    getSubjectWithTypeDDL();
    //});
}

function bindResultListScreenEvents() {

    //loadpagertableWithDefaultParams();
    //getCourseDDL();

    //$("#CollegeDDL").change(function () {
    //    getCourseDDL();
    //});
    //$("#PrintProgrammeDDL").change(function () {
    //    getCourseDDL();
    //});
    //$(document).on('change', "#CourseDDL", function () {
    //    getSubjectDDL();
    //});
    //$(document).on('change', "#SemesterDDL", function () {
    //    getSubjectDDL();
    //});

    function getresultstatistics($pagertable) {
        var _url = getBaseUrlCollege() + "Result/GetResultSummary?PrintProgramme=" + $("#PrintProgrammeDDL").find('option:selected').val() + "&Semester=" + $("#SemesterDDL").find('option:selected').val();
        var _tableParam = getpagertableparameter($pagertable);
        $.ajax({
            url: _url,
            type: "POST",
            contentType: 'application/json; charset=UTF-8',
            data: JSON.stringify(_tableParam),
            dataType: "json",
            success: function (data) {
                if (data != null) {
                    $(".jsItem1Count").html(data.TotalStudent);
                    $(".jsItem2Count").html(data.TotalAppeared);
                    $(".jsItem3Count").html(data.Other);
                }
            },
            beforeSend: function () { },
            complete: function () {
            },
            error: function (xhr, error, msg) {
            }
        });
    }
}

//function getSubjectDDL() {
//    var $targetSelect = $("#SubjectDDL");
//    clearDDOptions($targetSelect);
//    fillDDLDefaultOption($targetSelect);
//    var course_id = $("#CourseDDL").find('option:selected').val();
//    //if (isNullOrEmpty(course_id) || course_id == emptyGuid()) {
//    //    return;
//    //}
//    var _param = new Parameter();
//    _param.Filters = [];
//    if (!isNullOrEmpty(course_id))
//        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });
//    var college_id = $("#CollegeDDL").find('option:selected').val();
//    if (!isNullOrEmpty(college_id) && college_id != emptyGuid())
//        _param.Filters.push({ Column: "College_ID", Operator: "EqualTo", Value: college_id, GroupOperation: "AND", TableAlias: "ADMCollegeCourseMapping" });
//    var semester = $("#SemesterDDL").find('option:selected').val();
//    if (!isNullOrEmpty(semester))
//        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });
//    var printProgramme = $("#PrintProgrammeDDL").find('option:selected').val();
//    if (!isNullOrEmpty(printProgramme))
//        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgramme, GroupOperation: "AND", TableAlias: "ADMCourseMaster" });
//    _param.SortInfo = { ColumnName: "SubjectFullName" };
//    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };
//    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

//    clearDDOptions($targetSelect);
//    var _url = "/CUSrinagarAdminPanel/General/SubjectDDL";
//    $.ajax({
//        url: _url,
//        type: "POST",
//        datatype: "Json",
//        data: _param,
//        success: function (data) {
//            fillDDLOptions($targetSelect, data);
//            ChosenStyle();
//            resizechosen($targetSelect);
//        },
//        error: function (xhr, error, msg) {
//            fillDDLOptions($targetSelect, null);
//            showErrorMessage(msg);
//        }
//    });
//}



function getresultstatistics($pagertable) {
    var _url = getBaseUrlCollege() + "Result/GetResultSummary?PrintProgramme=" + $("#PrintProgrammeDDL").find('option:selected').val() + "&Semester=" + $("#SemesterDDL").find('option:selected').val();
    var _tableParam = getpagertableparameter($pagertable);
    return;
    $.ajax({
        url: _url,
        type: "POST",
        contentType: 'application/json; charset=UTF-8',
        data: JSON.stringify(_tableParam),
        dataType: "json",
        success: function (data) {
            if (data != null) {
                $(".jsItem1Count").html(data.TotalStudent);
                $(".jsItem2Count").html(data.TotalAppeared);
                $(".jsItem3Count").html(data.Other);
            }
        },
        beforeSend: function () { },
        complete: function () {
        },
        error: function (xhr, error, msg) {
        }
    });
}


//--------------------- upload entrance result --------------------
$("#frmUploadExamResult #PrintCenterNotice_CourseCategory,#frmUploadEntranceResult #PrintCenterNotice_ExaminationYear").change(function () {

    var Id = $("#frmUploadExamResult #PrintCenterNotice_CourseCategory").val();
    var Id1 = $("#frmUploadExamResult #PrintCenterNotice_ExaminationYear").val();
    if (Id !== "" && Id1 !== "") {
        $.ajax({
            async: true,
            url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesOMRDDL/" + Id + "/" + Id1,
            type: "POST",
            data: {},
            datatype: "html",
            success: function (response) {
                $("#frmUploadEntranceResult .CourseID").empty().html(response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
            }
        });
    }
});

//---------------- upload result --------------------
$("#frmUploadEntranceResult").submit(function (e) {
    if ($(this).valid()) {
        if (confirm("Are you sure you want to  upload Exam Result?")) {
            $('.JSUploadExamResultBtn').html('Working...').prop("disabled", true);
            return true;
        }
    }
    return false;
});
