jQuery(document).ready(function ($) {
    $("#Jspleasewait").addClass("hidden");

    $("#SearchCollege_ID").change(function () {
        var College_ID = $(this).val();
        $(".JSContent").empty().html("<div style='text-align:center' class='quadraText'><b>Working on it...</b></div>");
        $.ajax({
            async: true,
            url: "/CUSrinagarAdminPanel/SemesterCenters/_DisplayCenters",
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
        if ($("#" + Center_ID).val() == "F") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/ChangeCenterStatus",
                type: "POST",
                data: { Center_ID: Center_ID },
                datatype: "json",
                traditional: true,
                success: function (response) {
                    if (response == true) {
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
        if ($("#" + Center_ID).val() == "F") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/DeleteCenter",
                type: "POST",
                data: { Center_ID: Center_ID },
                datatype: "json",
                traditional: true,
                success: function (response) {
                    if (response == true) {
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


    //-------------------- Create centers ------------------------
    $("#frmCreateCenters").submit(function (e) {
        if ($(this).valid()) {
            $('.JSCreate').html('Creating...').prop("disabled", true);
            var College_ID = $("#frmCreateCenters #College_ID").val();
            var IsEntrance = $("#frmCreateCenters #IsEntrance").val();
            var PipeSeparatedCenters = $("#frmCreateCenters #PipeSeparatedCenters").val();
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/Create",
                type: "POST",
                data: { College_ID: College_ID, PipeSeparatedCenters: PipeSeparatedCenters, IsEntrance: IsEntrance },
                datatype: "json",
                traditional: true,
                success: function (response) {
                    if (response == true) {
                        $("#frmCreateCenters #College_ID").val('');
                        $("#frmCreateCenters #PipeSeparatedCenters").val('');
                        $('.JSCreate').html('<i class="ace-icon fa fa-check"></i> Create').prop("disabled", false);
                        $('.msg').empty().html("<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> Centers created successfully</div>");
                    } else {
                        $(".msg").empty().html("<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button> Cannot save centers either codes already exists or an Unknown error occured</div>");
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


    //---------------- Download Center Notice --------------------
    $("#frmCreateNoticeContent").submit(function (e) {
        if ($(this).valid()) {
            $('#frmCreateNoticeContent .JSDCNotice').html('Searching...').prop("disabled", true);
            return true;
        }
        return false;
    });
    $("#frmArchiveCreateNoticeContent").submit(function (e) {
        if ($(this).valid()) {
            $('#frmArchiveCreateNoticeContent .JSDCNotice').html('Searching...').prop("disabled", true);
            return true;
        }
        return false;
    });

    //---------------- Assign RollNos in Bulk --------------------
    $("#frmAssignRollNosInBulk").submit(function (e) {
        if ($(this).valid()) {
            var elementDDL = $("#frmAssignRollNosInBulk #ExamSetting_ID");
            var ExamSetting_ID = elementDDL.val();
            if ($.trim(ExamSetting_ID) === "") {
                $(".error").empty().text(" Required");
                return false;
            }
            $(".error").empty();
            if (confirm("Are you sure you want to proceed.\nRemember changes cannot be undo")) {
                $('.JSbtnAssignRollNosInBulk').html('Creating...').prop("disabled", true);
                $('.msg5').empty().addClass("quadrat").html("<b>Process may take several minutes to complete. Please be patient. Working...</b>");
                $.ajax({
                    url: "/CUSrinagarAdminPanel/SemesterCenters/AssignRollNosInBulk",
                    type: "POST",
                    data: { ExamSetting_ID: ExamSetting_ID },
                    datatype: "json",
                    traditional: true,
                    success: function (response) {
                        if (response.result === true) {
                            elementDDL.val('');
                            $('.JSbtnAssignRollNosInBulk').html('<i class="ace-icon fa fa-check"></i> Proceed').prop("disabled", false);
                            $('.msg5').empty().removeClass("quadrat").html("<div class='alert alert-success alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + response.msg + "</div>");
                        } else {
                            $(".msg5").empty().removeClass("quadrat").empty().html("<div class='alert alert-danger alert-dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>×</button>" + response.msg + "</div>");
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


    //----------------- JSDeleteDataFromCenterAllotment ----------
    $("#JSDeleteDataFromCenterAllotment").click(function () {
        if (confirm("Are you sure you want to Archive all data from Center Allotment Table?")) {
            $("#Jspleasewait").removeClass("hidden");

            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SemesterCenters/DeleteDataFromCenterAllotment",
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
});