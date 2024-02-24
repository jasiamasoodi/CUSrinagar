﻿window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    var isLateralEntry = $.trim($("#IsLateralEntry").val()).toLowerCase();
    if (isLateralEntry == "true") {
        $('#Preference option[value="' + selectedpreference + '"]').attr('selected', 'selected');
    }

    $("#Jsfee").addClass("quadrat");
    var IsNotJKResident = $("#Distt").val() === "other";
    if (IsNotJKResident) {
        var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + Number($("#AdditionalFeeForNonJK").val()) : "";
        $("#Jsfee").text("Total Fee Rs : " + basicFee + "" + AdditionalFeeForNonJK);
    } else {
        $("#Jsfee").text("Total Fee Rs : " + $("#TotalFee").val());
    }

    $("#StdRegistration").find('[id$="__Preference"]').each(function () {
        if ($(this).val() == '') {
            $(this).prop("disabled", true);
        }
    });

    $(".JSCourseClk").click(function () {
        var graduationFee = 50;
        var DDLId = "." + $(this).attr("dataid");
        if ($(this).is(":Checked")) {
            $(DDLId).prop("disabled", false);
        } else {
            $(DDLId).val('').prop("disabled", true);
        }
        var IsNotJKResident = $("#StudentAddress_District").val() === "OTHER|OTHER";
        var PercourseFee = Number($("#PerCourse").val());
        var basicFee = Number($("#BasicFee").val()) + PercourseFee;

        var checkedCourse = $(this).attr("dataid").toLowerCase();
        var allChecked = $('[id$="__IsClicked"]:checked');
        var count = allChecked.length;


        if (count > 0) {
            if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")) {
                basicFee = (basicFee - PercourseFee) + graduationFee;
            }
            else if (count > 1 || (count === 1 && !$(this).is(":Checked"))) {
                if (checkedCourse.toLowerCase() !== 'a3ee7f98-7b82-4d95-a2c0-faba7a18240e') {
                    basicFee = CalculateFee(PercourseFee, basicFee, checkedCourse, $(this).is(":Checked"));
                } else {
                    basicFee = $("#TotalFee").val();
                }
            }
            if (IsNotJKResident) {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + $("#AdditionalFeeForNonJK").val() : "";
                $("#Jsfee").text("Total Fee Rs : " + basicFee + AdditionalFeeForNonJK);

                if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")) {
                    $("#TotalFee").val(basicFee - graduationFee);
                } else {
                    $("#TotalFee").val(basicFee);
                }
            } else {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                $("#Jsfee").text("Total Fee Rs : " + basicFee);
                if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")) {
                    $("#TotalFee").val(basicFee - graduationFee);
                } else {
                    $("#TotalFee").val(basicFee);
                }
            }

        } else {
            $("#Jsfee").removeClass("quadrat");
            $("#Jsfee").text("");
            $("#TotalFee").val(0);
        }
    });


    function CalculateFee(PercourseFee, basicFee, checkedCourse, isChecked) {

        var arrayE = ['a7cb49e3-6e17-41d5-a696-62d8c5a8af1b',//HE
            '653aa86c-4900-484e-b566-ef49fd4e8313'];//IE
        var arrayC = ['c490c5af-95b6-407a-ae85-668a5fd9e277',//HC
            '58144c9c-319b-4aa1-b335-bf77b0fd1c24'];//IC
        var arrayH = ['9702e8ec-5e1e-4e46-8fcb-26f996c898d0',//HH
            '8368eeef-8f08-4077-aabe-48b348c1f85b'];//IH

        var arrayBCA = ['32996ce4-5613-4a2d-a565-030390d7f496',//mca
            '9b15f7e4-967d-453f-bee8-8b624fe6d0df'];//BCA
        var arrayBBA = ['dc9595e4-b4dc-41e3-aeb9-446c0b990bcf',//mba
            '4e7a4b78-b87c-4666-a517-069656d55281'];//BBA

        basicFee = isChecked ? Number($("#TotalFee").val()) + PercourseFee : Number($("#TotalFee").val()) - PercourseFee;
        if (jQuery.inArray(checkedCourse, arrayE) > -1) {
            var parallelValue = arrayE.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayE[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayC) > -1) {
            var parallelValue = arrayC.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayC[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayH) > -1) {
            var parallelValue = arrayH.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayH[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayBCA) > -1) {
            var parallelValue = arrayBCA.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayBCA[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayBBA) > -1) {
            var parallelValue = arrayBBA.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayBBA[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        }
        return basicFee;
    }

    $('#StdRegistration').submit(function (e) {
        var invalidPrefNumber = false;
        var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;

        if (isLateralEntry == "true") {
            var prefCount = 0;
            $("#StdRegistration").find('[id$="__Preference"]').each(function () {
                if ($.trim($(this).val()) != '') {
                    if (Number($.trim($(this).val())) > ChkCount) {
                        alert("You have selected " + $(this).val() + " as one of your perferences which is invalid as per your selected course(s)");
                        $("#Jspleasewait").hide();
                        invalidPrefNumber = true;
                        return false;
                    }
                    prefCount = prefCount + 1;
                }
            });
        }

        if (invalidPrefNumber) {
            $("#Jspleasewait").hide();
            return false;
        }
        if (Number(ChkCount) <= 0) {
            alert("Please select course(s)");
            $("#Jspleasewait").hide();
            return false;
        }

        else if (isLateralEntry == "true" && Number(prefCount) < Number(ChkCount)) {
            alert("Please select preference(s) for all your selected course(s)");
            $("#Jspleasewait").hide();
            return false;
        }

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

    $('body').on('change mouseover focus', '[id$="__Preference"]', function () {
        if ($(this).val() == "")
            return;
        $('#CourseSection select').find('option').prop('disabled', false);
        $('#CourseSection select').each(function () {
            $('#CourseSection select').not(this).find('option[value="' + this.value + '"]').prop('disabled', true);
        });
    });
});