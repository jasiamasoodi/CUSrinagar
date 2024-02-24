window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    var hasPaymentDone = $("#HasPayment").val() === "True";
    var IsNotJKResident = $("#Distt").val() === "other";
    $("#Jsfee").addClass("quadrat");

    $("#frmRegnCourses").find('[name$=".Preference"]').each(function () {
        if ($(this).val() === '')
            $(this).prop("disabled", true);
    });


    if (hasPaymentDone) {
        $("#Jsfee").text("Total Fee Paid Rs : " + Number($("#TotalFee").val()));
    } else {
        $("#Jsfee").text("Total Fee Rs : " + Number($("#TotalFee").val()));
    }

    $(".JSCourseClk").click(function () {

        var DDLId = "." + $(this).attr("dataid");
        if ($(this).is(":Checked")) {
            $(DDLId).prop("disabled", false);
        } else {
            $(DDLId).val('').prop("disabled", true);
        }
        var IsNotJKResident = $("#AdditionalFeeForNonJK").val() === "OTHER|OTHER";
        var PercourseFee = Number($("#PerCourse").val());
        var basicFee = Number($("#BasicFee").val()) + PercourseFee;

        var checkedCourse = $(this).attr("dataid").toLowerCase();
        var allChecked = $('[name$="].IsClicked"]:checked');

        var groupArray = [];
        var countc = 0;
        if (hasPaymentDone) {
            //fee submitted
            allChecked.each(function () {
                var checkedCourseGroupName = $(this).attr("data-groupname").trim();
                if (jQuery.inArray(checkedCourseGroupName, groupArray) == -1) {
                    if ($("[data-PrefGroup='" + checkedCourseGroupName + "']").val() !== "True") {
                        groupArray.push($(this).attr("data-groupname"));
                    }
                }
            });
            countc = groupArray.length;

        } else {
            //fee not submitted
            allChecked.each(function () {
                var checkedCourse = $(this).attr("data-groupname").trim();
                if (jQuery.inArray(checkedCourse, groupArray) == -1) {
                    groupArray.push($(this).attr("data-groupname"));
                }
            });
            countc = groupArray.length;
        }

        if (countc > 0) {
            if (!hasPaymentDone) {
                basicFee = basicFee - PercourseFee;
                //fee not submitted
                basicFee = (basicFee + (PercourseFee * countc));
                $("#TotalFee").val(basicFee);

                if (IsNotJKResident) {
                    $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                    var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + $("#AdditionalFeeForNonJK").val() : "";
                    $("#Jsfee").text("Total Fee Rs : " + basicFee + AdditionalFeeForNonJK);
                } else {
                    $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                    $("#Jsfee").text("Total Fee Rs : " + basicFee);
                    $("#TotalFee").val(basicFee);
                }
            } else {

                //fee Submitted
                basicFee = ((PercourseFee * countc));
                $("#TotalFee").val(basicFee);

                if (IsNotJKResident) {
                    $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                    var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + $("#AdditionalFeeForNonJK").val() : "";
                    $("#Jsfee").text("Total Fee Rs : " + basicFee + AdditionalFeeForNonJK);
                } else {
                    $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                    $("#Jsfee").text("Total Fee Rs : " + basicFee);
                    $("#TotalFee").val(basicFee);
                }
            }
        } else {
            $("#Jsfee").removeClass("quadrat");
            $("#Jsfee").text("");
            $("#TotalFee").val(0);
        }
    });






    $('body').on('change', '[name$=".Preference"]', function () {
        if ($(this).val() == "")
            return;
        var clickedPref = $(this);
        var classRelation = $(this).attr("data-relationclass");
        $('#CourseSection [data-relationclass="' + classRelation + '"]').each(function (index, element) {
            if ($(element).attr("name") != clickedPref.attr("name")) {
                if ($(element).val() == clickedPref.val()) {
                    $(element).val("");
                }
            }
        });
    });






    $("#frmRegnCourses").submit(function (e) {

        var submitOrNot = true;
        var prefCount = $('[name$=".Preference"]:not([disabled])').each(function () {
            if ($(this).val() === "") {
                submitOrNot = false;
                return false;
            }
        });

        if (!submitOrNot) {
            alert("Please select Preference for all Courses.");
            return false;
        }

        var CourseCount = $(".JSCourseClk:Checked").length;
        if (hasPaymentDone && CourseCount === 0) {
            CourseCount = 1;
        }

        if (CourseCount > 0) {
            if (confirm("Are you sure you want to proceed.")) {
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