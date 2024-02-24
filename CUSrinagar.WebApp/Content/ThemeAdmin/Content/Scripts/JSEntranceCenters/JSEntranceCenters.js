jQuery(document).ready(function ($) {
    $("#Jspleasewait").addClass("hidden");
    //-------------------- delete center ------------------------------
    $("#JSDeleteDataFromCenterAllotment").click(function () {
        if (confirm("Are you sure you want to truncate all data from Center Allotment Table?")) {
            $("#Jspleasewait").removeClass("hidden");

            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/DeleteDataFromCenterAllotment",
                type: "POST",
                datatype: "json",
                success: function (response) {
                    if (response === true) {
                        $("#Jspleasewait").addClass("hidden");
                        alert("Delete successfully");
                    } else {
                        $("#Jspleasewait").addClass("hidden");
                        alert(response);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $("#Jspleasewait").addClass("hidden");
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        } else {
            $("#Jspleasewait").addClass("hidden");
        }
    });

    //---------------- Assign RollNos in Bulk --------------------
    $("#frmAssignRollNosInBulk").submit(function (e) {
        if ($(this).valid()) {
            var _Programme = $(" #frmAssignRollNosInBulk #PrintCenterNotice_CourseCategory").val();
            var Batch = $("#frmAssignRollNosInBulk #PrintCenterNotice_ExaminationYear").val();

            if (confirm("Are you sure you want to proceed.\nRemember changes cannot be undo")) {
                $('.JSbtnAssignRollNosInBulk').html('Creating...').prop("disabled", true);
                $('.msg5').empty().addClass("quadrat").html("<b>Process may take several minutes to complete. Please be patient. Working...</b>");
                $.ajax({
                    url: "/CUSrinagarAdminPanel/Entrance/AssignRollNosInBulk",
                    type: "POST",
                    data: { _Programme: _Programme, Batch: Batch },
                    datatype: "json",
                    traditional: true,
                    success: function (response) {
                        if (response === true) {
                            $(" #frmAssignRollNosInBulk #PrintCenterNotice_CourseCategory").val('');
                            $("#frmAssignRollNosInBulk #PrintCenterNotice_ExaminationYear").val('');

                            $('.JSbtnAssignRollNosInBulk').html('<i class="ace-icon fa fa-check"></i> Proceed').prop("disabled", false);
                            $('.msg5').empty().removeClass("quadrat").html("<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> RollNo.'s Assigned Successfully</div>");
                        } else {
                            $(".msg5").empty().removeClass("quadrat").empty().html("<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + response + "</div>");
                            $('.JSbtnAssignRollNosInBulk').html('<i class="ace-icon fa fa-check"></i> Proceed').prop("disabled", false);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        $('.JSbtnAssignRollNosInBulk').html('<i class="ace-icon fa fa-check"></i> Proceed').prop("disabled", false);
                        alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    }
                });
            }
        }
        return false;
    });

    //--------------- search by college ------------------
    $("#SearchCollege_ID").change(function () {
        var College_ID = $(this).val();
        $(".JSContent").empty().html("<div style='text-align:center' class='quadraText'><b>Working on it...</b></div>");
        $.ajax({
            async: true,
            url: "/CUSrinagarAdminPanel/Entrance/_DisplayCenters",
            type: "POST",
            data: { College_ID: College_ID },
            datatype: "html",
            traditional: true,
            success: function (response) {
                $(".JSContent").empty().html(response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $(".JSContent").empty();
                alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
            }
        });
    });

    //--------------------change status --------------------------
    $("body").on("click", ".JsStatus", function () {
        var ClickedCheckBox = $(this);
        var Center_ID = ClickedCheckBox.attr("id");
        var status = ClickedCheckBox.is(':checked');
        if ($("#" + Center_ID).val() === "F") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/ChangeCenterStatus",
                type: "POST",
                data: { Center_ID: Center_ID },
                datatype: "json",
                traditional: true,
                success: function (response) {
                    if (response === true) {
                        //alert("Center status changed successfully");
                        //window.location.href = "/CUSrinagarAdminPanel/SemesterCenters/Centers";
                    } else {
                        ClickedCheckBox.prop("checked", !status);
                        alert("Operation aborted.\nCenter is having students associated with it.");
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    ClickedCheckBox.prop("checked", !status);
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        } else {
            ClickedCheckBox.prop("checked", !status);
            alert("Operation aborted.\nCenter is having students associated with it.");
        }
    });

    //-------------------- delete center ------------------------------
    $("body").on("click", ".JSDelete", function () {
        var ClickedDelete = $(this);
        var Center_ID = ClickedDelete.attr("id");
        if ($("#" + Center_ID).val() === "F") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/DeleteCenter",
                type: "POST",
                data: { Center_ID: Center_ID },
                datatype: "json",
                traditional: true,
                success: function (response) {
                    if (response === true) {
                        ClickedDelete.closest("tr").remove();
                    } else {
                        alert("Operation aborted.\nCenter is having students associated with it.");
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        } else {
            alert("Operation aborted.\nCenter is having students associated with it.");
        }
    });

    //---------------- Download Center Notice --------------------
    $("#frmCreateNoticeContent").submit(function (e) {
        if ($(this).valid()) {
            $('.JSDCNotice').html('Searching...').prop("disabled", true);
            return true;
        }
        return false;
    });

    //------------------ get courses ----------------------------
    $("#frmOMRMasterFile #PrintCenterNotice_CourseCategory,#frmOMRMasterFile #PrintCenterNotice_ExaminationYear").change(function () {

        var Id = $("#frmOMRMasterFile #PrintCenterNotice_CourseCategory").val();
        var Id1 = $("#frmOMRMasterFile #PrintCenterNotice_ExaminationYear").val();
        if (Id !== "" && Id1 !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesOMRDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmOMRMasterFile .CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });

    $("#frmAttendanceSheetFile #PrintCenterNotice_CourseCategory,#frmAttendanceSheetFile #PrintCenterNotice_ExaminationYear").change(function () {

        var Id = $("#frmAttendanceSheetFile #PrintCenterNotice_CourseCategory").val();
        var Id1 = $("#frmAttendanceSheetFile #PrintCenterNotice_ExaminationYear").val();
        if (Id !== "" && Id1 !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesOMRDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmAttendanceSheetFile .CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });

    $("#frmCategoryMeritList #PrintCenterNotice_CourseCategory,#frmCategoryMeritList #PrintCenterNotice_ExaminationYear").change(function () {

        var Id = $("#frmCategoryMeritList #PrintCenterNotice_CourseCategory").val();
        var Id1 = $("#frmCategoryMeritList #PrintCenterNotice_ExaminationYear").val();
        if (Id !== "" && Id1 !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesOMRDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmCategoryMeritList .CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });

    $("#frmGeneralMeritList #PrintCenterNotice_CourseCategory,#frmGeneralMeritList #PrintCenterNotice_ExaminationYear").change(function () {

        var Id = $("#frmGeneralMeritList #PrintCenterNotice_CourseCategory").val();
        var Id1 = $("#frmGeneralMeritList #PrintCenterNotice_ExaminationYear").val();
        if (Id !== "" && Id1 !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesOMRDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmGeneralMeritList .CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });

    $("#frmDistrictWiseCenterAllotmentCount #PrintCenterNotice_CourseCategory,#frmDistrictWiseCenterAllotmentCount #PrintCenterNotice_ExaminationYear").change(function () {

        var Id = $("#frmDistrictWiseCenterAllotmentCount #PrintCenterNotice_CourseCategory").val();
        var Id1 = $("#frmDistrictWiseCenterAllotmentCount #PrintCenterNotice_ExaminationYear").val();
        if (Id !== "" && Id1 !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesOMRDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmDistrictWiseCenterAllotmentCount .CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });



    //-------------------- Create centers ------------------------
    $("#frmCreateCenters").submit(function (e) {
        if ($(this).valid()) {
            $('.JSCreate').html('Creating...').prop("disabled", true);
            var College_ID = $("#frmCreateCenters #College_ID").val();
            var PipeSeparatedCenters = $("#frmCreateCenters #PipeSeparatedCenters").val();
            var IsEntrance = $("#frmCreateCenters #IsEntrance").val();
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/Create",
                type: "POST",
                data: { College_ID: College_ID, PipeSeparatedCenters: PipeSeparatedCenters, IsEntrance: IsEntrance },
                datatype: "json",
                traditional: true,
                success: function (response) {
                    if (response === true) {
                        $("#frmCreateCenters #College_ID").val('');
                        $("#frmCreateCenters #PipeSeparatedCenters").val('');
                        $('.JSCreate').html('<i class="ace-icon fa fa-check"></i> Create').prop("disabled", false);
                        $('.msg0').empty().html("<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> Centers created successfully</div>");
                    } else {
                        $(".msg0").empty().html("<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> Cannot save centers either codes already exists or an Unknown error occured</div>");
                        $('.JSCreate').html('<i class="ace-icon fa fa-check"></i> Create').prop("disabled", false);
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $('.JSCreate').html('<i class="ace-icon fa fa-check"></i> Create').prop("disabled", false);
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
        return false;
    });


    //--------------------- upload entrance result --------------------
    $("#frmUploadEntranceResult #PrintCenterNotice_CourseCategory,#frmUploadEntranceResult #PrintCenterNotice_ExaminationYear").change(function () {

        var Id = $("#frmUploadEntranceResult #PrintCenterNotice_CourseCategory").val();
        var Id1 = $("#frmUploadEntranceResult #PrintCenterNotice_ExaminationYear").val();
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
            if (confirm("Are you sure you want to  upload Entrance Result?")) {
                $('.JSUploadEntranceResultBtn').html('Working...').prop("disabled", true);
                return true;
            }
        }
        return false;
    });

    //---------------- frm SelfFinanced List --------------------
    $("#frmSelfFinancedList").submit(function (e) {
        if ($(this).valid()) {
            return true;
        }
        return false;
    });

    //------------------- PDF merit lists ------------------
    $("#frmPDFMeritList #PDFListFilter_PrintProgramme,#frmPDFMeritList #PDFListFilter_Batch").change(function () {
        var Id = $("#frmPDFMeritList #PDFListFilter_PrintProgramme").val();
        var Id1 = $("#frmPDFMeritList #PDFListFilter_Batch").val();
        if (Id !== "" && Id1 !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesPDFDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmPDFMeritList .Course_ID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });

    var today = new Date();
    var date = (today.getDate()+1) + '-' + (today.getMonth() + 1) + '-' + today.getFullYear();
    $("#frmPDFMeritList #PDFListFilter_MSGOnBottomOfPDF").val("Mail your objections, grievances at entrance" + today.getFullYear() + "@cusrinagar.edu.in upto " + date +" 4:00 PM. No objection(s) shall be entertained afterwards.");
});
