﻿window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    $("body").css("background-color", "lightgray");
    $("#CaptchaInputText").attr("maxlength", "10");
    $("input[type='text']").attr("autocomplete", "off");

    //status or job form
    $("#StdRegistration").submit(function (e) {
        if ($("#prevf").attr("value") == "#jspreview") {
            if ($(this).valid()) {

                var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;
                if (Number(ChkCount) <= 0) {
                    alert("Please select course(s)");
                    $("#Jspleasewait").hide();
                    return false;
                }
                $(this).off("submit").submit();
                return;
            }
        }
    });
    $(document).on('submit', '#StdRegistration', function (e) {
        if ($("#prevf").attr("value") != "#jspreview") {

            var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;
            if ($(this).valid()) {

                if (Number(ChkCount) <= 0) {
                    alert("Please select course(s)");
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
            } else {
                $("#jsfinalSubmit").prop("disabled", false).val("Final Submit");
                $("#Jspleasewait").hide();
                return false;
            }
        }
    });

    $("#jspreview").focus(function () {
        $("#StdRegistration").attr("action", "/Registration/GraduationPreview");
        $("#StdRegistration").attr("target", "_blank");
        $("#prevf").attr("value", "#jspreview");
    });

    $("#jspreview").mouseover(function () {
        $("#StdRegistration").attr("action", "/Registration/GraduationPreview");
        $("#StdRegistration").attr("target", "_blank");
        $("#prevf").attr("value", "#jspreview");
    });

    $("#jsfinalSubmit").focus(function () {
        $("#StdRegistration").attr("action", "/Registration/Graduation");
        $("#StdRegistration").removeAttr("target");
        $("#prevf").attr("value", "");
    });
    $("#jsfinalSubmit").mouseover(function () {
        $("#StdRegistration").attr("action", "/Registration/Graduation");
        $("#StdRegistration").removeAttr("target");
        $("#prevf").attr("value", "");
    });




    Show12th();
    $("#IsProvisional").change(function (e) {
        Show12th();
        //Remove current form validation information
        $("#StdRegistration").removeData("validator").removeData("unobtrusiveValidation");
        //Parse the form again
        $.validator.unobtrusive.parse("#StdRegistration");
    });
    function Show12th() {
        var vals = $("#IsProvisional").val();
        if (vals == "True") {
            $("#12TH").css("display", "none");
        } else {
            $("#12TH").css("display", "");
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

    $("input[type='text']").change(function () {
        $(this).val($(this).val().toUpperCase());
    });


    $("#BoardRegistrationNo").change(function () {
        var BoardRegistrationNo = $("#BoardRegistrationNo").val();
        var programme = $("input[name='programme']").val();
        if (BoardRegistrationNo != "") {
            $("#Jspleasewait").show(150);
            $.ajax({
                url: "/Registration/BoardData",
                type: "POST",
                data: { BoardRegistrationNo: BoardRegistrationNo, programme: programme },
                dataType: "json",
                traditional: true,
                success: function (response) {
                    removeBoardData();
                    if (response.BoardRegistrationNo != null && response.BoardRegistrationNo.trim() != "") {
                        if (response.FullName != null && response.FullName.trim() != "") {
                            $("#FullName").val(response.FullName.toUpperCase()).prop("readonly", true);
                        }
                        if (response.FathersName != null && response.FathersName.trim() != "") {
                            $("#FathersName").val(response.FathersName.toUpperCase()).prop("readonly", true);
                        }
                        if (response.MothersName != null && response.MothersName.trim() != "") {
                            $("#MothersName").val(response.MothersName.toUpperCase()).prop("readonly", true);
                        }
                        if (response.DisplayDOBAs != null && response.DisplayDOBAs.trim() != "") {
                            $("#EnteredDOB").val(response.DisplayDOBAs.toUpperCase()).prop("readonly", true);
                        }
                        if (response.DisplayDOBAs != null && response.Gender.trim() != "") {
                            $("#Gender").val(response.Gender.toUpperCase());
                        }
                        if (response.Session != null && response.Session.trim() != "") {
                            $("#12TH").find('[id$="__Session"]').val(response.Session.toUpperCase());
                        }
                        if (response.YearOfPassing != null && response.YearOfPassing != 0) {
                            $("#12TH").find('[id$="__Year"]').val(response.YearOfPassing).prop("readonly", true);
                        }
                        if (response.RollNo != null && response.RollNo.trim() != "") {
                            $("#12TH").find('[id$="__RollNo"]').val(response.RollNo.toUpperCase()).prop("readonly", true);
                        }
                        if (response.MaxMarks != null && response.MaxMarks != 0) {
                            $("#12TH").find('[id$="__MaxMarks"]').val(response.MaxMarks).prop("readonly", true);
                        }
                        if (response.MarksObt != null && response.MarksObt != 0) {
                            $("#12TH").find('[id$="__MarksObt"]').val(response.MarksObt).prop("readonly", true);
                        }
                    }
                    $("#Jspleasewait").hide(150);
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    removeBoardData();
                    $("#Jspleasewait").hide(150);
                    alert('status code: ' + jqXHR.status + '\n We are unable to fetch your Board details ,\nplease check your internet connection or refresh and try again.');
                }
            });
        }
        else {
            removeBoardData();
            $("#Jspleasewait").hide(150);
        }
    });

    function removeBoardData() {
        $("#FullName").val('').prop("readonly", false);
        $("#FathersName").val('').prop("readonly", false);
        $("#MothersName").val('').prop("readonly", false);
        $("#EnteredDOB").val('').prop("readonly", false);
        $("#Gender").val('');
        $("#12TH").find('[id$="__Session"]').val('');
        $("#12TH").find('[id$="__Year"]').val('').prop("readonly", false);
        $("#12TH").find('[id$="__RollNo"]').val('').prop("readonly", false);
        $("#12TH").find('[id$="__MaxMarks"]').val('').prop("readonly", false);
        $("#12TH").find('[id$="__MarksObt"]').val('').prop("readonly", false);
    }

    $("#GetColleges").change(function () {
        var CourseCode = $("#GetColleges").val();
        var CourseText = $("#GetColleges option:selected").text();
        if (CourseCode != "") {
            $.ajax({
                url: "/Registration/GetColleges",
                type: "POST",
                data: { CourseID: CourseCode },
                dataType: "json",
                traditional: true,
                success: function (response) {
                    if (response != "") {
                        var table = '<br/><span style="font-size:18px">' + CourseText + ' is Provided in the following Colleges :</span> <br/><table style="width:100%;margin:0;">';
                        $("#showColleges").empty();
                        $.each(response, function (index, value) {
                            table += "<tr style='border:1px solid grey;'><th style='padding:10px;'>" + Number(index + 1) + ". " + value + "</th></tr>";
                        });
                        $("#showColleges").html(table + "</table>");
                    } else {
                        $("#showColleges").empty();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n We are unable to connect to server ,\nplease check your internet connection or refresh and try again.');
                }
            });
        }
        else {
            $("#showColleges").empty();
        }
    });
});