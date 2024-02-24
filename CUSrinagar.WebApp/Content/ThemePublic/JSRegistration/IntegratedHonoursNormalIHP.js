window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    //onGenderChange($("#Gender").val());
    $("body").css("background-color", "lightgray");
    $("#CaptchaInputText").attr("maxlength", "10");
    $("input[type='text']").attr("autocomplete", "off");
    $("#StdRegistration").find('[id$="__Preference"]').each(function () {
        if ($(this).val() === '')
            $(this).prop("disabled", true);
    });
    //status or job form

    $(document).on('submit', '#StdRegistration', function (e) {
        if ($("#prevf").attr("value") !== "#jspreview") {
            if ($(this).valid()) {
                var invalidPrefNumber = false;
                var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;
                
                    var prefCount = 0;
                    $("#StdRegistration").find('[id$="__Preference"]').each(function () {
                        if ($.trim($(this).val()) != '') {
                            if (Number($.trim($(this).val())) > ChkCount) {
                                alert("You have selected " + $(this).val() + " as one of your perferences which is invalid as per your selected course(s)");
                                $("#Jspleasewait").hide();
                                invalidPrefNumber = true;
                                return false;
                            }
                            prefCount = prefCount + 1;
                        }
                    });
                

                if (invalidPrefNumber) {
                    $("#Jspleasewait").hide();
                    return false;
                }
                if (Number(ChkCount) <= 0) {
                    alert("Please select course(s)");
                    $("#Jspleasewait").hide();
                    return false;
                } else if ( Number(prefCount) < Number(ChkCount)) {
                    alert("Please select preference(s) for all your selected course(s)");
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
        } else if ($("#prevf").attr("value") === "#jspreview") {

            if ($(this).valid()) {
                var invalidPrefNumber = false;
                var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;

                var prefCount = 0;
                $("#StdRegistration").find('[id$="__Preference"]').each(function () {
                    if ($.trim($(this).val()) != '') {
                        if (Number($.trim($(this).val())) > ChkCount) {
                            alert("You have selected " + $(this).val() + " as one of your perferences which is invalid as per your selected course(s)");
                            $("#Jspleasewait").hide();
                            invalidPrefNumber = true;
                            return false;
                        }
                        prefCount = prefCount + 1;
                    }
                });


                if (invalidPrefNumber) {
                    $("#Jspleasewait").hide();
                    return false;
                }
                if (Number(ChkCount) <= 0) {
                    alert("Please select course(s)");
                    $("#Jspleasewait").hide();
                    return false;
                } else if (Number(prefCount) < Number(ChkCount)) {
                    alert("Please select preference(s) for all your selected course(s)");
                    $("#Jspleasewait").hide();
                    return false;
                }

                return true;
            }
        }
    });

    $("#jspreview").focus(function () {
        $("#StdRegistration").attr("action", "/Registration/IntegratedHonorsPreview");
        $("#StdRegistration").attr("target", "_blank");
        $("#prevf").attr("value", "#jspreview");
    });

    $("#jspreview").mouseover(function () {
        $("#StdRegistration").attr("action", "/Registration/IntegratedHonorsPreview");
        $("#StdRegistration").attr("target", "_blank");
        $("#prevf").attr("value", "#jspreview");
    });

    $("#jsfinalSubmit").focus(function () {
        $("#StdRegistration").attr("action", "/Registration/IntegratedHonors");
        $("#StdRegistration").removeAttr("target");
        $("#prevf").attr("value", "");
    });
    $("#jsfinalSubmit").mouseover(function () {
        $("#StdRegistration").attr("action", "/Registration/IntegratedHonors");
        $("#StdRegistration").removeAttr("target");
        $("#prevf").attr("value", "");
    });




    //Show12th();
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
            $("#12TH,#Post-Graduation,#DiplomaBSc").css("display", "none");
            if ($("#Category").val() == "OM") {
                $(".CertificateSection").hide(300);
            }
            $(".MCSection").hide();
        } else {
            $("#12TH,#Post-Graduation,#DiplomaBSc").css("display", "");
            $(".CertificateSection").show(300);
            $(".MCSection").show();
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
        var DDLId = "." + $(this).attr("dataid");
        if ($(this).is(":Checked")) {
            $(DDLId).prop("disabled", false);
        } else {
            $(DDLId).val('').prop("disabled", true);
        }
        var IsNotJKResident = $("#StudentAddress_District").val() === "OTHER|OTHER";
        var PercourseFee = Number($("#PerCourse").val());
        var basicFee = Number($("#BasicFee").val()) + PercourseFee;

        var checkedCourse = $(this).attr("dataid").toLowerCase();
        var allChecked = $('[id$="__IsClicked"]:checked');

        var groupArray = [];
        allChecked.each(function () {
            var checkedCourse = $(this).attr("data-groupname").trim();
            if (jQuery.inArray(checkedCourse, groupArray) == -1) {
                groupArray.push($(this).attr("data-groupname"));
            }
        });
        var count = groupArray.length;

        if (count > 0) {
            if (!$(this).is(":Checked")) {
                basicFee = (basicFee + (PercourseFee * count));
                $("#TotalFee").val(basicFee);
            }
            else {
                basicFee = (basicFee + (PercourseFee * count));
                $("#TotalFee").val(basicFee);
            }
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
            $("#Jsfee").removeClass("quadrat");
            $("#Jsfee").text("");
            $("#TotalFee").val(0);
        }
    });


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
                            $("#Gender").val(response.Gender.toUpperCase());//.change();
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
        $("#Gender").val('');//.change();
        $("#12TH").find('[id$="__Session"]').val('');
        $("#12TH").find('[id$="__Year"]').val('').prop("readonly", false);
        $("#12TH").find('[id$="__RollNo"]').val('').prop("readonly", false);
        $("#12TH").find('[id$="__MaxMarks"]').val('').prop("readonly", false);
        $("#12TH").find('[id$="__MarksObt"]').val('').prop("readonly", false);
    }




    $('body').on('change', '[id$="__Preference"]', function () {
        if ($(this).val() == "")
            return;
        var clickedPref = $(this);
        var classRelation = $(this).attr("data-relationclass");
        $('#CourseSection [data-relationclass="' + classRelation + '"]').each(function (index, element) {
            if ($(element).attr("id") != clickedPref.attr("id")) {
                if ($(element).val() == clickedPref.val()) {
                    $(element).val("");
                }
            }
        });
    });



    $("#showorhideigsection").click(function () {
        if ($(this).is(":Checked")) {
            $("#jsfeeug").removeClass("quadrat").hide();
            $("#CourseSection").removeClass("hidden");
        } else {
            //remove checked courses
            var data = $("#StdRegistration").find('#CourseSection [id$="__IsClicked"]:checked');
            $.each(data, function (index, item) {
                $(item).click();
            });
            $("#CourseSection").addClass("hidden");
            if ($(".ugonly").is(":Checked")) {
                $("#jsfeeug").text($("#Jsfee").text()).addClass("quadrat").show();
            }
        }
    });

    $(".ugonly").click(function () {
        if ($("#showorhideigsection").is(":Checked")) {
            //dsfdsafsd
        } else {
            $("#jsfeeug").text($("#Jsfee").text()).addClass("quadrat").show();
        }
        if (!$(this).is(":Checked")) {
            $("#jsfeeug").text('').removeClass("quadrat").hide();
        }
    });

    $("#Category").change(function () {
        var categorySelected = $(this).val();
        if (categorySelected == "OM") {
            $(".CCSection").hide();
            if ($("#IsProvisional").val() == "True") {
                $(".CertificateSection").hide(300);
            }
        } else {
            $(".CertificateSection").show(300);
            $(".CCSection").show();
            $("label[for='CategoryCertificate']").text("Category Certificate of " + categorySelected);
        }
    });
});