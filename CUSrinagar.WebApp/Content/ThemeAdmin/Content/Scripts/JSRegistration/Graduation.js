window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    $(".JSCourseClk").click(function () {
        CalculateUGFee();
    });

    $("#StudentAddress_District").change(function () {
        CalculateUGFee();
    });

    function CalculateUGFee() {
        if (Number($("#FeeForAlreadyInIH").val()) > 0)
            return;

        var PercourseFee = Number($("#PerCourse").val());
        var basicFee = Number($("#BasicFee").val()) + PercourseFee;

        var IsNotJKResident = $("#StudentAddress_District").val() === "OTHER|OTHER";

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
    }


    $(document).on('submit', '#StdRegistration', function (e) {
        var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;
        if (Number(ChkCount) <= 0) {
            alert("Please select course(s)");
            $("#Jspleasewait").hide();
            return false;
        }

        var ans = confirm("Are you sure ,you want to save details.");
        if (ans) {
            if ($("#jsfinalSubmit").val() == "Submit") {
                $("#jsfinalSubmit").val("Submitting...").prop("disabled", true);
            }
            $("#Jspleasewait").fadeIn();
            return true;
        } else {
            $("#jsfinalSubmit").prop("disabled", false).val("Submit");
            $("#Jspleasewait").hide();
            return false;
        }
    });
});

