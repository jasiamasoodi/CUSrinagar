window.oncontextmenu = function () { return false; }

$(document).ready(function ($) {
    $("input[type='text']").change(function () {
        $(this).val($(this).val().toUpperCase());
    });
    $("#StudentAddress_Mobile").focus();

    $("#EditStdRegistration").submit(function (e) {
        var errorsCount=$("#EditStdRegistration .field-validation-error").length;
        if ($(this).valid() && Number(errorsCount)<=0) {
            if (confirm("Are you sure you want to apply changes?")) {
                $("#Jspleasewait").show();
                return true;
            }
        }
        $("#Jspleasewait").hide();
        return false;
    });

    Show12th();
    $("#IsProvisional").change(function () {
        Show12th();
        //Remove current form validation information
        $("#EditStdRegistration").removeData("validator").removeData("unobtrusiveValidation");
        //Parse the form again
        $.validator.unobtrusive.parse("#EditStdRegistration");
    });
    function Show12th() {
        var vals = $("#IsProvisional").val();
        var idtoHide = "#" + $("#ExamType").val();
        if (vals === "True") {
            $(idtoHide).css("display", "none");
            $("#RemoveQ_ID").val($("#Q_ID").val());
        } else {
            $(idtoHide).css("display", "").removeClass("hidden");
            $("#RemoveQ_ID").val('');
        }
    }

    $(".jsvalminm").change(function () {
        var maxMarks = $(this).closest("tr").find(".jsvalmm").val();
        var minMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).val('');
            alert("Min. Marks Obtained should be less or equal to Max. Marks");
        }
    });

    $(".jsvalmm").change(function () {
        var minMarks = $(this).closest("tr").find(".jsvalminm").val();
        var maxMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).closest("tr").find(".jsvalminm").val('');
            alert("Max. Marks should be greater or equal to Min. Marks");
        }
    });
});