$(document).ready(function () {
    showHideCollegeField();
    //showHideCourseField();

    $(document).on('change', "#ProgrammeDDL,#ProgramDDL", function () {
        getCourseDDL();
        fillSubjectTitles();
    });
    $("#SubjectType").change(function () {
        //showHideCourseField();
        //showHideSchool();
        //showHideCollegeField();
        setSubjectNumber();
        //setSubjectCode();
        //setMarksFields();

    });
    $("#Semester").change(function () {
        setSubjectNumber();
        fillSubjectTitles();
        //setSubjectCode();
        //setMarksFields();
        //showHideSchool();
    });
    $("#SubjectFullName").change(function () {
        setSubjectCode();
    });

    $(document).on('change', "#CourseDDL", function () {
        //getSubjectDDL();
        //showHideSchool();
        //setSubjectCode();
        //setMarksFields();
        setSubjectNumber();
    });
    $(document).on('change', "#CollegeDDL", function () {
        //getSchoolList();
        getProfessorList();
        getCRList();
        getExistingProfessorNCR();
    });
    $(document).on('change', "#SchoolFullName", function () {
        getDepartmentDataList();
    });

    $(".jsIsApplicable").change(function () {
        setApplicableInputs($(this));
    });
    $(document).on('focusOut', '#SubjectNumber', function (e) {
        //setSubjectCode();
    });
    $(document).on('focusout', '#Batch', function (e) {
        $('#CollegeDDL').val('').trigger('chosen:updated');
        $('#CRDDL').val('').trigger('chosen:updated');
        $('#ProfessorDDL').val('').trigger('chosen:updated');
    });
    $(document).on('change', '#SubjectDDL', function (e) {
        $('#CollegeDDL').val('').trigger('chosen:updated');
        $('#CRDDL').val('').trigger('chosen:updated');
        $('#ProfessorDDL').val('').trigger('chosen:updated');
    });

    $("form").submit(function () {
        if ($(this).hasClass("NotValidate"))
        { return true; }
        var $form = $(this);
        if (!$form.valid()) return false;
        if (isNullOrEmpty($("#Semester option:selected").val())) {
            showErrorMessage("Please choose semester."); return false;
        }
        var subjectType = $("#SubjectType").find('option:selected').val();
        if (subjectType == SubjectType.SEC || subjectType == SubjectType.GE || subjectType == SubjectType.OE) {
            var college_id = $("#College_ID option:selected").val();
            if (isNullOrEmpty(college_id)) {
                showErrorMessage("College name is required for the subject type " + $("#SubjectType").find('option:selected').text());
                return false;
            }
            //$("#CourseDDL").val('');
        } else {
            if (isNullOrEmpty($("#CourseDDL option:selected").val())) {
                showErrorMessage("Please choose course."); return false;
            }
        }
        $("#CourseFullName").val($("#CourseDDL option:selected").text());
        $("#CollegeFullName").val($("#College_ID option:selected").text());
    });


});

function setApplicableInputs($element) {
    //$(".jsIsApplicable")

    var $container = $element.closest('.jsGroupSection');
    var $inputElements = $container.find('input[type=number]');
    if ($element.is(":checked")) {
        $inputElements.removeAttr('readonly');//.val('0');
        $container.find('.jsIsPassComponent').removeAttr('disabled');
        $container.find("[id*=Label]").removeAttr('readonly');
        //$container.find('.jsIsPassComponent').attr('checked', 'checked').prop('checked', 'checked');
        //$container.find('.jsIsPassComponent').trigger('change');
        $inputElements.each(function (index, element) {
            if (parseInt($(element).val()) <= -5) {
                $(element).val('0');
            }
        });
    } else {
        $inputElements.attr('readonly', 'readonly').val('-9');
        $container.find("[id*=Label]").attr('readonly', 'readonly').val('');
        $container.find('.jsIsPassComponent').removeAttr('checked').attr('disabled', 'disabled').trigger('change');
    }
}

function showHideCollegeField() {
    var subjectType = $("#SubjectType").find('option:selected').val();
    if (subjectType == SubjectType.SEC || subjectType == SubjectType.GE || subjectType == SubjectType.OE) {
        //$("#jsCollegeField").removeClass('chosen-disabled disable-element');
    } else {
        $("#jsCollegeField").find('select option').removeAttr('selected');//addClass('chosen-disabled disable-element').
        $("#jsCollegeField").find('select option [value=""]').attr('selected', 'selected');
    }
    resizechosen($("#College_ID"));
}


function setSubjectNumber() {
    var course_id = $("#CourseDDL").val();
    var semester = $("#Semester").val();
    var subjectType = $("#SubjectType").val();
    var _Programme = $("#ProgrammeDDL option:selected").val();
    if (!isNullOrEmpty(subjectType) && !isNullOrEmpty(semester)) {
        var _url = "/CUSrinagarAdminPanel/Subject/GetSubjectNumberCount";
        $.ajax({
            url: _url,
            data: { course_Id: course_id, semester: semester, subjectType: subjectType, programme: _Programme, College_ID: $("#College_ID").val() },
            contentType: "application/json; charset=utf-8",
            success: function (code) {
                $('#SubjectNumber').val(code);
                setSubjectCode();
            }
        });
    }
}

function setSubjectCode() {
    if ($("#Subject_ID").val() != emptyGuid()) return;
    var _year = new Date().getFullYear().toString().substring(2, 4);
    var _Programme = $("#ProgrammeDDL").val();
    var _ProgramCode = Object.keys(Programme).filter(function (key, index) { if (Programme[key] == _Programme) return key; })[0] || "";
    var _SubjectNumber = $("#SubjectNumber").val();
    var _Semester = $("#Semester").val();
    var _CourseFullName = $("#CourseDDL option:selected").text() || "";
    if ($("#CourseDDL").val().length == 0) _CourseFullName = "";
    var _CourseCode = _CourseFullName.replace('.', '').replace('-', '').replace('_', '');
    var _SubjectType = $("#SubjectType").val();
    var _SubjectTypeCode = Object.keys(SubjectType).filter(function (key, index) { if (SubjectType[key] == _SubjectType) return key; })[0] || "";
    var _SubjectCode = _ProgramCode.substr(0, 2) + _CourseCode.substring(0, 2) + $("#SubjectFullName").val().substring(0, 3) + _year + _SubjectTypeCode.substr(0, 1) + _Semester + _SubjectNumber;
    $("#SubjectCode").val(_SubjectCode.toUpperCase());
}


function showHideSchool() {
    var _url = "/CUSrinagarAdminPanel/General/GetCourse";
    var _course_id = $("#CourseDDL option:selected").val();

    if (isNullOrEmpty(_course_id)) {
        $("#SchoolFullName").val('');
        $('#SchoolFullName').trigger("chosen:updated");
        return;
    }
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { Course_ID: _course_id },
        success: function (data) {
            if (data != null && data.SchoolFullName && data.SchoolFullName.length > 0) {
                $("#SchoolFullName").val(data.SchoolFullName);
            } else {
                $("#SchoolFullName").val("");
            }
            $('#SchoolFullName').trigger("chosen:updated");
            getDepartmentDataList();
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}

function getDepartmentDataList() {
    var _url = "/CUSrinagarAdminPanel/General/DepartmentDataList";
    var _schoolfullname = $("#SchoolFullName").find('option:selected').val();
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { SchoolFullName: _schoolfullname },
        success: function (data) {
            $("#DepartmentDataList option").remove();
            if (data != null && data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    $("#DepartmentDataList").append(`<option value="${data[i].Value}">${data[i].Text}</option>`);
                }
            }
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}

function setMarksFields() {

    var program = $("#ProgrammeDDL").val();
    var semester = $("#Semester").val();
    var course = $("#CourseDDL").val();
    var subjectType = $("#SubjectType").val();
    if (isNullOrEmpty(program) || isNullOrEmpty(semester) || isNullOrEmpty(subjectType))
        return;

    var _url = "/CUSrinagarAdminPanel/Subject/GetCourse";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { programme: program, Course_ID: course, Semester: semester, subjectType: subjectType },
        success: function (data) {
            if (data == null || data.Subject_ID == emptyGuid()) {
                if (subjectType == SubjectType.SEC)
                    data = { TotalCredit: 4, ExternalMaxMarks: 30, IsExternalMarksApplicable: true, ExternalMinPassMarks: 12, IsInternalMarksApplicable: true, IsExternalPassComponent: true, IsExternalAttendance_AssessmentApplicable: false, InternalMaxMarks: 26, InternalMinPassMarks: 12, IsInternalAttendance_AssessmentApplicable: false, IsInternalPassComponent: true };
                else {
                    data = { TotalCredit: 6, ExternalMaxMarks: 56, IsExternalMarksApplicable: true, ExternalMinPassMarks: 22, IsInternalMarksApplicable: true, IsExternalPassComponent: true, IsExternalAttendance_AssessmentApplicable: true, ExternalAttendance_AssessmentMaxMarks: 4, ExternalAttendance_AssessmentMinPassMarks: 0, InternalMaxMarks: 28, InternalMinPassMarks: 14, IsInternalAttendance_AssessmentApplicable: true, InternalAttendance_AssessmentMaxMarks: 2, IsInternalPassComponent: true };
                }
            } else {
                return true;
            }
            $("#TotalCredit").val(data.TotalCredit);

            // --------------- New Block ----------------------------//

            //coloumn-I
            if (data.IsExternalMarksApplicable) {
                $("#IsExternalMarksApplicable").siblings('.jsIsApplicable').attr('checked', 'checked').prop('checked', 'checked');
                $("#ExternalMaxMarks").val(data.ExternalMaxMarks);
                $("#ExternalMinPassMarks").val(data.ExternalMinPassMarks);
                $("#ExternalMarksLabel").val(data.ExternalMarksLabel);
                $('#ExternalIsPartOf option[value=' + data.ExternalIsPartOf + ']').attr('selected', 'selected');
                $('#ExternalVisibleTo option[value=' + data.ExternalVisibleTo + ']').attr('selected', 'selected');
                if (data.IsExternalPassComponent) {
                    $("#IsExternalPassComponent").siblings('.jsIsPassComponent').attr('checked', 'checked').prop('checked', 'checked').trigger('change');
                } else {
                    $("#IsExternalPassComponent").siblings('.jsIsPassComponent').removeAttr('checked').removeAttr('disabled').trigger('change');
                }
            } else {
                $("#IsExternalMarksApplicable").siblings('.jsIsApplicable').removeAttr('checked');
            }
            setApplicableInputs($("#IsExternalMarksApplicable").siblings('.jsIsApplicable'));
            //end column-I

            //coloumn-II
            if (data.IsExternalAttendance_AssessmentApplicable) {
                $("#IsExternalAttendance_AssessmentApplicable").siblings('.jsIsApplicable').attr('checked', 'checked').prop('checked', 'checked');
                $("#ExternalAttendance_AssessmentMaxMarks").val(data.ExternalAttendance_AssessmentMaxMarks);
                $("#ExternalAttendance_AssessmentMinPassMarks").val(data.ExternalAttendance_AssessmentMinPassMarks);
                $("#ExternalAttendance_AssessmentMarksLabel").val(data.ExternalAttendance_AssessmentMarksLabel);
                $('#ExternalAttendanceIsPartOf option[value=' + data.ExternalAttendanceIsPartOf + ']').attr('selected', 'selected');
                $('#ExternalAttendanceVisibleTo option[value=' + data.ExternalAttendanceVisibleTo + ']').attr('selected', 'selected');
                if (data.IsExternalAttendance_AssessmentPassComponent) {
                    $("#IsExternalAttendance_AssessmentPassComponent").siblings('.jsIsPassComponent').attr('checked', 'checked').prop('checked', 'checked').trigger('change');
                } else {
                    $("#IsExternalAttendance_AssessmentPassComponent").siblings('.jsIsPassComponent').removeAttr('checked').removeAttr('disabled').trigger('change');
                }
            } else {
                $("#IsExternalAttendance_AssessmentApplicable").siblings('.jsIsApplicable').removeAttr('checked');
            }
            setApplicableInputs($("#IsExternalAttendance_AssessmentApplicable").siblings('.jsIsApplicable'));
            //end column-II

            //coloumn-III
            if (data.IsInternalMarksApplicable) {
                $("#IsInternalMarksApplicable").siblings('.jsIsApplicable').attr('checked', 'checked').prop('checked', 'checked');
                $("#InternalMaxMarks").val(data.InternalMaxMarks);
                $("#InternalMinPassMarks").val(data.InternalMinPassMarks);
                $("#InternalMarksLabel").val(data.InternalMarksLabel);
                $('#InternalIsPartOf option[value=' + data.InternalIsPartOf + ']').attr('selected', 'selected');
                $('#InternalVisibleTo option[value=' + data.InternalVisibleTo + ']').attr('selected', 'selected');
                if (data.IsInternalPassComponent) {
                    $("#IsInternalPassComponent").siblings('.jsIsPassComponent').attr('checked', 'checked').prop('checked', 'checked').trigger('change');
                } else {
                    $("#IsInternalPassComponent").siblings('.jsIsPassComponent').removeAttr('checked').removeAttr('disabled').trigger('change');
                }
            } else {
                $("#IsInternalMarksApplicable").siblings('.jsIsApplicable').removeAttr('checked');
            }
            setApplicableInputs($("#IsInternalMarksApplicable").siblings('.jsIsApplicable'));
            //end column-III

            //coloumn-IV
            if (data.IsInternalAttendance_AssessmentApplicable) {
                $("#IsInternalAttendance_AssessmentApplicable").siblings('.jsIsApplicable').attr('checked', 'checked').prop('checked', 'checked');
                $("#InternalAttendance_AssessmentMaxMarks").val(data.InternalAttendance_AssessmentMaxMarks);
                $("#InternalAttendance_AssessmentMinPassMarks").val(data.InternalAttendance_AssessmentMinPassMarks);
                $("#InternalAttendance_AssessmentMarksLabel").val(data.InternalAttendance_AssessmentMarksLabel);
                $('#InternalAttendanceIsPartOf option[value=' + data.InternalAttendanceIsPartOf + ']').attr('selected', 'selected');
                $('#InternalAttendanceVisibleTo option[value=' + data.InternalAttendanceVisibleTo + ']').attr('selected', 'selected');
                if (data.IsInternalAttendance_AssessmentPassComponent) {
                    $("#IsInternalAttendance_AssessmentPassComponent").siblings('.jsIsPassComponent').attr('checked', 'checked').prop('checked', 'checked').trigger('change');
                } else {
                    $("#IsInternalAttendance_AssessmentPassComponent").siblings('.jsIsPassComponent').removeAttr('checked').removeAttr('disabled').trigger('change');
                }
            } else {
                $("#IsInternalAttendance_AssessmentApplicable").siblings('.jsIsApplicable').removeAttr('checked');
            }
            setApplicableInputs($("#IsInternalAttendance_AssessmentApplicable").siblings('.jsIsApplicable'));
            //end column-IV

            // ---------------- End Block -------------------------//
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}



function getSchoolList() {
    var _college_ID = $("#College_ID option:selected").val();
    var $targetSelect = $("#SchoolFullName");
    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/GetSchools";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { College_ID: _college_ID },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}


function getProfessorList() {
    var $targetSelect = $("#ProfessorDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];
    var college_id = $("#CollegeDDL").find('option:selected').val();
    if (!isNullOrEmpty(college_id))
        _param.Filters.push({ Column: "College_ID", Operator: "EqualTo", Value: college_id, GroupOperation: "AND", TableAlias: "AppUsers" });

    _param.SortInfo = { ColumnName: "FullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "FullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/ProfessorDDL";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}
function getCRList() {
    var $targetSelect = $("#CRDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];
    var college_id = $("#CollegeDDL").find('option:selected').val();
    if (!isNullOrEmpty(college_id))
        _param.Filters.push({ Column: "College_ID", Operator: "EqualTo", Value: college_id, GroupOperation: "AND", TableAlias: "AppUsers" });

    _param.SortInfo = { ColumnName: "FullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "FullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/CRDDL";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}


function getExistingProfessorNCR() {
    var form = $("#AssignProfessorCR");
    var _url = "/CUSrinagarAdminPanel/UserProfile/CheckExistSubject";

    $.ajax({
        type: "POST",
        url: _url,
        data: form.serialize(), // serializes the form's elements.
        success: function (data) {
            if (data.PC_ID !== undefined && data.PC_ID !== null) {
                $(`#ProfessorDDL option[value='${data.Professor.toUpperCase()}']`).attr('selected', 'selected').trigger("chosen:updated");
                //$("#ProfessorDDL").prop('selectedValue', data.Professor).trigger("chosen:updated");
                $(`#CRDDL option[value='${data.CR.toUpperCase()}']`).attr('selected', 'selected').trigger("chosen:updated");
                //$("#CRDDL").val(data.cr).trigger("chosen:updated");
                $("#ProfessorCRClasses_ID").val(data.PC_ID);
            }
        }
    });
}


function showStructureList() {
    $("#StructureList").modal('show');
    var param = new Parameter();
    param.PageInfo.PageNumber = param.PageInfo.PageSize = -1;
    param.PageInfo.DefaultOrderByColumn = "SubjectStructure_ID";
    $.ajax({
        url: getBaseUrlAdmin() + "Subject/StructureListPartial",
        type: 'POST',
        data: param,//jQuery.param({ parameter: param }),
        //contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
        success: function (html) {
            $("#StructureList #StructureListBody").html(html);
        },
        error: function (xhr, error, message) {
            console.log(error + ': ' + message);
        },
        beforeSend: function () { showLoader(); },
        complete: function () { hideLoader(); }
    });


}

function fillSubjectTitles() {

    var $select = $('#SubjectFullNameDataList');
    clearDDOptions($select);
    var _param = new Parameter();
    _param.Filters = [];
    if ($("#Semester").val().length === 0) return;

    _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: $("#Semester").val(), GroupOperation: "AND", TableAlias: "" });

    if ($("#ProgrammeDDL").val().length > 0 && $("#ProgrammeDDL").val() === "2")
        _param.Filters.push({ Column: "Programme", Operator: "EqualTo", Value: "2", GroupOperation: "AND", TableAlias: "C" });
    else if ($("#ProgrammeDDL").val().length > 0)
        _param.Filters.push({ Column: "Programme", Operator: "NotEqualTo", Value: "2", GroupOperation: "AND", TableAlias: "C" });

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };

    var _url = "/CUSrinagarAdminPanel/General/SubjectDDLWithDetail";

    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: _param,
        beforeSend: function () { showLoader(); },
        success: function (data) {
            if (data != null) {
                $.each(data, function (index, item) {
                    $select.append('<option value="' + item.Text + '"></option>');
                });
            }
        },
        error: function (xhr, error, msg) {

        },
        complete: function () {
            hideLoader();

        }
    });
}