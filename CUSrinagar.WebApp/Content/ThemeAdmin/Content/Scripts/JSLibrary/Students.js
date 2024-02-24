

$(document).ready(function () {

    var $pagertable = $(".js-pager-table");


});






jQuery(function ($) {

    $(document).on('change', '.SearchDDL', function (e) {
        var id = $(this).val();
        var Type = $(this).attr('data-ddltype');
        if (Type == "Combination" || Type == "Semester") { id = $("#CourseID").val(); }
        var childType = $(this).attr('data-ddlchildtype');
        var childSubType = $(this).attr('data-ddlsubchildtype');
        var Semester = $(this).val();
        var path = chilDDLPath;
        if (Type != "" && Type != undefined && id != "" && id != undefined) {
            $('#' + Type).html("<span style='color:#62a8d1'>...Please Wait..."); // Modify me
            $.ajax({
                url: path,
                data: { id: id, Type: Type, childType: childType, childSubType: childSubType, Semester: Semester },
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

    $(document).on('change', '#Semester', function (e) {
        var Semester = $(this).val();
        var ProgrammeId = $("#ProgrammeId").val();
        var CourseID = $("#CourseID").val();
        var year = $("#Year").val();

        var path = CenterListPath;
        if (Semester != "" && Semester != undefined && ProgrammeId != ""
            && ProgrammeId != undefined && CourseID != "" && CourseID != undefined && year != "" && year != undefined) {
            $('#Center_IDs').html("<span style='color:#62a8d1'>...Please Wait..."); // Modify me
            $.ajax({
                url: path,
                data: { Course_ID: CourseID, printProgramme: ProgrammeId, examYear: year, semester: Semester },
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                encode: true,
                type: "Get",
                cache: false,
                async: false,
                success: function (response) {
                    var PartialView = response;
                    $('#Center_IDs').html(PartialView);// Modify me

                    ChosenStyle();
                }
            });
        }
    });


    $(document).on('click', '.JSDownloadBtn', function (e) {
        e.preventDefault();
        var semis = true;
        var Sem = $("#Semester option:selected").val();
        var Cid = $.trim($("#Center_ID option:selected").val());
        var yearis = true;
        var Year = $("#Year").val();
        var Course_ID = $("#CourseID").val();
        var Combination_ID = $("#CombinationID").val();
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

            if (Combination_ID && Combination_ID != "") {
                url += "/" + Combination_ID;

            }
            if (Programme && Programme != "") {
                url += "?Programme=" + Programme;

            }

            if (Cid != "") {
                Cid = "&c=" + Cid;
            }
            url += "&sem=" + Sem + "&ExamRollNo=" + ExamRollNo + Cid;


            window.open(url, '_blank');
        }

        return false;

    });

});