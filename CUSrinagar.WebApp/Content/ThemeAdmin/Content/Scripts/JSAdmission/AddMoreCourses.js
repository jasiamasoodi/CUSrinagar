window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    var hasPaymentDone = $("#HasPayment").val() === "True";
    var IsNotJKResident = $("#Distt").val() === "other";
    $("#Jsfee").addClass("quadrat");
    if (hasPaymentDone) {
        if (IsNotJKResident) {
            $("#Jsfee").text("Total Fee Paid Rs : " + (Number($("#TotalFee").val()) + Number($("#AdditionalFeeForNonJK").val())));
        } else {
            $("#Jsfee").text("Total Fee Paid Rs : " + Number($("#TotalFee").val()));
        }
    } else {
        if (IsNotJKResident) {
            var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + Number($("#AdditionalFeeForNonJK").val()) : "";
            $("#Jsfee").text("Total Fee Rs : " + basicFee + "" + AdditionalFeeForNonJK);
        } else {
            $("#Jsfee").text("Total Fee Rs : " + $("#TotalFee").val());
        }
    }

    $(".JSCourseClk").click(function () {

        var allChecked = $("#frmRegnCourses").find('[name$="].IsClicked"]:checked');
        var count = allChecked.length;

        if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")
            && $(this).attr("dataid").toLowerCase() === "a3ee7f98-7b82-4d95-a2c0-faba7a18240e") {
            if (hasPaymentDone) {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                $("#Jsfee").text("Total Fee Rs : 0");
                return;
            } else {
                $("#TotalFee").val(Number($("#BasicFee").val()));
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                if (IsNotJKResident) {
                    $("#Jsfee").text("Total Fee Rs : " + Number(Number($("#BasicFee").val()) + 50 + Number($("#AdditionalFeeForNonJK").val())));
                } else {
                    $("#Jsfee").text("Total Fee Rs : " + Number(Number($("#BasicFee").val()) + 50));
                }
            }
        } else {
            var PercourseFee = Number($("#PerCourse").val());
            var checkedCourse = $(this).attr("dataid").toLowerCase();

            if ($(this).attr("dataid").toLowerCase() === "a3ee7f98-7b82-4d95-a2c0-faba7a18240e") {
                return;
            }
            if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")) {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                $("#TotalFee").val(Number($("#BasicFee").val()));

                if (IsNotJKResident) {
                    $("#Jsfee").text("Total Fee Rs : " + Number(Number($("#BasicFee").val()) + 50 + Number($("#AdditionalFeeForNonJK").val())));
                } else {
                    $("#Jsfee").text("Total Fee Rs : " + Number(Number($("#BasicFee").val()) + 50));
                }
                return;
            }

            if (count > 0) {
                if (count > 1 || (count === 1 && !$(this).is(":Checked") && !hasPaymentDone)) {
                    basicFee = CalculateFee(PercourseFee, basicFee, checkedCourse, $(this).is(":Checked"), hasPaymentDone);
                } else {
                    if (count > 1) {
                        basicFee = CalculateFeeForLater(PercourseFee, checkedCourse, $(this).is(":Checked"));
                    } else {
                        if (hasPaymentDone) {
                            basicFee = PercourseFee;
                        } else {
                            basicFee = Number($("#BasicFee").val()) + PercourseFee;
                        }
                    }
                }

                if (!hasPaymentDone && IsNotJKResident) {
                    $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                    var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + Number($("#AdditionalFeeForNonJK").val()) : "";
                    $("#Jsfee").text("Total Fee Rs : " + basicFee + "" + AdditionalFeeForNonJK);
                } else {
                    $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                    $("#Jsfee").text("Total Fee Rs : " + basicFee);
                }
                $("#TotalFee").val(basicFee);
            } else {
                $("#Jsfee").removeClass("quadrat");
                $("#Jsfee").text("");
                $("#TotalFee").val(0);
            }
        }
    });


    function CalculateFee(PercourseFee, basicFee, checkedCourse, isChecked, hasPaymentDone) {

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
        var totalFee = $("#TotalFee").val();
        if (!hasPaymentDone) {
            if (Number(totalFee) === Number($("#BasicFee").val()) + 50) {
                totalFee = Number(totalFee) - 50;
            }
        }

        basicFee = isChecked ? Number(totalFee) + PercourseFee : Number(totalFee) - PercourseFee;
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



    function CalculateFeeForLater(PercourseFee, checkedCourse, isChecked) {

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

        basicFee = isChecked ? PercourseFee : basicFee;
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



    $("#frmRegnCourses").submit(function (e) {
        if ($(".JSCourseClk:Checked").length > 0) {
            if (confirm("Are you sure you want to apply.")) {
                if ($(this).valid()) {
                    $(".JSprogress").empty().html('<b class="btn btn-success">Working...</b>');
                    return true;
                }
            }
        } else {
            alert("Please select the Courses.");
        }
        return false;
    });

    $("#jsRCReprint").click(function () {
        $(".JSprogress").empty().html('<b class="btn btn-success">Working...</b>');
    });
});