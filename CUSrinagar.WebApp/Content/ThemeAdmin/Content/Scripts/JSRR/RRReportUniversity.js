jQuery(function ($) {

    $(document).on('change', '.SearchDDL', function (e) {
        var id = $(this).val();
        var Type = $(this).attr('data-ddltype');
        if (Type == "Combination" || Type == "Semester")
        { id = $("#CourseID").val(); }
        var college_ID = $("#College_ID").val();
        var childType = $(this).attr('data-ddlchildtype');
        var childSubType = $(this).attr('data-ddlsubchildtype');
        var path = chilDDLPath;
        if (Type != "" && Type != undefined && id != "" && id != undefined) {
            $('#' + Type).html("<span style='color:#62a8d1'>...Please Wait..."); // Modify me
            $.ajax({
                url: path,
                data: { College_ID: college_ID, id: id, Type: Type, childType: childType, childSubType: childSubType },
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                encode: true,
                type: "Get",
                cache: false,
                async: false,
                success: function (response) {
                    var PartialView = response;
                    $('#' + Type).html(PartialView); // Modify me
                    ChosenStyle();
                }
            });
        }
    });

    $(document).on('click', '.JSDownloadBtn', function (e) {
        e.preventDefault();
        var semis = true;
        var Sem = $("#Semester option:selected").val();
        var yearis = true;
        var Year = $("#Year").val();
        var Course_ID = $("#CourseID").val();
        var College_ID = $("#College_ID").val();
        var Programme = $("#ProgrammeId").val();
        var ExamRollNo = $("#ExamRollNo").val();
        var isValidated = true;
        if (!Programme) {
            $("#ProgrammeIdValidator").html("Required");
        } else { $("#ProgrammeIdValidator").html(""); }
        if (!Year) {
            yearis = false;
            $("#YearValidator").html("Required");
        }
        else {
            yearis = true;
            $("#YearValidator").html("");
        }
        if (!Sem) {
            semis = false;
            $("#SemesterValidator").html("Required");
        }
        else {
            semis = true;
            $("#SemesterValidator").html("");
        }
        if (!Course_ID) {
            isValidated = false;
            $("#CourseIDValidator").html("Required");
        }
        else {
            $("#CourseIDValidator").html("");
            isValidated = true;
        }

        if (isValidated && yearis && semis) {

            var url = gridPathWithParam + "/" + Year;
            url += "/" + Course_ID;

            if (Programme && Programme != "") {
                url += "?Programme=" + Programme;
            }
            url += "&sem=" + Sem + "&ExamRollNo=" + ExamRollNo + "&College_ID=" + College_ID;
            window.open(url, '_blank');
        }

        return false;

    });


    $(document).on('click', '.JSDownloadInExcel', function (e) {
        e.preventDefault();
        var semis = true;
        var Sem = $("#Semester option:selected").val();
        var yearis = true;
        var Year = $("#Year").val();
        var Course_ID = $("#CourseID").val();
        var College_ID = $("#College_ID").val();
        var Programme = $("#ProgrammeId").val();
        var ExamRollNo = $("#ExamRollNo").val();
        var isValidated = true;
        if (!Programme) {
            $("#ProgrammeIdValidator").html("Required");
        } else { $("#ProgrammeIdValidator").html(""); }
        if (!Year) {
            yearis = false;
            $("#YearValidator").html("Required");
        }
        else {
            yearis = true;
            $("#YearValidator").html("");
        }
        if (!Sem) {
            semis = false;
            $("#SemesterValidator").html("Required");
        }
        else {
            semis = true;
            $("#SemesterValidator").html("");
        }
        if (!Course_ID) {
            isValidated = false;
            $("#CourseIDValidator").html("Required");
        }
        else {
            $("#CourseIDValidator").html("");
            isValidated = true;
        }

        if (isValidated && yearis && semis) {
            var url = "/CUSrinagarAdminPanel/RR/DownloadInExcel/" + Year;
            url += "/" + Course_ID;
            if (Programme && Programme != "") {
                url += "?Programme=" + Programme;
            }
            url += "&sem=" + Sem + "&ExamRollNo=" + ExamRollNo + "&College_ID=" + College_ID;
            window.open(url, '_blank');
        }

        return false;

    });


});