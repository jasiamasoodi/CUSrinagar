jQuery(document).ready(function ($) {

    $("#Jspleasewait").addClass("hidden");
    $(document).on('submit', '#frmAssignCentersInBulk', function (e) {
        var isOK = JSValidation();
        if ($(this).valid()) {
            if (isOK) {
                if (confirm("Are you sure you want to proceed?")) {
                    $("#Jspleasewait").removeClass("hidden");
                    return true;
                }
            }
        }
        $("#Jspleasewait").addClass("hidden");
        return false;
    });


    function JSValidation() {
        var CheckBoxSelected = $('body').find('[id$="__HasToBeAssigned"]:checked');
        if (CheckBoxSelected.length !== 0) {

            var isAnyEmpty = false;
            $(CheckBoxSelected).each(function () {
                if ($(this).closest("tr").find('[id$="__AssignmentOrder"]').val() === "") {
                    isAnyEmpty = true;
                }
            });
            if (isAnyEmpty) {
                alert("Please provide Assignment Order for each selected Centers.");
                return false;
            }

            isAnyEmpty = false;
            var CustomTotalEntranceFormsAvailable = 0;
            $(CheckBoxSelected).each(function () {
                if ($(this).closest("tr").find('[id$="__NoOfStudentsToBeAssignedPerCenter"]').val() === "") {
                    isAnyEmpty = true;
                } else {
                    CustomTotalEntranceFormsAvailable += Number($(this).closest("tr").find('[id$="__NoOfStudentsToBeAssignedPerCenter"]').val());
                }
            });
            if (isAnyEmpty) {
                alert("Please provide No Of Students To Be Assigned Per Center for each selected Centers.");
                return false;
            }

            var TotalEntranceFormsAvailable = $('body').find("#TotalEntranceFormsAvailable").val();

            if (Number(TotalEntranceFormsAvailable) > 0) {
                if (Number(TotalEntranceFormsAvailable) > Number(CustomTotalEntranceFormsAvailable)) {
                    alert("There are still " + (Number(TotalEntranceFormsAvailable) - Number(CustomTotalEntranceFormsAvailable)) + " Entrance Forms available which needs to be assigned.\nPlease select more centers\nOR\nincrease No. Of Students To Be Assigned Per Center.");
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
                if ($(this).attr('id') !== id && $(this).closest("tr").find('[id$="__AssignmentOrder"]').val() !== "") {
                    return $(this).closest("tr").find('[id$="__AssignmentOrder"]').val();
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

    $('body').on("change", ".HasToBeAssigned", function () {
        if ($(this).is(":Checked")) {
            var AOrder = Number($("input[name='AssignmentOrderIndex']").val()) + 1;
            $(this).closest("tr").find('[id$="__AssignmentOrder"]').val(AOrder);
            $("input[name='AssignmentOrderIndex']").val(AOrder);
        } else {
            $(this).closest("tr").find('[id$="__AssignmentOrder"]').val('');
        }
    });


    $("#PrintProgramme,#Batch").change(function () {
        var ARGExamForm = $(this).val();
        var Id = $("#frmAssignCentersInBulk #PrintProgramme").val();
        var Id1 = $("#frmAssignCentersInBulk #Batch").val();
        if (Id !== "" && Id1 !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesDDL/" + Id + "/" + Id1,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $(".CourseID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });

    $('body').on("change", "#Course_ID", function () {
        var PrintProgramme = $("#frmAssignCentersInBulk #PrintProgramme").val();
        var Batch = $("#frmAssignCentersInBulk #Batch").val();
        var Course_ID = $(this).val();

        if (PrintProgramme !== "" &&
            Batch !== "" &&
            Course_ID !== "") {
            $("#Jspleasewait").removeClass("hidden");
            $("#JSDiplayContent").empty();
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_DispalyAvaliableCenters",
                type: "POST",
                data: {
                    PrintProgramme: PrintProgramme,
                    Batch: Batch,
                    Course_ID: Course_ID
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
                }, complete: function () {
                    resetMultiSelect();
                }
            });
        } else {
            $("#JSDiplayContent").empty();
            $(this).val('');
            alert("Please select all the fields");
        }
    });


    $('body').on("change", "#frmRelocateCenter #RelocateCenters_FromCenter_ID", function () {
        var Id = $(this).val();
        if (Id !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/Entrance/_GetCoursesRCDDL/" + Id,
                type: "POST",
                data: {},
                datatype: "html",
                success: function (response) {
                    $(".FromCourseID").empty().html(response);
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
                url: "/CUSrinagarAdminPanel/Entrance/RelocateCenter/",
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
});