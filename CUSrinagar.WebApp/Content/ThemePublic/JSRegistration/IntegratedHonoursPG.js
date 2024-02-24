window.oncontextmenu = function () {
    return false;
}

$(document).ready(function ($) {
    $("body").css("background-color", "lightgray");
    $("#CaptchaInputText").attr("maxlength", "10");
    $("input[type='text']").attr("autocomplete", "off");
    $(".sCGPA").each(function () {
        if ($.trim($(this).val()) === '') {
            $(this).prop("disabled", true);
        } else {
            $(this).prop("disabled", false);
        }
    });

    //Status or job form
    $(document).on('submit', '#StdRegistration', function (e) {
        var isLateralEntry = $.trim($("#IsLateralEntry").val()).toLowerCase();
        var ChkCount = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;

        if ($("#prevf").attr("value") !== "#jspreview") {
            if ($(this).valid()) {
                var invalidPrefNumber = false;
                var subjectCGPACount = 0;
                //var overAllCGPA = $("#StdRegistration").find('[id$="__OverAllCGPA"]').val();

                //if ($.trim(overAllCGPA) == '' || Number(overAllCGPA) <= 0) {
                //    alert("Please Enter Over All CGPA Points for Graduation");
                //    $("#StdRegistration").find('[id$="__OverAllCGPA"]').focus();
                //    $("#Jspleasewait").hide();
                //    return false;
                //}

                var prefCount = 0;
                var dubplicatepref = false;
                var prevValu = '';
                $("#StdRegistration").find('[id$="__Preference"]').each(function () {
                    if ($.trim($(this).val()) != '') {
                        prefCount = prefCount + 1;
                        if (prevValu === $.trim($(this).val()) && dubplicatepref === false) {
                            dubplicatepref = true;
                        }
                        prevValu = $.trim($(this).val());
                    }
                });

                $("#StdRegistration .sCGPA[disabled!='disabled']").each(function () {
                    if ($.trim($(this).val()) != '') {
                        subjectCGPACount = subjectCGPACount + 1;
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
                } else if (isLateralEntry == "true" && Number(prefCount) < Number(ChkCount)) {
                    alert("Please select preference(s) for all your selected course(s)");
                    $("#Jspleasewait").hide();
                    return false;
                } else if (dubplicatepref) {
                    alert("Duplicate preference is not allowed");
                    $("#Jspleasewait").hide();
                    return false;
                }

                if (Number(subjectCGPACount) < Number(ChkCount)) {
                    alert("Please Enter CGPA Points for all your selected course(s)");
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
                var invalidPrefNumberp = false;
                var ChkCountp = $("#StdRegistration").find('[id$="__IsClicked"]:checked').length;

                var overAllCGPA = $("#StdRegistration").find('[id$="__OverAllCGPA"]').val();

                //if ($.trim(overAllCGPA) == '' || Number(overAllCGPA) <= 0) {
                //    alert("Please Enter Over All CGPA Points for Graduation");
                //    $("#Jspleasewait").hide();
                //    return false;
                //}
                var prefCount = 0;
                var dubplicatepref = false;
                var prevValu = '';
                $("#StdRegistration").find('[id$="__Preference"]').each(function () {
                    if ($.trim($(this).val()) != '') {
                        prefCount = prefCount + 1;
                        if (prevValu === $.trim($(this).val()) && dubplicatepref === false) {
                            dubplicatepref = true;
                        }
                        prevValu = $.trim($(this).val());
                    }
                });

                var subjectCGPACount = 0;

                $("#StdRegistration .sCGPA[disabled!='disabled']").each(function () {
                    if ($.trim($(this).val()) != '') {
                        subjectCGPACount = subjectCGPACount + 1;
                    }
                });

                if (invalidPrefNumberp) {
                    $("#Jspleasewait").hide();
                    return false;
                }
                if (Number(ChkCountp) <= 0) {
                    alert("Please select course(s)");
                    $("#Jspleasewait").hide();
                    return false;
                } else if (isLateralEntry == "true" && Number(prefCount) < Number(ChkCount)) {
                    alert("Please select preference(s) for all your selected course(s)");
                    $("#Jspleasewait").hide();
                    return false;
                } else if (dubplicatepref) {
                    alert("Duplicate preference is not allowed");
                    $("#Jspleasewait").hide();
                    return false;
                }


                if (Number(subjectCGPACount) < Number(ChkCountp)) {
                    alert("Please Enter CGPA Points for all your selected course(s)");
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
        var graduationFee = 50;
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
        var count = allChecked.length;

        if (allChecked.length > 1) {
            $(".JSCourseClk:not(:checked)").prop("disabled", true);
        } else {
            $(".JSCourseClk:not(:checked)").prop("disabled", false);
        }


        if (count > 0) {
            if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")) {
                basicFee = (basicFee - PercourseFee) + graduationFee;
            }
            else if (count > 1 || (count === 1 && !$(this).is(":Checked"))) {
                if (checkedCourse.toLowerCase() !== 'a3ee7f98-7b82-4d95-a2c0-faba7a18240e') {
                    basicFee = CalculateFee(PercourseFee, basicFee, checkedCourse, $(this).is(":Checked"));
                } else {
                    basicFee = $("#TotalFee").val();
                }
            }
            if (IsNotJKResident) {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                var AdditionalFeeForNonJK = Number($("#AdditionalFeeForNonJK").val()) > 0 ? "+" + $("#AdditionalFeeForNonJK").val() : "";
                $("#Jsfee").text("Total Fee Rs : " + basicFee + AdditionalFeeForNonJK);

                if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")) {
                    $("#TotalFee").val(basicFee - graduationFee);
                } else {
                    $("#TotalFee").val(basicFee);
                }
            } else {
                $("#Jsfee").removeClass("quadrat").addClass("quadrat");
                $("#Jsfee").text("Total Fee Rs : " + basicFee);
                if (count === 1 && $('input[dataid="a3ee7f98-7b82-4d95-a2c0-faba7a18240e"]').is(":checked")) {
                    $("#TotalFee").val(basicFee - graduationFee);
                } else {
                    $("#TotalFee").val(basicFee);
                }
            }

        } else {
            $("#Jsfee").removeClass("quadrat");
            $("#Jsfee").text("");
            $("#TotalFee").val(0);
        }
    });


    function CalculateFee(PercourseFee, basicFee, checkedCourse, isChecked) {

        var arrayE = ['a7cb49e3-6e17-41d5-a696-62d8c5a8af1b',//HE
            '653aa86c-4900-484e-b566-ef49fd4e8313'];//IE
        var arrayC = ['c490c5af-95b6-407a-ae85-668a5fd9e277',//HC
            '58144c9c-319b-4aa1-b335-bf77b0fd1c24'];//IC
        var arrayH = ['9702e8ec-5e1e-4e46-8fcb-26f996c898d0',//HH
            '8368eeef-8f08-4077-aabe-48b348c1f85b'];//IH

        var arrayBCA = ['32996ce4-5613-4a2d-a565-030390d7f496',//mca
            '9b15f7e4-967d-453f-bee8-8b624fe6d0df'];//BCA
        var arrayBBA = ['dc9595e4-b4dc-41e3-aeb9-446c0b990bcf',//mba
            '4e7a4b78-b87c-4666-a517-069656d55281'];//BBA

        basicFee = isChecked ? Number($("#TotalFee").val()) + PercourseFee : Number($("#TotalFee").val()) - PercourseFee;
        if (jQuery.inArray(checkedCourse, arrayE) > -1) {
            var parallelValue = arrayE.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayE[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayC) > -1) {
            var parallelValue = arrayC.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayC[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayH) > -1) {
            var parallelValue = arrayH.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayH[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayBCA) > -1) {
            var parallelValue = arrayBCA.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayBCA[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        } else if (jQuery.inArray(checkedCourse, arrayBBA) > -1) {
            var parallelValue = arrayBBA.indexOf(checkedCourse) == 0 ? 1 : 0;
            if (($("input[dataid='" + arrayBBA[parallelValue] + "']").is(":Checked"))) {
                basicFee = isChecked ? basicFee - PercourseFee : basicFee + PercourseFee;
            }
        }
        return basicFee;
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
                            $("#Gender").val(response.Gender.toUpperCase());//.change();
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
    }

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
            $("#GRADUATION,#DiplomaBSc").css("display", "none");
            if ($("#Category").val() == "OM") {
                $(".CertificateSection").hide(300);
            }
            $(".MCSection").hide();
        } else {
            $("#GRADUATION,#DiplomaBSc").css("display", "");
            $(".CertificateSection").show(300);
            $(".MCSection").show();
        }
    }

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