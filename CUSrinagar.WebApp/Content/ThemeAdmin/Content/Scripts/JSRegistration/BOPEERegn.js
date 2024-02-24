window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    var _IsBoardRegnNoPresentInBatch = false;
    $("body").css("background-color", "lightgray");
    $("#CaptchaInputText").attr("maxlength", "10");
    $("input[type='text']").attr("autocomplete", "off");
    $(".JSlblResult").text($("#trToRemove").val() + " Result");

    $(document).on('submit', '#BoprrRegn', function (e) {

        var ChkCount = $("#BoprrRegn").find('[id$="__IsClicked"]:checked').length;
        var BoardRegnNo = $("#StdBoardRegistrationNo").val();

        if ($(this).valid()) {

            if ($.trim(BoardRegnNo) === "") {
                $("#errorMsg").empty().text(" Required");
                return false;
            } else {
                $("#errorMsg").empty();
            }

            if (Number(ChkCount) <= 0) {
                alert("Please select course(s)");
                $("#Jspleasewait").hide();
                return false;
            }
            BoardRegnNoPresentInBatch();
            if (!_IsBoardRegnNoPresentInBatch) {
                var ans = confirm("Please review your details again before final submission.\nNo change is allowed after final submission of the Form\nUnless you accept the form in your college control panel.");
                if (ans) {
                    if ($("#jsfinalSubmit").val() === "Final Submit") {
                        $("#jsfinalSubmit").val("Submitting...").prop("disabled", true);
                    }
                    $("#Jspleasewait").fadeIn();
                    return true;
                } else {
                    $("#jsfinalSubmit").prop("disabled", false).val("Final Submit");
                    $("#Jspleasewait").hide();
                    return false;
                }
            } else {
                alert(`${BoardRegnNo} already exists in Batch ${$("#BoprrRegn #Batch").val()}`);
                return false;
            }
        } else {
            $("#jsfinalSubmit").prop("disabled", false).val("Final Submit");
            $("#Jspleasewait").hide();
            return false;
        }
    });

    function BoardRegnNoPresentInBatch() {
        var BoardRegnNo = $("#BoprrRegn #StdBoardRegistrationNo").val();
        var Batch = $("#BoprrRegn #Batch").val();
        var Programme = $("#BoprrRegn #programme").val();
        $.ajax({
            url: "/CUCollegeAdminPanel/BOPEERegistration/IsBoardRegnNoPresentInBatch",
            type: "POST",
            data: { StdBoardRegistrationNo: BoardRegnNo, programme: Programme, Batch: Batch },
            dataType: "json", async: false,
            success: function (response) {
                _IsBoardRegnNoPresentInBatch =  response;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert('status code: ' + jqXHR.status + '\n We are unable to connect to server ,\nplease check your internet connection or refresh and try again.');
                _IsBoardRegnNoPresentInBatch =  false;
            }
        });
    }



    Show12th();
    $("#IsProvisional").change(function (e) {
        Show12th();
    });
    function Show12th() {
        var vals = $("#IsProvisional").val();
        var trToRemove = $("#trToRemove").val();
        if (vals === "True") {
            $("#" + trToRemove).css("display", "none");
        } else {
            $("#" + trToRemove).css("display", "");
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

    $("input[type='text']").change(function () {
        $(this).val($(this).val().toUpperCase());
    });

});