jQuery(document).ready(function ($) {
    $(".HideRollNos").hide();
    $("#JSfrmSMS").submit(function () {
        if ($(this).valid()) {
            if (confirm("Are you sure you want to Send?")) {
                $(".JSSendSMS").empty().html('<i class="fa fa-send"></i>&nbsp; Sending...').prop("disabled", true);
                return true;
            }
        }
        return false;
    });


    $("#OtherPhoneNo").focusout(function () {
        var OtherNos = $(this).val();
        $(this).val('').val(OtherNos.replace(/(\r\n|\n|\r|\s)/gm, ""));
    });
    $("#Message").focusout(function () {
        var _Message = $(this).val();
        $(this).val('').val(_Message.replace(/(\r\n|\n|\r)/gm, ""));
    });
    $("#RollNos").focusout(function () {
        var _Message = $(this).val();
        $(this).val('').val(_Message.replace(/(\r\n|\n|\r|\s)/gm, ""));
    });

    //------------------ get courses ----------------------------
    $("body").on("change", "#JSfrmSMS #Programme,#JSfrmSMS #Batch,#JSfrmSMS #SMSRegarding,#JSfrmSMS #College_ID", function () {
        var smsregarding = $("#JSfrmSMS #SMSRegarding").val();
        var programmeElement = $("#JSfrmSMS #Programme");
        var batchElement = $("#JSfrmSMS #Batch");
        var collegeElement = $("#JSfrmSMS #College_ID");

        if ($(this).attr('id') === "SMSRegarding" && smsregarding === "7") {
            $("#OtherPhoneNo").attr("required", "required");
            $("#hideOnOther").slideUp(200);
            $("#RollNos").removeAttr("required");
            $("#RollNos-error").empty();
            $(".HideRollNos").hide(200);
            return;
        } else {
            $("#OtherPhoneNo").removeAttr("required");
            $("#OtherPhoneNo-error").empty();
            $("#hideOnOther").slideDown(200);
        }

        if ($(this).attr('id') === "SMSRegarding" && smsregarding === "5") {
            $("#RollNos").attr("required", "required");
            $(".HideRollNos").show(200);
            return;
        } else if ($(this).attr('id') === "SMSRegarding" && smsregarding !== "5") {
            $("#RollNos").removeAttr("required");
            $("#RollNos-error").empty();
            $(".HideRollNos").hide(200);
        }

        var prog = programmeElement.val();
        var batch = batchElement.val();
        var College = collegeElement.val();
        if (prog !== "" && batch !== "" && smsregarding !== "") {
            if ($(this).attr("id") !== "College_ID") {
                GetCollegeCenterSMSDDL();
            }

            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SMS/_GetCoursesSMSDDL",
                type: "POST",
                data: JSON.stringify({
                    SMSRegarding: smsregarding,
                    Programme: prog,
                    Batch: batch,
                    College_ID: College
                }),
                datatype: "html",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    $("#JSfrmSMS .CourseID").empty().html(response);
                    resetMultiSelect();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    });


    //------------------ get courses for Centers ----------------------------
    function GetCollegeCenterSMSDDL() {
        var smsregarding = $("#JSfrmSMS #SMSRegarding").val();
        var programmeElement = $("#JSfrmSMS #Programme");
        var batchElement = $("#JSfrmSMS #Batch");

        var prog = programmeElement.val();
        var batch = batchElement.val();
        if (prog !== "" && batch !== "" && smsregarding !== "") {
            $.ajax({
                async: true,
                url: "/CUSrinagarAdminPanel/SMS/_GetCollegeCenterSMSDDL",
                type: "POST",
                data: JSON.stringify({
                    SMSRegarding: smsregarding,
                    Programme: prog,
                    Batch: batch
                }),
                datatype: "html",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    $("#JSfrmSMS .CollegeID").empty().html(response);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }
    }
});