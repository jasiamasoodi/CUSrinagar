$(document).ready(function ($) {
    var _baseURL = null;
    if (!isNullOrEmpty($("#collegebaseurl").val()) && $("#collegebaseurl").val().length > 0) {
        _baseURL = "CUCollegeAdminPanel";
    } else {
        _baseURL = "CUSrinagarAdminPanel";
    }

    //Apply fresh proceed confirmation 
    $("#CombinationForm").submit(function (e) {
        if ($(this).valid()) {
            var ans = confirm("Are you sure, you want to save this Combination ?");
            if (ans) {
                $('body').find("input[id=Save]").val("Saving...").prop("disabled", true);
                $('body').find("button[id=addSubject]").prop("disabled", true);
                return true;
            } else {
                $('body').find("input[id=Save]").val("Save").prop("disabled", false);
                $('body').find("button[id=addSubject]").prop("disabled", false);
                return false;
            }
        } else {
            $('body').find("input[id=Save]").val("Save").prop("disabled", false);
            $('body').find("button[id=addSubject]").prop("disabled", false);
            return false;
        }
    });
    $("#Semester").change(function () {
        {
            $('#Course_ID').val('').trigger('chosen:updated');
            $("#jssubjects").empty();
        }
    });
    $("body").on("change", "#Course_ID", function () {
        var Course_ID = $("#Course_ID").val();
        var Semester = $("#Semester").val();
        if (Course_ID !== "" && Semester !== "") {
            $("#jsstatus").show();
            $(this).prop("disabled", true);
            $.ajax({
                url: "/" + _baseURL +"/Combination/GetSubjectsOf",
                type: "POST",
                data: { Course_ID: Course_ID, Semester: Semester, College_ID:$("#College_ID").val() },
                dataType: "html",
                traditional: true,
                success: function (response) {
                    $("#jssubjects").empty().html(response);
                    $("#jsstatus").hide();
                    $("#Course_ID").prop("disabled", false);
                    //Remove current form validation information
                    $("#CombinationForm").removeData("validator").removeData("unobtrusiveValidation");
                    //Parse the form again
                    $.validator.unobtrusive.parse("#CombinationForm");
                    ChosenStyle();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $("#Course_ID").prop("disabled", false);
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                    $("#jsstatus").hide();
                }
            });
        }
        else {
            $("#jsstatus").hide();
        }
    });

    $("body").on("click", "#addSubject", function () {
        var indexLevel = Number($("#IndexLevel").val());
        var RmIndexLevel = Number($("#RmIndexLevel").val());
        var MaxLevel = Number($("#CombinationHelper_MaxSubjectsAllowed").val());
        if (RmIndexLevel === MaxLevel) {
            alert("Maximum allowed subjects in a combination is " + MaxLevel);
            return;
        }
        var options = $('#helperDDL').html();

        var sno = (indexLevel + 1);
        //jssubjects
        var newSubject = '<div class="form-group rm' + indexLevel + '">' +
            '<label class="col-sm-3 control-label no-padding-right">' +
            '<label for="CombinationHelper_FinalCombinations_' + indexLevel + '__Subject_ID">Subject </label> ' + sno + ' <span class="red"> *</span>' +
            '</label>' +
            '<div class="col-sm-6">' +
            '<span class="red">' + '<span class="field-validation-valid" data-valmsg-for="CombinationHelper.FinalCombinations[' + indexLevel + '].Subject_ID" data-valmsg-replace="true">' + '</span>' + '</span>' +
            '<select class="chosen-select form-control valid" data-val="true" data-val-required=" Required" id="CombinationHelper_FinalCombinations_' + indexLevel + '__Subject_ID" name="CombinationHelper.FinalCombinations[' + indexLevel + '].Subject_ID" aria-required="true" aria-describedby="CombinationHelper_FinalCombinations_' + indexLevel + '__Subject_ID-error" aria-invalid="false">' +
            options +
            '</select>' +
            '</div>' +
            '<a href="javascript:void(0);" class="red jsRemove" dataIndex="' + indexLevel + '">Remove</a></div >';
        $("#jsExtraSubj").append(newSubject);
        $("#IndexLevel").val(Number(indexLevel + 1));
        $("#RmIndexLevel").val(Number(RmIndexLevel + 1));
        //Remove current form validation information
        $("#CombinationForm").removeData("validator").removeData("unobtrusiveValidation");
        //Parse the form again
        $.validator.unobtrusive.parse("#CombinationForm");
        $('select').find('option').prop('disabled', false);
        $('select').each(function () {
            $('select').not(this).find('option[value="' + this.value + '"]').prop('disabled', true);
        });
        ChosenStyle();
    });

    $("body").on("click", ".jsRemove", function () {
        var toRemove = '.rm' + $(this).attr("dataIndex");
        $(toRemove).remove();
        $("#RmIndexLevel").val(Number(Number($("#RmIndexLevel").val()) - 1));
        $('select').find('option').prop('disabled', false);
        $('select').each(function () {
            $('select').not(this).find('option[value="' + this.value + '"]').prop('disabled', true);
        });
    });
    $('body').on('change mouseover focus', '.DDL', function () {
        if ($(this).val() === "")
            return;
        $('select').find('option').prop('disabled', false);
        $('select').each(function () {
            $('select').not(this).find('option[value="' + this.value + '"]').prop('disabled', true);
        });

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
                    $("#jssubjects").empty();
                     ChosenStyle();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    alert('status code: ' + jqXHR.status + '\n Please check your internet connection or refresh and try again.');
                }
            });
        }

    });
});