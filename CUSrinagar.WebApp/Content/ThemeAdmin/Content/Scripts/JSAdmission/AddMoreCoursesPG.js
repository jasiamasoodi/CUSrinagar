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
        var PercourseFee = Number($("#PerCourse").val());
        var basicFeeDB = Number($("#BasicFee").val());

        var checkedCourse = $(this).attr("dataid").toLowerCase();
        var allChecked = $("#frmRegnCourses").find('[name$="].IsClicked"]:checked');
        var count = allChecked.length;
        if (count > 0) {
            if ((count > 1 && !hasPaymentDone) || (count === 1 && !$(this).is(":Checked") && !hasPaymentDone)) {
                basicFee = basicFeeDB + (PercourseFee * count);
            } else {
                if (count > 1) {
                    basicFee = (PercourseFee * count);
                } else {
                    basicFee = PercourseFee;
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
    });


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