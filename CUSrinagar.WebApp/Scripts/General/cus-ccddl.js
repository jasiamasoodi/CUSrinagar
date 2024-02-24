
$(document).ready(function () {

    $(".jsPrintProgrammeDDL").change(function () {
        getCourseDDLByPrintProgramme();
        var $targetSelect = $(".jsSubjectDDL");
        clearDDOptions($targetSelect);
    });

    $(".jsProgrammeDDL").change(function () {
        getCourseDDLByProgramme();
        var $targetSelect = $(".jsSubjectDDL");
        clearDDOptions($targetSelect);
    });

    $(document).on('change', ".jsCourseDDL", function () {
        getSubjectDDL();
    });

    $(document).on('change', ".jsSemesterDDL", function () {
        getSubjectDDL();
    });

});

//Cascading Dropdown List start
function getCourseDDLByPrintProgramme() {
    var $element = $(".jsPrintProgrammeDDL");
    $('.js-pager-table').data('otherparam1', $element.find('option:selected').val());
    $('.js-exportToCSV').data('otherparam1', 'printProgramme=' + $element.find('option:selected').val());
    var $targetSelect = $(".jsCourseDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);
    var college_id = $("#CollegeDDL").val();
    var programme = $element.find('option:selected').val();
    if (isNullOrEmpty(college_id) || college_id == emptyGuid() || isNullOrEmpty(programme)) {
        return;
    }
    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/CollegeProgramCourses";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { College_ID: college_id, printProgramme: programme },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);


            //fillDDLDefaultOption($targetSelect);
            //if (data != null) {
            //    $.each(data, function (index, item) {
            //        $targetSelect.append('<option value="' + item.Value + '">' + item.Text.trim() + '</option>');
            //    });
            //}
            //resizechosen($targetSelect);
            $(".jsCourseDDL").trigger('change');
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}

function getCourseDDLByProgramme() {
    var $element = $(".jsProgrammeDDL");
    $('.js-pager-table').data('otherparam1', $element.find('option:selected').val());
    $('.js-exportToCSV').data('otherparam1', 'printProgramme=' + $element.find('option:selected').val());
    var $targetSelect = $(".jsCourseDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);
    var college_id = $("#CollegeDDL").val();
    var programme = $element.find('option:selected').val();

    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/CoursesByProgramme";
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        data: { College_ID: college_id, programme: programme },
        success: function (data) {
            fillDDLOptions($targetSelect, data);
            ChosenStyle();
            resizechosen($targetSelect);


            //fillDDLDefaultOption($targetSelect);
            //if (data != null) {
            //    $.each(data, function (index, item) {
            //        $targetSelect.append('<option value="' + item.Value + '">' + item.Text.trim() + '</option>');
            //    });
            //}
            //resizechosen($targetSelect);
            $(".jsCourseDDL").trigger('change');
        },
        error: function (xhr, error, msg) {
            fillDDLOptions($targetSelect, null);
            showErrorMessage(msg);
        }
    });
}

function getSubjectDDL() {
    var $targetSelect = $(".jsSubjectDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];
    var course_id = $(".jsCourseDDL").find('option:selected').val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });

    var semester = $(".jsSemesterDDL").find('option:selected').val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "ADMSubjectMaster" });

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    clearDDOptions($targetSelect);
    var _url = "/CUSrinagarAdminPanel/General/SubjectDDL";
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


//cascading Dropdown list end



