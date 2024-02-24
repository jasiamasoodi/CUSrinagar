$(document).ready(function () {

    resetMultiSelect();

    $("#Course_ID").change(function () {
        $("#jsSubjectList").html("");
        $("#CompulsarySubjects").val("");
        getSubjectDDL();
    });

    $("#Semester").change(function () {
        $("#jsSubjectList").html("");
        $("#CompulsarySubjects").val("");
        getSubjectDDL();
    });

    $("#CombinationSettingForm").submit(function (event) {
        var subjects = (isNullOrEmpty($("#SubjectDDL").val()) || $("#SubjectDDL").val().indexOf("") == 0) ? "" : $("#SubjectDDL").val().toString().replace(/,/g, "|");
        $("#CompulsarySubjects").val(subjects);
    });


});

function getSubjectDDL() {
    var $targetSelect = $("#SubjectDDL");
    clearDDOptions($targetSelect);
    fillDDLDefaultOption($targetSelect);

    var _param = new Parameter();
    _param.Filters = [];
    _param.Filters.push({ Column: "Status", Operator: "EqualTo", Value: 1, GroupOperation: "AND", TableAlias: "S" });
    var course_id = $("#Course_ID").find('option:selected').val();
    if (!isNullOrEmpty(course_id))
        _param.Filters.push({ Column: "Course_ID", Operator: "EqualTo", Value: course_id, GroupOperation: "AND", TableAlias: "S" });

    var semester = $("#Semester").find('option:selected').val();
    if (!isNullOrEmpty(semester))
        _param.Filters.push({ Column: "Semester", Operator: "EqualTo", Value: semester, GroupOperation: "AND", TableAlias: "S" });

    _param.SortInfo = { ColumnName: "SubjectFullName" };
    _param.PageInfo = { PageNumber: -1, PageSize: -1, DefaultOrderByColumn: "SubjectFullName" };
    _param.SortInfo = { ColumnName: "", OrderBy: 1 };

    clearDDOptions($targetSelect);
    fillSubjectDDLAction($targetSelect, _param, null);
}
