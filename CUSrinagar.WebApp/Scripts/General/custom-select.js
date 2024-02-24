$(document).ready(function () {
    bindCascadingDDL();

});

function bindCascadingDDL() {
    $(document).on('change', '.jsCollegeDDL', function (e) {
        var scope = $(this).data('scope');
        var callback = $(this).data('callback');
        fillProgrammeDDL(scope, callback);
        fillCourseDDL(scope, callback);
        fillSubjectDropDownload(scope, callback);
    });

    $(document).on('change', '.jsPrintProgrammeDDL', function (e) {
        var scope = $(this).data('scope');
        var _printProgramme = $(`.jsPrintProgrammeDDL[data-scope='${scope}'] option:selected`).val();
        $('.js-pager-table').data('otherparam1', _printProgramme);
        $('.js-exportToCSV').data('otherparam1', 'printProgramme=' + _printProgramme);

        var callback = $(this).data('callback');
        fillCourseDDL(scope, callback);
    });
    $(document).on('change', '.jsProgrammeDDL', function (e) {
        var scope = $(this).data('scope');
        var callback = $(this).data('callback');
        var _programme = $(`.jsProgrammeDDL[data-scope='${scope}'] option:selected`).val();
        var _printProgramme = getPrintProgrammeFromProgramme(_programme);
        $('.js-pager-table').data('otherparam1', _printProgramme);
        $('.js-exportToCSV').data('otherparam1', 'printProgramme=' + _printProgramme);

        fillCourseDDL(scope, callback);
    });
    $(document).on('change', '.jsProgrammecourseDDL', function (e) {
        var scope = $(this).data('scope');
        var callback = $(this).data('callback');
        var _programme = $(`.jsProgrammecourseDDL[data-scope='${scope}'] option:selected`).val();
        var _printProgramme = getPrintProgrammeFromProgramme(_programme);
        $('.js-pager-table').data('otherparam1', _printProgramme);
        $('.js-exportToCSV').data('otherparam1', 'printProgramme=' + _printProgramme);

        fillCourseWiseDDL(scope, callback);
    });

    $(document).on('change', '.jsDepartmentDDL', function (e) {
        var scope = $(this).data('scope');
        var callback = $(this).data('callback');
        fillSubjectDropDownload(scope, callback);
        fillCombinationDDL(scope, callback);
        fillBillNoDDL(scope, callback);
    });


    $(document).on('change', '.jsCourseDDL', function (e) {
        var scope = $(this).data('scope');
        var callback = $(this).data('callback');
        fillSubjectDropDownload(scope, callback);
        fillCombinationDDL(scope, callback);
    });
    $(document).on('change', '.jsSemesterDDL', function (e) {
        var scope = $(this).data('scope');
        var callback = $(this).data('callback');
        fillSubjectDropDownload(scope, callback);
        fillCombinationDDL(scope, callback);
        fillBillNoDDL(scope, callback);

    });


}

function fillProgrammeDDL(scope, callback) {
    var $targetSelect = $(`.jsProgrammeDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;

    var _college_ID = $(`.jsCollegeDDL[data-scope='${scope}']`).val();
    clearDDOptions($targetSelect);

    var _url = "/CUSrinagarAdminPanel/General/SelectListProgrammesByCollege";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { College_ID: _college_ID },
        beforeSend: function () { showLoader(); },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
            hideLoader();
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}

function fillCourseDDL(scope, callback) {
    var $targetSelect = $(`.jsCourseDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;

    var _college_ID = $(`.jsCollegeDDL[data-scope='${scope}']`).val();
    var _printProgramme = $(`.jsPrintProgrammeDDL[data-scope='${scope}'] option:selected`).val();
    var _programme = $(`.jsProgrammeDDL[data-scope='${scope}'] option:selected`).val();
    clearDDOptions($targetSelect);

    var _url = "/CUSrinagarAdminPanel/General/GetCourseList";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { College_ID: _college_ID, printProgramme: _printProgramme, programme: _programme },
        beforeSend: function () { showLoader(); },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
            hideLoader();
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}

function fillCourseWiseDDL(scope, callback) {
    var $targetSelect = $(`.jsCourseDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;

    var _college_ID = $(`.jsCollegeDDL[data-scope='${scope}']`).val();
    var _printProgramme = $(`.jsPrintProgrammeDDL[data-scope='${scope}'] option:selected`).val();
    var _programme = $(`.jsProgrammecourseDDL[data-scope='${scope}'] option:selected`).val();
    clearDDOptions($targetSelect);

    var _url = "/CUSrinagarAdminPanel/General/GetCourseList";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { College_ID: _college_ID, printProgramme: _printProgramme, programme: _programme },
        beforeSend: function () { showLoader(); },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
            hideLoader();
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}


function fillSubjectDropDownload(scope, callback) {
    if ($(".jsVWSCSubjectDDL") != null && $(".jsVWSCSubjectDDL").length > 0 && $(`.jsVWSCSubjectDDL[data-scope='${scope}']`) != null && $(`.jsVWSCSubjectDDL[data-scope='${scope}']`).length > 0) {
        fillVWSCSubjectDDL(scope, callback);
    }
    if ($(".jsSubjectDDL") != null && $(".jsSubjectDDL").length > 0 && $(`.jsSubjectDDL[data-scope='${scope}']`) != null && $(`.jsSubjectDDL[data-scope='${scope}']`).length > 0) {
        fillSubjectDDL(scope, callback);
    }
    if ($(".jsSubjectCompactDDL") != null && $(".jsSubjectCompactDDL").length > 0 && $(`.jsSubjectCompactDDL[data-scope='${scope}']`) != null && $(`.jsSubjectCompactDDL[data-scope='${scope}']`).length > 0) {
        fillSubjectCompactDDL(scope, callback);
    }
}

function fillVWSCSubjectDDL(scope, callback) {
    var $targetSelect = $(`.jsVWSCSubjectDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;
    clearDDOptions($targetSelect);
    //fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];

    var college_id = $(`.jsCollegeDDL[data-scope='${scope}']`).find('option:selected').val() || $(".jsCollegeDDL").val();
    if (!isNullOrEmpty(college_id))
        _param.Filters.push({ Column: "College_ID", Operator: "EqualTo", Value: college_id, GroupOperation: "AND", TableAlias: "" });

    var printProgmme = $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(printProgmme))
        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgmme, GroupOperation: "AND", TableAlias: "" });

    var programme = $(`.jsProgrammeDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(programme))
        _param.Filters.push({ Column: "Programme", Operator: "EqualTo", Value: programme, GroupOperation: "AND" });

    var course_id = $(`.jsCourseDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "" });

    var course_id = $(`.jsDepartmentDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Department_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "" });

    var semester = $(`.jsSemesterDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "" });

    var combination = $(`.jsCombinationDDL[data-scope='${scope}']`);
    if (combination != null && combination.length > 0 && !isNullOrEmpty($(`.jsCombinationDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsCombinationDDL[data-scope='${scope}']`).val()))
        _param.Filters.push({ Column: combination.data('column'), Operator: combination.data('operator') || "EqualTo", Value: combination.val(), GroupOperation: combination.data('groupoperation'), TableAlias: combination.data('tablealias') });

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectType,SubjectFullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    var _url = "/CUSrinagarAdminPanel/General/VWSCSubjectDDLWithDetail";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        beforeSend: function () { showLoader(); },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}

function fillSubjectDDL(scope, callback) {
    var $targetSelect = $(`.jsSubjectDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;
    clearDDOptions($targetSelect);
    //fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];

    var printProgmme = $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(printProgmme))
        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgmme, GroupOperation: "AND", TableAlias: "" });

    var $element = $(`.jsProgrammeDDL[data-scope='${scope}']`); //.find('option:selected').val() || $(`.jsProgrammeDDL[data-scope='${scope}']`).val();
    if ($element != null && $element.length > 0 && !isNullOrEmpty($element.find('option:selected').val() || $element.val()))
        _param.Filters.push({ Column: $element.data('column'), Operator: $element.data('operator') || "EqualTo", Value: $element.val() || $element.find('option:selected').val(), GroupOperation: $element.data('groupoperation'), TableAlias: $element.data('tablealias') });

    //if (!isNullOrEmpty(programme))
    //    _param.Filters.push({ Column: "Programme", Operator: combination.data('operator') || "EqualTo", Value: programme, GroupOperation: "AND", TableAlias: "ADMCourseMaster" });

    //var combination = $(`.jsCombinationDDL[data-scope='${scope}']`);
    //if (combination != null && combination.length > 0 && !isNullOrEmpty($(`.jsCombinationDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsCombinationDDL[data-scope='${scope}']`).val()))
    //    _param.Filters.push({ Column: combination.data('column'), Operator: combination.data('operator') || "EqualTo", Value: combination.val(), GroupOperation: combination.data('groupoperation'), TableAlias: combination.data('tablealias') });

    var $department_id = $(`.jsDepartmentDDL[data-scope='${scope}']`);
    if ($department_id.length > 0 && (!isNullOrEmpty($department_id.find('option:selected').val()) || !isNullOrEmpty($department_id.val())))
        _param.Filters.push({ Column: "Department_ID", Operator: "EqualTo", Value: $department_id.find('option:selected').val() || $department_id.val(), GroupOperation: "AND", TableAlias: $department_id.data('tablealias') || "S" });



    var $course_id = $(`.jsCourseDDL[data-scope='${scope}']`);
    if ($course_id.length > 0 && (!isNullOrEmpty($course_id.find('option:selected').val()) || !isNullOrEmpty($course_id.val())))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: $course_id.find('option:selected').val() || $course_id.val(), GroupOperation: "AND", TableAlias: $course_id.data('tablealias') || "S" });

    var semester = $(`.jsSemesterDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsSemesterDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "" });

    var $subjectType = $(`.jsSubjectTypeDDL[data-scope='${scope}']`);
    if ($subjectType != null && $subjectType.length > 0) {
        var _value = $subjectType.find('option:selected').val() || $subjectType.val();
        if (!isNullOrEmpty(_value)) {
            _param.Filters.push({ Column: $subjectType.data('column'), Operator: $subjectType.data('operator'), Value: _value, GroupOperation: $subjectType.data('GroupOperation') || "AND", TableAlias: $subjectType.data('tablealias') });

            var collegeidApplicable = false;
            $(_value.split(',')).each(function (index, element) {
                if (element == SubjectType.GE || element == SubjectType.OE || element == SubjectType.SEC) {
                    collegeidApplicable = true;
                }
            });
            var $college = $(`.jsCollegeDDL[data-scope='${scope}']`);
            if ($college.length > 0 && collegeidApplicable && $college.find('option:selected').val().length > 0)
                _param.Filters.push({ TableAlias: $college.data('tablealias'), Column: "College_ID", Operator: "EqualTo", Value: $college.find('option:selected').val() || $college.val(), GroupOperation: "AND" });
        }
    }

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };
    fillSubjectDDLAction($targetSelect, _param, callback);
}

function fillBillNoDDL(scope, callback) {
    var $targetSelect = $(`.jsBillNoDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;


    var _param = new Parameter();
    _param.Filters = [];
    var userId = $(`.jsUserId[data-scope='${scope}']`).find('option:selected').val() || $(`.jsUserId[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(userId))
        _param.Filters.push({ Column: "User_ID", Operator: "EqualTo", Value: userId, GroupOperation: "AND", TableAlias: "" });
    var printProgmme = $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(printProgmme))
        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgmme, GroupOperation: "AND", TableAlias: "" });

    var $element = $(`.jsProgrammeDDL[data-scope='${scope}']`); //.find('option:selected').val() || $(`.jsProgrammeDDL[data-scope='${scope}']`).val();
    if ($element != null && $element.length > 0 && !isNullOrEmpty($element.find('option:selected').val() || $element.val()))
        _param.Filters.push({ Column: $element.data('column'), Operator: $element.data('operator') || "EqualTo", Value: $element.val() || $element.find('option:selected').val(), GroupOperation: $element.data('groupoperation'), TableAlias: $element.data('tablealias') });


    var $department_id = $(`.jsDepartmentDDL[data-scope='${scope}']`);
    if ($department_id.length > 0 && (!isNullOrEmpty($department_id.find('option:selected').val()) || !isNullOrEmpty($department_id.val())))
        _param.Filters.push({ Column: "Department_ID", Operator: "EqualTo", Value: $department_id.find('option:selected').val() || $department_id.val(), GroupOperation: "AND", TableAlias: $department_id.data('tablealias') || "SM" });



    var $course_id = $(`.jsCourseDDL[data-scope='${scope}']`);
    if ($course_id.length > 0 && (!isNullOrEmpty($course_id.find('option:selected').val()) || !isNullOrEmpty($course_id.val())))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: $course_id.find('option:selected').val() || $course_id.val(), GroupOperation: "AND", TableAlias: $course_id.data('tablealias') || "S" });

    var semester = $(`.jsSemesterDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsSemesterDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "" });

    var $subjectType = $(`.jsSubjectTypeDDL[data-scope='${scope}']`);
    if ($subjectType != null && $subjectType.length > 0) {
        var _value = $subjectType.find('option:selected').val() || $subjectType.val();
        if (!isNullOrEmpty(_value)) {
            _param.Filters.push({ Column: $subjectType.data('column'), Operator: $subjectType.data('operator'), Value: _value, GroupOperation: $subjectType.data('GroupOperation') || "AND", TableAlias: $subjectType.data('tablealias') });

            var collegeidApplicable = false;
            $(_value.split(',')).each(function (index, element) {
                if (element == SubjectType.GE || element == SubjectType.OE || element == SubjectType.SEC) {
                    collegeidApplicable = true;
                }
            });
            var $college = $(`.jsCollegeDDL[data-scope='${scope}']`);
            if ($college.length > 0 && collegeidApplicable && $college.find('option:selected').val().length > 0)
                _param.Filters.push({ TableAlias: $college.data('tablealias'), Column: "College_ID", Operator: "EqualTo", Value: $college.find('option:selected').val() || $college.val(), GroupOperation: "AND" });
        }
    }

    _param.SortInfo = { ColumnName: "BillNo" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "BillNo" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };
    fillBillNoDDLAction($targetSelect, _param, callback);
}

function fillSubjectDDLAction($targetSelect, _param, callback) {
    var _url = "/CUSrinagarAdminPanel/General/SubjectDDLWithDetail";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        beforeSend: function () { showLoader(); },
        success: function (data) {
            clearDDOptions($targetSelect);
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}

function fillBillNoDDLAction($targetSelect, _param, callback) {
    var _url = "/CUSrinagarAdminPanel/General/BillDDL";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        beforeSend: function () { showLoader(); },
        success: function (data) {
            clearDDOptions($targetSelect);
            fillDDLOptions($targetSelect, data);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}
function fillSubjectCompactDDL(scope, callback) {
    var $targetSelect = $(`.jsSubjectCompactDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;
    clearDDOptions($targetSelect);
    //fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];

    var printProgmme = $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(printProgmme))
        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgmme, GroupOperation: "AND", TableAlias: "" });

    var programme = $(`.jsProgrammeDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(programme))
        _param.Filters.push({ Column: "Programme", Operator: "EqualTo", Value: programme, GroupOperation: "AND", TableAlias: "" });

    var course_id = $(`.jsCourseDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });

    var semester = $(`.jsSemesterDDL[data-scope='${scope}']`).find('option:selected').val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "" });

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    var _url = "/CUSrinagarAdminPanel/General/SubjectListCompact";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        beforeSend: function () { showLoader(); },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}

function fillCombinationDDL(scope, callback) {
    var $targetSelect = $(`.jsCombinationDDL[data-scope='${scope}']`);
    if ($targetSelect != null && $targetSelect.length == 0) return;
    clearDDOptions($targetSelect);
    //fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];
    var college_id = $(`.jsCollegeDDL[data-scope='${scope}']`).find('option:selected').val() || $(".jsCollegeDDL").val();
    if (!isNullOrEmpty(college_id))
        _param.Filters.push({ Column: "College_ID", Operator: "EqualTo", Value: college_id, GroupOperation: "AND", TableAlias: "" });
    var printProgmme = $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsPrintProgrammeDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(printProgmme))
        _param.Filters.push({ Column: "PrintProgramme", Operator: "EqualTo", Value: printProgmme, GroupOperation: "AND", TableAlias: "" });
    var programme = $(`.jsProgrammeDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsProgrammeDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(programme))
        _param.Filters.push({ Column: "Programme", Operator: "EqualTo", Value: programme, GroupOperation: "AND", TableAlias: "" });
    var course_id = $(`.jsCourseDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsCourseDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "" });
    var semester = $(`.jsSemesterDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsSemesterDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "" });
    var batch = $(`.jsBatchDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsBatchDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(batch))
        _param.Filters.push({ Column: "Batch", Operator: "EqualTo", Value: batch, GroupOperation: "AND", TableAlias: "" });
    var status = $(`.jsStatusDDL[data-scope='${scope}']`).find('option:selected').val() || $(`.jsStatusDDL[data-scope='${scope}']`).val();
    if (!isNullOrEmpty(status))
        _param.Filters.push({ Column: "status", Operator: "EqualTo", Value: status, GroupOperation: "AND", TableAlias: "" });

    _param.SortInfo = { ColumnName: "CombinationCode" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "CombinationCode" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    var _url = "/CUSrinagarAdminPanel/General/CombinationDDL";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        beforeSend: function () { showLoader(); },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
            hideLoader();
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}

function resizeMultiselect($selects) {
    if ($selects.length == 0) {
        $selects = $('select.multiselect');
    }
    $selects.each(function () {
        var $this = $(this);
        var $container = $this.closest(".jsDDLContainer")
        $container.find('.multiselect-search').css({ 'width': ($container.find("select").width() - 70) });
        $container.find('.multiselect-search').closest('.input-group').css({ 'padding-left': 10 });
    });
}

function resetMultiSelect($container) {
    var $multiselects = ($container != null && $container.find('.multiselect').length > 0) ? $container.find('.multiselect') : $(".multiselect");
    if ($multiselects.length > 0) {
        $multiselects.multiselect({
            enableFiltering: true,
            enableHTML: true,
            enableCaseInsensitiveFiltering: true,
            buttonClass: 'btn btn-white btn-primary',
            templates: {
                button: '<button type="button" class="multiselect dropdown-toggle" data-toggle="dropdown"><span class="multiselect-selected-text"></span> &nbsp;<b class="fa fa-caret-down"></b></button>',
                ul: '<ul class="multiselect-container dropdown-menu" style = "overflow:auto;max-height:500px;" ></ul>',
                filter: '<li class="multiselect-item filter"><div class="input-group"><span class="input-group-addon"><i class="fa fa-search"></i></span><input class="form-control multiselect-search" type="text"></div></li>',
                filterClearBtn: '<span class="input-group-btn"><button class="btn btn-default btn-white btn-grey multiselect-clear-filter" type="button"><i class="fa fa-times-circle red2"></i></button></span>',
                li: '<li><a tabindex="0"><label></label></a></li>',
                divider: '<li class="multiselect-item divider"></li>',
                liGroup: '<li class="multiselect-item multiselect-group"><label></label></li>'
            }
        });
        resizeMultiselect($multiselects);
    }
}

function fillDDL($targetDDL) {
    clearDDOptions($targetDDL);
    fillDDLDefaultOption($targetDDL);
    var _dependentsonddl = $targetDDL.attr('data-dependentsonddl').split(',');
    var unfilledSubddl = false;
    var _dependentDDLs = {};
    $.each(_dependentsonddl, function (index, ddl) {
        var _$select = $("[data-ddl=" + ddl + "]").first();
        if (_$select.find('option:selected').val().length == 0 || _$select.find('option:selected').val() == "-1") {
            unfilledSubddl = true;
            return;
        }
        else {
            _dependentDDLs[ddl] = _$select.find('option:selected').val();
        }
    });
    if (unfilledSubddl) return;
    var _url = getBaseUrlAdmin() + "General/CascadeCourseDDL";  //$targetDDL.attr('data-url');
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { TargetDDLName: $targetDDL.attr('data-ddl'), DependentDDLs: _dependentDDLs },
        beforeSend: function () { showLoader() },
        success: function (data) {
            fillDDLOptions($targetDDL, data);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetDDL, null);
            showErrorMessage(msg);
            hideLoader();
        },
        complete: function () {
            hideLoader();
            if (!isNullOrEmpty(callback) && callback.length > 0) {
                window[callback]();
            }
        }
    });
}

function clearDDOptions($selects) {
    if ($selects != null) {
        $selects.each(function (index, element) {
            var $select = $(element);
            $select.find('option').remove();
        });
    }
}

function fillDDLDefaultOption($selects) {
    if ($selects != null) {
        $selects.each(function (index, element) {
            var $select = $(element);
            if ($select.attr('data-defaulttextvalue') != null) {
                var otionTextValue = $select.attr('data-defaulttextvalue').split(',')
                var val = otionTextValue[1];
                if (val == undefined) { val = ""; }
                $select.append('<option value="' + val + '">' + otionTextValue[0] + '</option>');
                if ($select.hasClass('chosen-select')) {
                    fillchosenselect($select);
                }
            }
        });
    }

}

function clearDDLDefaultOption($selects) {
    if ($selects != null) {
        $selects.each(function (index, element) {
            var $select = $(element);
            if ($select.attr('data-defaulttextvalue') != null) {
                var otionTextValue = $select.attr('data-defaulttextvalue').split(',')
                $select.append('<option ' + otionTextValue[0] + '</option>');
                if ($select.hasClass('chosen-select')) {
                    fillchosenselect($select);
                }
            }
        });
    }

}

function fillchosenselect($selects) {
    //$select.closest('.jsDDLContainer').empty().html($select);
    $('.chosen-select').chosen({ allow_single_deselect: true });
    if ($selects != null) {
        $selects.each(function (index, element) {
            var $select = $(element);
            resizechosen($select);
        });
    }

}

function fillDDLOptions($selects, data) {
    if ($selects != null) {
        $selects.each(function (index, element) {
            var $select = $(element);
            fillDDLDefaultOption($select);
            if (data != null) {
                $.each(data, function (index, item) {
                    $select.append('<option value="' + item.Value + '">' + item.Text.trim() + '</option>');
                });
            }
            $select.closest('.jsDDLContainer').empty().html($select);

            if ($select.hasClass('chosen-select')) {
                fillchosenselect($select);
            } else if ($select.hasClass('multiselect')) {
                $select.closest('.jsDDLContainer').empty().html($select);
                resetMultiSelect($select);
            }
            $select.trigger('change');
        });
    }

}

function onResizeWindow() {

    $(window).off('resize.multiselect').on('resize.multiselect', function () {
        var $selects = $('select.multiselect');
        resizeMultiselect($selects);
    }).trigger('resize.multiselect');

}

function bindmultiselectevents() {
    $(".jsDDLContainer").on('click', 'button.multiselect', function () {
        var $container = $(this).closest(".jsDDLContainer");
        setTimeout(function () { $container.find('.multiselect-search').focus(); }, 50);
    });
}