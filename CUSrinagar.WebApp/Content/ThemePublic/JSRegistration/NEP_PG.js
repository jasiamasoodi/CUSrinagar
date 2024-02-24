﻿window.oncontextmenu = function () {
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

    $(".moreprefselected").change(function () {
        var classOfPScore = $(this).attr("data-percentile-score-class");
        var ddlValue = $(this).val();
        if ($.trim(ddlValue) != "") {
            $("." + classOfPScore).prop('required', true);
        } else {
            $("." + classOfPScore).prop('required', false);
            $("." + classOfPScore).val('');
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
        var trToRemove = "trToRemove";
        if (vals === "True") {
            $("#" + trToRemove).css("display", "none");
        } else {
            $("#" + trToRemove).css("display", "");
        }
    }
    $(".bsf").keyup(function () {
        setBoardForSelfFinanceQuota();
    });
    $(".bsf").focusout(function () {
        setBoardForSelfFinanceQuota();
    });
    function setBoardForSelfFinanceQuota() {
        if (isBSF === "True") {
            var BoardRegnSF = $(".bsf").val();
            BoardRegnSF = BoardRegnSF.replace(/SF-/g, "");
            BoardRegnSF = BoardRegnSF.replace(/sf-/g, "");
            BoardRegnSF = BoardRegnSF.replace(/-sf/g, "");
            BoardRegnSF = BoardRegnSF.replace(/-SF/g, "");
            if (BoardRegnSF !== "" && BoardRegnSF !== undefined) {
                $(".bsf").val("SF-" + $.trim(BoardRegnSF));
            } else {
                $(".bsf").val('');
            }
        }
    }
});