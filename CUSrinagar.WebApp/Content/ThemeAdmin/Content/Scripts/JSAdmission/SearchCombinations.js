
$(document).ready(function ($) {
    var _baseURL = null;
    if (!isNullOrEmpty( $("#collegebaseurl").val()) && $("#collegebaseurl").val().length > 0) {
        _baseURL = "CUCollegeAdminPanel";
    } else {
        _baseURL = "CUSrinagarAdminPanel";
    }

    $("#SearchCombinationForm").submit(function (e) {
        if ($(this).valid()) {
            if ($("#btnclicked").val() === "Search") {
                $("#JSSearch").val("Searching...").prop("disabled", true);
            }
            return true;
        } else {
            $("#JSSearch").prop("disabled", false).val("Search");
            return false;
        }
    });

    $("#JSSearch").on("mouseover focus", function (e) {
        $("#btnclicked").val("Search");
    });
    $("#JSExcel").on("mouseover focus", function (e) {
        $("#btnclicked").val("JSExcel");
    });
    $("#College_ID").change(function () {
        var College_ID = $("#College_ID").val();
        if (College_ID !== "") {
            $.ajax({
                url: "/" + _baseURL+"/Combination/GetCourseList",
                type: "POST",
                data: { College_ID: College_ID },
                dataType: "html",
                traditional: true,
                success: function (response) {
                    $("#CourseDiv").empty().html(response);
                    ChosenStyle();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }

    });
});