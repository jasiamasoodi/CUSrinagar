window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    $("body").css("background-color", "");
    $("#CaptchaInputText").attr("maxlength", "10");
    $("input[type='text']").attr("autocomplete", "off");
    $("body #CaptchaInputText-error").css('color', 'red');

    $("form").submit(function (e) {
        if (!$('form').valid()) return false;
        showLoader();
        $("[type='submit']").attr("disabled", true);
        return true;
    });

    $("input[type='text']").change(function () {
        $(this).val($(this).val().toUpperCase());
    });


    $(".jsvalminm").change(function () {
        var maxMarks = $(".jsvalmm").val();
        var minMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).val('');
            alert("Min. Marks Obtained should be less or equal to Max. Marks");
        }
    });

    $(".jsvalmm").change(function () {
        var minMarks = $(".jsvalminm").val();
        var maxMarks = $(this).val();
        if (Number(maxMarks) < Number(minMarks)) {
            $(this).find(".jsvalminm").val('');
            alert("Max. Marks should be greater or equal to Min. Marks");
        }
    });
    $(".jscolpef").each(function () {
        if ($.trim($(this).val()) == "") {
            $(".jsnextbt").hide();
            return;
        }
    });

    setTimeout(function () {
        $('.alert').hide();
    }, 7000);

    Show12th();
    $("#IsProvisional").change(function (e) {
        Show12th();
    });
    function Show12th() {
        var vals = $("#IsProvisional").val();
        var trToRemove ="trToRemove";
        if (vals === "True") {
            $("#" + trToRemove).css("display", "none");
        } else {
            $("#" + trToRemove).css("display", "");
        }
    }

});