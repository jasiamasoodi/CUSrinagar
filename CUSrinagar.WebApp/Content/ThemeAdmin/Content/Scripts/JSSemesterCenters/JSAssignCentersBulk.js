jQuery(document).ready(function ($) {

    $("#Jspleasewait").addClass("hidden");

    function JSValidation() {
        var CheckBoxSelected = $('body').find('[id$="__HasToBeAssigned"]:checked');
        if (CheckBoxSelected.length != 0) {

            var isAnyEmpty = false;
            $(CheckBoxSelected).each(function () {
                if ($(this).closest("tr").find('input[type="Number"]').val() == "") {
                    isAnyEmpty = true;
                }
            });
            if (isAnyEmpty) {
                alert("Please provide Assignment Order for each selected Centers.");
                return false;
            }

            var TotalExamFormsAvailable = $('body').find("#TotalExamFormsAvailable").val();

            if (Number(TotalExamFormsAvailable) > 0) {
                var NoOfStudentsToBeAssignedPerCenter = $("#NoOfStudentsToBeAssignedPerCenter").val();
                var TotalExamFormsToAssign = CheckBoxSelected.length * Number(NoOfStudentsToBeAssignedPerCenter);
                if (Number(TotalExamFormsAvailable) > Number(TotalExamFormsToAssign)) {
                    alert("There are still " + (Number(TotalExamFormsAvailable) - Number(TotalExamFormsToAssign)) + " Exam forms available which needs to be assigned.\nPlease select more centers\nOR\nincrease No. Of Students To Be Assigned Per Center.");
                    return false;
                }
            }
            return true;
        }
        else {
            alert("Please Choose the centers to be alloted using checkboxes.");
            return false;
        }
    }



    $('body').on("focusout", ".AssignCentersAs", function () {
        var id = $(this).closest("tr").find("[id$='__HasToBeAssigned']").attr('id');

        var CheckBoxSelected = $('body').find("[id$='__HasToBeAssigned']:checked");
        if (CheckBoxSelected.length > 1) {

            var checkedValues = $(CheckBoxSelected).map(function () {
                if ($(this).attr('id') !== id && $(this).closest("tr").find('input[type="Number"]').val() !== "") {
                    return $(this).closest("tr").find('input[type="Number"]').val();
                }
            }).get();

            if (checkedValues.length > 0) {
                if ($.inArray($(this).val(), checkedValues) !== -1) {
                    $(this).closest("tr").find('.errMsg').empty().text("Duplicate");
                    $(this).val('');
                } else {
                    $(this).closest("tr").find('.errMsg').empty();
                }
            }
        }
    });


    $("#StudentesOfCollege_ID").change(function () {
        $("#ARGExamForm_ID").val('');
        $("#Course_ID").val('');
    });


    $('body').on("change", ".HasToBeAssigned", function () {
        if ($(this).is(":Checked")) {
            var AOrder = Number($("input[name='AssignmentOrderIndex']").val()) + 1;
            $(this).closest("tr").find('input[type="Number"]').val(AOrder);
            $("input[name='AssignmentOrderIndex']").val(AOrder);
        } else {
            $(this).closest("tr").find('input[type="Number"]').val('');
        }
    });


    $("#ARGExamForm_ID").change(function () {
        var ARGExamForm = $(this).val();
        var StudentesOfCollege_ID = $("#StudentesOfCollege_ID").val();
        if (ARGExamForm != "" && StudentesOfCollege_ID != "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/_GetCoursesDDLForBulkAssign/" + ARGExamForm + "/" + StudentesOfCollege_ID,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $(".CourseID").empty().html(response);
                    ChosenStyle();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        } else {
            $(this).val('');
            alert("Please select College / Exam Forms");
        }
    });


    $("#NoOfStudentsToBeAssignedPerCenter").val('800');


    $('body').on("change", "#CenterLocationCollege_ID,#Course_ID", function () {
        var StudentesOfCollege_ID = $("#StudentesOfCollege_ID").val();
        var ARGExamForm_ID = $("#ARGExamForm_ID").val();
        var Course_ID = $("body").find("#Course_ID").val();
        var CenterLocationCollege_ID = $("#CenterLocationCollege_ID").val();
        var NoOfStudentsToBeAssignedPerCenter = $("#NoOfStudentsToBeAssignedPerCenter").val();
        var _SemesterCenterCategory = $("#SemesterCenterCategory").val();
        if ($.trim(CenterLocationCollege_ID) == "") {
        } else {

            if (StudentesOfCollege_ID != "" &&
                ARGExamForm_ID != "" &&
                Course_ID != "" &&
                CenterLocationCollege_ID != "" &&
                NoOfStudentsToBeAssignedPerCenter != "" && _SemesterCenterCategory != "") {
                $("#Jspleasewait").removeClass("hidden");
                $("#JSDiplayContent").empty();
                $.ajax({
                    async: true,
                    url: "/CUSrinagarAdminPanel/SemesterCenters/_DispalyAvaliableCenters",
                    type: "POST",
                    data: {
                        StudentesOfCollege_ID: StudentesOfCollege_ID,
                        ARGExamForm_ID: ARGExamForm_ID,
                        Course_ID: Course_ID,
                        CenterLocationCollege_ID: CenterLocationCollege_ID,
                        NoOfStudentsToBeAssignedPerCenter: NoOfStudentsToBeAssignedPerCenter,
                        SemesterCenterCategory: _SemesterCenterCategory
                    },
                    datatype: "html",
                    success: function (response) {
                        $("#JSDiplayContent").html(response);
                        var CheckBoxSelected = $('body').find("[id$='__HasToBeAssigned']");
                        $(CheckBoxSelected).each(function () {
                            $("<span class='lbl'></span>").insertAfter(this);
                        });
                        $("#Jspleasewait").addClass("hidden");
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        $("#Jspleasewait").addClass("hidden");
                        alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    }
                });
            } else {
                $("#JSDiplayContent").empty();
                $(this).val('');
                alert("Please select all the fields");
            }
        }
    });

    $('body').on("change", "#frmRelocateCenter #RelocateCenters_FromCenter_ID", function () {
        var Id = $(this).val();
        if (Id !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/_GetCoursesRCDDL/" + Id,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $("#frmRelocateCenter .FromCourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });


    $('body').on("click", "#frmRelocateCenter .JSBtnRelocateCenter", function () {
        var FromCenter = $("#frmRelocateCenter #RelocateCenters_FromCenter_ID").val();
        var FromCourse = $("#frmRelocateCenter #RelocateCenters_FromCourse_ID").val();
        var ToCenter = $("#frmRelocateCenter #RelocateCenters_ToCenter_ID").val();
        if (FromCenter !== "" && FromCourse !== "" && ToCenter !== "") {
            $(this).prop("disabled", true).empty().html('<i class="fa fa- save"></i>&nbsp;Working...');
            var relocateCenter = { FromCenter_ID: FromCenter, FromCourse_ID: FromCourse, ToCenter_ID: ToCenter };
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/RelocateCenter/",
                type: "POST",
                data: relocateCenter,
                datatype: "html",
                success: function (response) {
                    if (response.Issucces === true) {
                        $('.responseMsg').empty().removeClass("quadrat").html("<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> " + response.msg + "</div>");
                    } else {
                        $('.responseMsg').empty().removeClass("quadrat").html("<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> " + response.msg + "</div>");
                    }
                },
                complete: function () {
                    $("#frmRelocateCenter .JSBtnRelocateCenter").prop("disabled", false).empty().html('<i class="fa fa- save"></i>&nbsp;Proceed');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        } else {
            alert("All fields are required.");
        }
    });


    //----------------------------- archive centers-----------------------------------------

    $('body').on("focusout", "#frmArchiveCenters #ArchiveCenters_Semester", function () {
        $.ajax({
            async: true,
            url: "/CUSrinagarAdminPanel/SemesterCenters/_GetCoursesACDDL/",
            type: "POST",
            data: {},
            datatype: "html",
            success: function (response) {
                $("#frmArchiveCenters .FromCourseID").empty().html(response);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
            }
        });
    });


    $('body').on("click", "#frmArchiveCenters .JSBtnArchiveCenter", function () {
        var FromCourse = $("#frmArchiveCenters #ArchiveCenters_Course_ID").val();
        var semester = $("#frmArchiveCenters #ArchiveCenters_Semester").val();
        if (FromCourse !== "" && semester !== "") {
            $(this).prop("disabled", true).empty().html('<i class="fa fa- save"></i>&nbsp;Working...');
            var relocateCenter = { Course_ID: FromCourse, Semester: semester };
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/ArchiveCenters/",
                type: "POST",
                data: relocateCenter,
                datatype: "html",
                success: function (response) {
                    if (response.Issucces === true) {
                        $('.responseMsg2').empty().removeClass("quadrat").html("<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> " + response.msg + "</div>");
                    } else {
                        $('.responseMsg2').empty().removeClass("quadrat").html("<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> " + response.msg + "</div>");
                    }
                },
                complete: function () {
                    $("#frmArchiveCenters .JSBtnArchiveCenter").prop("disabled", false).empty().html('<i class="fa fa- save"></i>&nbsp;Archive Now');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        } else {
            alert("All fields are required.");
        }
    });
});