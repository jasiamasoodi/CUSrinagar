jQuery(function ($) {
    $(document).on('click', '.admitcards .JSDownloadBtn', function (e) {
        e.preventDefault();
        var semis = true;
        var Sem = $(".admitcards #Semester option:selected").val();
        var yearis = true;
        var Year = $(".admitcards #Year").val();
        var Course_ID = $(".admitcards #CourseID").val();
        var College_ID = $(".admitcards #College_ID").val();
        var Programme = $(".admitcards #ProgrammeId").val();
        var isValidated = true;
        if (!Programme) {
            $(".admitcards #ProgrammeIdValidator").html("Required");
        } else { $(".admitcards #ProgrammeIdValidator").html(""); }
        if (!Year) {
            yearis = false;
            $(".admitcards #YearValidator").html("Required");
        }
        else {
            yearis = true;
            $(".admitcards #YearValidator").html("");
        }
        if (!Sem) {
            semis = false;
            $(".admitcards #SemesterValidator").html("Required");
        }
        else {
            semis = true;
            $(".admitcards #SemesterValidator").html("");
        }
        if (!Course_ID) {
            isValidated = false;
            $(".admitcards #CourseIDValidator").html("Required");
        }
        else {
            $(".admitcards #CourseIDValidator").html("");
            isValidated = true;
        }
        if (!College_ID) {
            isValidated = false;
            $(".admitcards #CollegeIDValidator").html("Required");
        }
        else {
            $(".admitcards #CollegeIDValidator").html("");
            isValidated = true;
        }

        if (isValidated && yearis && semis) {

            var url = gridPathWithParam + "/" + Year;
            url += "/" + Course_ID;
            if (Programme && Programme !== "") {
                url += "?Programme=" + Programme;

            }
            url += "&sem=" + Sem + "&College_ID=" + College_ID;
            window.open(url, '_blank');
        }
        return false;
    });


    $(document).on('click', '.attsheets .JSDownloadBtn', function (e) {
        e.preventDefault();
        var semis = true;
        var Sem = $(".attsheets #Semester option:selected").val();
        var yearis = true;
        var Year = $(".attsheets #Year").val();
        var Course_ID = $(".attsheets #CourseID option:selected").val();
        var College_ID = $(".attsheets #College_ID option:selected").val();
        var Programme = $(".attsheets #ProgrammeId option:selected").val();
        var Center_ID = $(".attsheets #Center_ID option:selected").val();
        var isValidated = true;

        if (Programme=="") {
            $(".attsheets #ProgrammeIdValidator").html("Required");
            isValidated = false;
        } else { $(".attsheets #ProgrammeIdValidator").html(""); isValidated = true; }

        if (!College_ID) {
            $(".attsheets #colValidator").html("Required");
            isValidated = false;
        } else { $(".attsheets #colValidator").html(""); isValidated = true; }

        if (!Year) {
            yearis = false;
            $(".attsheets #YearValidator").html("Required");
        }
        else {
            yearis = true;
            $(".attsheets #YearValidator").html("");
        }
        if (!Sem) {
            semis = false;
            $(".attsheets #SemesterValidator").html("Required");
        }
        else {
            semis = true;
            $(".attsheets #SemesterValidator").html("");
        }
        if (Course_ID=="") {
            isValidated = false;
            $(".attsheets #CourseIDValidator").html("Required");
        }
        else {
            $(".attsheets #CourseIDValidator").html("");
            isValidated = true;
        }
        

        if (isValidated && yearis && semis) {

            var url = gridasPathWithParam + "?y=" + Year;
            url += "&course_id=" + Course_ID;
            url += "&sem=" + Sem + "&College_ID=" + College_ID;
            if (Programme !== "") {
                url += "&Programme=" + Programme;

            }
            if (Center_ID !== "") {
                url += "&CC=" + Center_ID;
            }
            window.open(url, '_blank');
        }
        return false;
    });
    $(document).on('change', '.attsheets #College_ID', function (e) {
        $(".attsheets #CourseID").prop('selectedIndex', 0);
        $(".attsheets #ProgrammeId").prop('selectedIndex', 0);

    });

    $(document).on('change', '.attsheets .SearchDDL', function (e) {
        var id = $(this).val();
        var Type = $(this).attr('data-ddltype');
        var cid = $(".attsheets #College_ID").val();
        if (Type == "Combination" || Type == "Semester") { id = $(".attsheets #CourseID").val(); }
        var childType = $(this).attr('data-ddlchildtype');
        var childSubType = $(this).attr('data-ddlsubchildtype');
        var Semester = $(this).val();
        var path = chilDDLPath;
        if (Type != "" && Type != undefined && id != "" && id != undefined && cid != "" && cid != undefined) {
            $('.attsheets #' + Type).html("<span style='color:#62a8d1'>...Please Wait..."); // Modify me
            $.ajax({
                url: path,
                data: { College_ID: cid, id: id, Type: Type, childType: childType, childSubType: childSubType, Semester: Semester },
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                encode: true,
                type: "Get",
                cache: false,
                async: false,
                success: function (response) {
                    var PartialView = response;
                    $('.attsheets #' + Type).html(PartialView); // Modify me

                    ChosenStyle();
                }
            });
        }
    });

    $(document).on('change', '.attsheets #Semester', function (e) {
        var Semester = $(this).val();
        var ProgrammeId = $(".attsheets #ProgrammeId").val();
        var CourseID = $(".attsheets #CourseID").val();
        var year = $(".attsheets #Year").val();
        var cid = $(".attsheets #College_ID").val();

        var path = CenterListPath;
        if (Semester != "" && Semester != undefined && ProgrammeId != ""
            && ProgrammeId != undefined && CourseID != "" && CourseID != undefined && year != "" && year != undefined && cid != "" && cid != undefined) {
            $('.attsheets #Center_IDs').html("<span style='color:#62a8d1'>...Please Wait..."); // Modify me
            $.ajax({
                url: path,
                data: { College_ID: cid, Course_ID: CourseID, printProgramme: ProgrammeId, examYear: year, semester: Semester },
                datatype: "json",
                contentType: "application/json; charset=utf-8",
                encode: true,
                type: "Get",
                cache: false,
                async: false,
                success: function (response) {
                    var PartialView = response;
                    $('.attsheets #Center_IDs').html(PartialView);// Modify me

                    ChosenStyle();
                }
            });
        }
    });

});