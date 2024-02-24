$(document).ready(function () {

    if ($(".collegeRole").is(':checked')) {
        // checked
        $(".CollegeDDL").removeClass("hidden");
    }
    if ($(".fileRole").is(':checked')) {
        // checked
        $(".DepartmentDDL").removeClass("hidden");
    }
    try {
        for (i = 0; i < tracka; i++) {
            var value = $("#Column_" + i).val();
            if (value == "StudentCode") {
                $("#divExamRollNo_" + i).addClass("hidden");
                $("#divStudentCode_" + i).removeClass("hidden");
            }
            else {
                $("#divStudentCode_" + i).addClass("hidden");
                $("#divExamRollNo_" + i).removeClass("hidden");

            }
        }
    }
    catch (e) { }
});
$(document).on('change', '.collegeRole', function (e) {

    if ($(this).is(':checked')) {
        // checked
        $(".CollegeDDL").removeClass("hidden");
    } else {
        // unchecked
        $(".CollegeDDL").addClass("hidden");
        $("#College_ID").val('');
    }
});
$(document).on('change', '.fileRole', function (e) {

    if ($(this).is(':checked')) {
        // checked
        $(".DepartmentDDL").removeClass("hidden");
    } else {
        // unchecked
        $(".DepartmentDDL").addClass("hidden");
        $("#Department_ID").val('');
    }
});
$(document).on('change', '.ParentDDL', function (e) {
    var typeis = $(this).data("typeis");
    var track = $(this).data("id");
    var id = $("#" + typeis + "Subjects_" + track + "__Course_ID option:selected").val();
    var Semester = $("#" + typeis + "Subjects_" + track + "__Semester option:selected").val();
    var Type = $(this).data("ddltype");
    if (Semester == "")
        Semester = 0;
    FillChild(id, Type, track, Semester);
    //var Type = "Subject";
    //FillChild(id, Type, track);
});


function FillChild(id, Type, track, Semester) {
    var childType = $(this).attr('data-ddlchildtype');
    var childSubType = $(this).attr('data-ddlsubchildtype');
    path = childpath;
    if (Type != "" && Type != undefined) {
        $.ajax({
            url: path,
            data: { id: id, Type: Type, track: track, Semester: Semester },
            datatype: "json",
            contentType: "application/json; charset=utf-8",
            encode: true,
            type: "Get",
            cache: false,
            async: false,
            success: function (response) {
                var PartialView = response;
                $('#' + Type + "_" + track).html(PartialView); // Modify me
                ChosenStyle();
                //BindParentClick();
            },
            error: function (response) {
            }
        });
    }
}

$(document).on('click', '#addNew', function (e) {
    var btn = $(this);
    var track = $(btn).data("id");
    $.ajax({
        url: subjectpath,
        data: { track: track },
        datatype: "json",
        contentType: "application/json; charset=utf-8",
        encode: true,
        type: "Get",
        cache: false,
        async: false,
        success: function (response) {
            var PartialView = response;
            $("#Div_" + track).after(PartialView); // Modify me
            track = track + 1;
            $(btn).data("id", track);
            var value = $("#Column_" + track).val();
            if (value == "StudentCode") {
                $("#divExamRollNo_" + track).addClass("hidden");
                $("#divStudentCode_" + track).removeClass("hidden");
            }
            else {
                $("#divStudentCode_" + track).addClass("hidden");
                $("#divExamRollNo_" + track).removeClass("hidden");

            }
            BindEvents();
            var conta = $("#Div_" + track);
            resetMultiSelect(conta);
            try { ChosenStyle(); }
            catch (e) { }
        },
        error: function (response) {
        }
    });
});
$(document).on('click', '#DeleteNew', function (e) {
    var btn = $("#addNew");
    var track = $(btn).data("id");
    var divn = "#Div_" + (track - 1) + " .jsDeleteItem";
    if ($(divn).length > 0) { $(this).remove(); }// Modify me
    if (track > 0) {
        $("#Div_" + track).html("").remove(); // Modify me
        $(btn).data("id", track - 1);

    }
    if (track == 1) { $(this).remove(); }

});

$(document).on('change', '.SemesterChange', function (e) {
    var track = $(this).data("track");
    var type = "";
    var Column = "";
    try {
        type = $(this).data("typeis");
        Column = $("#Column_" + track).val();
    }
    catch (e) { }
    type = (type == undefined || type == "") ? "Professor" : type;
    Column = (Column == undefined || Column == "") ? "" : Column;
    SetMinMaxRollNo(track, type, Column);

});
function SetMinMaxRollNo(track, type, Column) {
    type = (type == undefined || type == "") ? "Professor" : type;
    Column = (Column == undefined || Column == "") ? "" : Column;
    var ColumnType = (Column == "StudentCode") ? Column : "RollNo";
    var ColumnIs = (Column == undefined || Column == "") ? "RollNo" : Column;
    var ischecked = $("#RangeCB_" + track).prop("checked");
    if (ischecked == false) {
        var baseurl = (type == "Professor") ? (getBaseUrlCollege() + "UserProfile/GetMinMaxRollNo") : (getBaseUrlAdmin() + "UserProfile/GetMinMaxRollNoEvalvator");
        var Columns = (type == "Professor") ? "UG" : Column;
        var Semester = $("#" + type + "Subjects_" + track + "__Semester").val();
        if (Semester) {
            var CourseId = $("#" + type + "Subjects_" + track + "__Course_ID option:selected").val();
            var subjectId = $("#" + type + "Subjects_" + track + "__Subject_ID option:selected").val();
            var _url = baseurl;
            if (subjectId != "" && subjectId != null) {
                $.ajax({
                    url: _url,
                    data: { CourseId: CourseId, SubjectId: subjectId, Semester: Semester, Column: Columns },
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    encode: true,
                    type: "GET",
                    cache: false,
                    //async: false,
                    success: function (response) {
                        if (response != null) {
                            var minval = 0;
                            var maxval = 0;
                            if (ColumnType == "StudentCode") {
                                minval = response.MinStudentCode;
                                maxval = response.MaxStudentCode;

                            }
                            else {
                                minval = response.MinRollNo;
                                maxval = response.MaxRollNo;
                            }
                            $("#" + type + "Subjects_" + track + "__Min" + ColumnType).val(minval);
                            $("#" + type + "Subjects_" + track + "__Max" + ColumnType).val(maxval);
                            $("#MinMax" + Column + "_" + track).html("First " + ColumnType + ": " + minval + "<br/>Last " + ColumnType + ": " + maxval + "<br/>No of Students: " + response.NoofStudents); // Modify me
                            $("#" + type + "Subjects_" + track + "__" + ColumnIs + "From").attr({
                                "max": parseInt(maxval),        // substitute your own
                                "min": parseInt(minval),         // values (or variables) here
                                "maxlength": maxval + "".length
                            });
                            $("#" + type + "Subjects_" + track + "__" + ColumnIs + "To").attr({
                                "max": parseInt(maxval),        // substitute your own
                                "min": parseInt(minval),          // values (or variables) here
                                "maxlength": maxval + "".length
                            });
                        }
                        else {
                            $("#MinMax" + Column + "_" + track).html("Some error occured."); // Modify me
                        }
                    },
                    error: function (xhr, error, msg) {
                        alert(msg + " Please refresh and try again.")
                    }
                });
            }
        }
    }
}



$(document).on('change', ".jsvalRange", function () {
    var RollNoFrom = $(this).closest(".jssubjectcontainer").find(".jsvalrollnofrom").val();
    var RollNoTo = $(this).val();
    if (parseFloat(RollNoTo) < parseFloat(RollNoFrom)) {
        $(this).val('');
        alert("The Value is incorrect");
    }
});
function BindEvents() {

    $(".roolf").focusout(function () {
        var type = "Professor";
        try { type = $(this).data("typeis"); }
        catch (e) { }
        checkRange(this, true, type);
    });

    $(".roolt").focusout(function () {
        var type = "Professor";
        try { type = $(this).data("typeis"); }
        catch (e) { }
        checkRange(this, false, type);
    });
}

BindEvents();
function checkRange(obj, isFrom, type) {
    //if ($(obj).val()) {
    //    var min = $(obj).prop("min");
    //    var max = $(obj).prop("max");
    //    // Check correct, else revert back to old value.
    //    if (parseInt($(obj).val()) <= parseInt(max) && parseInt($(obj).val()) >= parseInt(min)) {

    //        if (checkDuplicateExists(obj, isFrom, type)) {
    //            $(obj).val("");
    //        }
    //        else;

    //    }
    //    else {
    //        $(obj).val("");
    //    }
    //  }
}
function checkDuplicateExists(obj, isFrom, type) {

    type = (type == undefined || type == "") ? "Professor" : type;
    var track = $(obj).data("id");
    var Column = $("#Column_" + track).val();
    Column = (Column == undefined || Column == "") ? "RollNo" : Column;
    var totaltrack = $("#addNew").data("id");
    var Semester = $("#" + type + "Subjects_" + track + "__Semester").val();
    var CourseId = $("#" + type + "Subjects_" + track + "__Course_ID option:selected").val();
    var subjectId = $("#" + type + "Subjects_" + track + "__Subject_ID option:selected").val();
    var RollNoFrom = $("#" + type + "Subjects_" + track + "__" + Column + "From").val();
    var RollNoTo = $("#" + type + "Subjects_" + track + "__" + Column + "To").val();
    for (i = 0; i <= totaltrack; i++) {
        if (track != i) {
            var Semesterc = $("#" + type + "Subjects_" + i + "__Semester").val();
            var CourseIdc = $("#" + type + "Subjects_" + i + "__Course_ID option:selected").val();
            var subjectIdc = $("#" + type + "Subjects_" + i + "__Subject_ID option:selected").val();
            var RollNoFromc = $("#" + type + "Subjects_" + i + "__" + Column + "From").val();
            var RollNoToc = $("#" + type + "Subjects_" + i + "__" + Column + "To").val();
            var issame = Semester == Semesterc && CourseId == CourseIdc && subjectId == subjectIdc;

            if (issame) {
                if (!isFrom && RollNoFrom == RollNoFromc && RollNoTo == RollNoToc) {
                    alert("Combination already exists");
                    return true;
                }
                else if (RollNoFrom) {
                    if (RollNoFromc && track < i && isFrom && RollNoFrom >= RollNoFromc) {
                        alert("Should be lesser than" + RollNoFromc);
                        return true;
                    }
                    else if (RollNoFromc && track > i && isFrom && RollNoFrom <= RollNoFromc) {
                        alert("Roll No. already assigned Should be greater than" + RollNoFromc);
                        return true;
                    }
                    else if (RollNoToc && track > i && isFrom && RollNoFrom <= RollNoToc) {
                        alert("Roll No. already assigned Should be greater than" + RollNoToc);
                        return true;
                    }
                }
                else if (RollNoTo) {
                    if (RollNoToc && track < i && !isFrom && RollNoTo >= RollNoToc) {
                        alert("Should be lesser than" + RollNoToc);
                        return true;
                    }
                    else if (RollNoToc && track > i && !isFrom && RollNoTo <= RollNoToc) {
                        alert("Should be greater than" + RollNoTo);
                        return true;
                    }
                }

            }
        }
    }
    return false;

}


$(document).on('change', '.Column', function (e) {
    var track = $(this).data("id");
    var value = $(this).val();
    if (value == "StudentCode") {
        $("#divExamRollNo_" + track).addClass("hidden");
        $("#divStudentCode_" + track).removeClass("hidden");
    }
    else {
        $("#divStudentCode_" + track).addClass("hidden");
        $("#divExamRollNo_" + track).removeClass("hidden");

    }
});
function ShowHideDivEval(Rangeallchk, track, type) {
    var txtRollNo = document.getElementById(type + "_" + track);
    //  var txtRollNoTo = document.getElementById("RollNoTo");
    txtRollNo.style.display = Rangeallchk.checked ? "none" : "block";//hides the textbox roll no to
    //alert("hello");
    var divMinMax = document.getElementById("MinMax" + type + "_" + track);
    divMinMax.style.display = Rangeallchk.checked ? "none" : "block";//hides the Range of Min and Max roll No.s
    if (Rangeallchk.checked) {
        var txtRollNoFrom = document.getElementById("EvalvatorSubjects_" + track + "__" + type + "From");
        var txtRollNoTo = document.getElementById("EvalvatorSubjects_" + track + "__" + type + "To");
        // MinMax_@track
        txtRollNoFrom.value = "";
        txtRollNoTo.value = "";
    }
    else { SetMinMaxRollNo(track, "Evalvator", type); }
    // txtRollNoTo.style.display = Rangeallchk.checked ? "none" : "block";
}
function ShowHideDiv(Rangeallchk, track) {
    var txtRollNo = document.getElementById("Rollno_" + track);
    //  var txtRollNoTo = document.getElementById("RollNoTo");
    txtRollNo.style.display = Rangeallchk.checked ? "none" : "block";//hides the textbox roll no to
    //alert("hello");
    var divMinMax = document.getElementById("MinMax_" + track);
    divMinMax.style.display = Rangeallchk.checked ? "none" : "block";//hides the Range of Min and Max roll No.s
    if (Rangeallchk.checked) {
        var txtRollNoFrom = document.getElementById("ProfessorSubjects_" + track + "__RollNoFrom");
        var txtRollNoTo = document.getElementById("ProfessorSubjects_" + track + "__RollNoTo");
        // MinMax_@track
        txtRollNoFrom.value = "";
        txtRollNoTo.value = "";
    }
    else { SetMinMaxRollNo(track); }
    // txtRollNoTo.style.display = Rangeallchk.checked ? "none" : "block";
}
$(document).on('change', ".SearchDDL", function () {
    var type = "Professor";
    try { type = $(this).data("typeis"); }
    catch (ex) { }
    type = (type == undefined || type == "") ? "Professor" : type;
    var name = $(this).attr("id");
    var track = $(this).data("id");
    if (track == undefined) { track = $(this).data("track"); }
    var pprefix = type + "Subjects_" + track + "__";
    var ddlName = name.replace(pprefix, "");
    switch (ddlName) {
        case 'Course_ID':
            {
                clearDDL(pprefix + "Subject_ID");
                clearDDL(pprefix + "Semester");
                break;
            }

        case 'Semester':
            {
                break;
            }
    }
    var Column = $("#Column_" + track).val();
    parentDiv = (Column == undefined || Column == "") ? "" : ("#div" + Column + "_" + track);
    var ischecked = $(parentDiv + "  #RangeCB_" + track).prop("checked");
    if (!ischecked) { $(parentDiv + "  #RangeCB_" + track).trigger("click"); }
    $("#EvalvatorSubjects_" + track + "__ExamRollNoFrom").val("");
    $("#EvalvatorSubjects_" + track + "__StudentCodeFrom").val("");
    $("#EvalvatorSubjects_" + track + "__ExamRollNoTo").val("");
    $("#EvalvatorSubjects_" + track + "__StudentCodeTo").val("");

});

function clearDDL(type) {
    $("#" + type).val('').trigger("chosen:updated");
}
$(".jsDeleteItem").click(function () {
    // var $tr = $(this).closest('tr');
    var _url = $(this).data('url');
    var msg = '<h4>Are you sure you want to delete this row ?</h4>';
    //msg += '<h5>Once deleted this student, corresponding student has to reassign all its subject combination details</h5>';
    showConfirmationDialog(msg);
    $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
        var $btn = $(this);
        hideConfirmationDialog();
        if ($btn.data('response') == 'yes') {
            // var baseSubject_ID = $tr.find("[name*=BaseSubject_ID]").val();
            //var _ID = $tr.find(".js_ID").val();
            //var _url = getBaseUrlCollege() + `UserProfile/Delete?id=${_ID}&baseSubject_ID=` + baseSubject_ID;
            window.location = _url;
        }
    });
});
$("#DeleteAll").click(function () {
    var UserID = $("#DeleteAll").data("userid");
    var msg = '<h4>Are you sure you want to delete all selected rows ?</h4>';
    //msg += '<h5>Once deleted this student, corresponding student has to reassign all its subject combination details</h5>';
    showConfirmationDialog(msg);
    $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
        var $btn = $(this);
        hideConfirmationDialog();
        var _ID = [];
        if ($btn.data('response') == 'yes') {
            $(".jsdeleteall").each(function () {
                if (this.checked) { _ID.push($(this).data("evalid")) }
            });

            var _url = getBaseUrlAdmin() + 'UserProfile/DeleteaLL?User_Id=' + UserID + '&id=' + JSON.stringify(_ID);

            window.location = _url;
        }
    });
});



