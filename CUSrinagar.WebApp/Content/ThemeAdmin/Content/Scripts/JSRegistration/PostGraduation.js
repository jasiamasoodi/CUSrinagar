window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    var IsNotJKResident = $("#Distt").val() === "other";

    $("body").css("background-color", "lightgray");
    $("#CaptchaInputText").attr("maxlength", "10");
    $("input[type='text']").attr("autocomplete", "off");
    $("#StdRegistration").find('[id$="__Preference"]').each(function () {
        if ($(this).val() == '')
            $(this).prop("disabled", true);
    });

    var totalFee = Number($("#TotalFee").val());
    if (totalFee > 0) {
        $("#Jsfee").addClass("quadrat");
        if (IsNotJKResident) {
            var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + Number($("#AdditionalFeeForNonJK").val()) : "";
            $("#Jsfee").text("Total Fee Rs : " + totalFee + AdditionalFeeForNonJK);
        } else {
            $("#Jsfee").text("Total Fee Rs : " + totalFee);
        }
    }

    $("#StdRegistration").submit(function (e) {

        var invalidPrefNumber = false;
        var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;
        //var prefCount = 0;
        //$("#StdRegistration").find('[id$="__Preference"]').each(function () {
        //    if ($.trim($(this).val()) != '') {
        //        if (Number($.trim($(this).val())) > ChkCount) {
        //            alert("You have selected " + $(this).val() + " as one of your perferences which is invalid as per your selected course(s)");
        //            $("#Jspleasewait").hide();
        //            invalidPrefNumber = true;
        //            return false;
        //        }
        //        prefCount = prefCount + 1;
        //    }
        //});
        if (invalidPrefNumber) {
            $("#Jspleasewait").hide();
            return false;
        }
        if (Number(ChkCount) <= 0) {
            alert("Please select course(s)");
            $("#Jspleasewait").hide();
            return false;
        }
        //else if (Number(prefCount) < Number(ChkCount)) {
        //    alert("Please select preference(s) for all your selected course(s)");
        //    $("#Jspleasewait").hide();
        //    return false;
        //}
        var ans = confirm("Please review your details again before final submission.\nNo change is allowed after final submission of the Form.");
        if (ans) {
            if ($("#jsfinalSubmit").val() == "Final Submit") {
                $("#jsfinalSubmit").val("Submitting...").prop("disabled", true);
            }
            $("#Jspleasewait").fadeIn();
            return true;
        } else {
            $("#jsfinalSubmit").prop("disabled", false).val("Final Submit");
            $("#Jspleasewait").hide();
            return false;
        }

        return false;

    });

    $(".JSCourseClk").click(function () {
        var DDLId = "." + $(this).attr("dataid");
        if ($(this).is(":Checked")) {
            $(DDLId).prop("disabled", false);
        } else {
            $(DDLId).val('').prop("disabled", true);
        }

        var PercourseFee = Number($("#PerCourse").val());
        var basicFee = Number($("#BasicFee").val()) + PercourseFee;

        var count = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;
        if (count > 0) {
            if (count > 1) {
                basicFee = basicFee + (PercourseFee * (count - 1));
            }
            if (IsNotJKResident) {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                $("#Jsfee").text("Total Fee Rs : " + basicFee + "+" + $("#AdditionalFeeForNonJK").val());
            } else {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                $("#Jsfee").text("Total Fee Rs : " + basicFee);
            }
            $("#TotalFee").val(basicFee);
        } else {
            $("#Jsfee").removeClass("quadrat");
            $("#Jsfee").text("");
            $("#TotalFee").val("0");
        }
    });

    $('body').on('change mouseover focus', '[id$="__Preference"]', function () {
        if ($(this).val() == "")
            return;
        $('#CourseSection select').find('option').prop('disabled', false);
        $('#CourseSection select').each(function () {
            $('#CourseSection select').not(this).find('option[value="' + this.value + '"]').prop('disabled', true);
        });

    });
});