$(document).ready(function () {
    getSubjectRegion();
    getDefaultParams();
    getCoursePoints();

    $("#js-release-student").click(function () {
        var msg = '<h4>Are you sure you want to delete this student from your college ?</h4>';
        msg += '<h5>Once deleted this student, corresponding student has to reassign all its subject combination details</h5>';
        showConfirmationDialog(msg);
        $('#confirmationDialog').off('click.confirmation', 'button').on('click.confirmation', 'button', function () {
            var $btn = $(this);
            hideConfirmationDialog();
            if ($btn.data('response') == 'yes') {
                var _student_id = $("[id$=Student_ID]").val()
                var _printProgramme = $("#printProg option:selected").val();
                var semester = parseInt($("#sem").find('option:selected').val()) || 0;
                DeleteStudentCombinations(_student_id, _printProgramme, semester);
            }
        });
    });


    function DeleteStudentCombinations(_student_id, _printProgramme, semester) {
        var _url = getBaseUrlCollege() + "AssignCombination/ReleaseStudent";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { Student_ID: _student_id, printProgramme: _printProgramme, Semester: semester },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessMessage(responseData.NumberOfRecordsEffected + ' deleted successfully.');
                    $("#js-release-student").hide();
                    $("#jsEditLinkContainer").hide();
                    var fieldInput = $('#Form_RegistrationNumber');
                    var fldLength = fieldInput.val().length;
                    fieldInput.focus();
                    fieldInput[0].setSelectionRange(fldLength, fldLength);
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }
        });
    }



    $(document).on("change", '.jsCourseDDL', function () {
        //if ($("#CourseText").length == 0) return true;
        getCoursePoints();
        getDefaultParams();
        getSubjectRegion();
    });

    function getDefaultParams() {
        var _Course_ID = $(".jsCourseDDL option:selected").val();
        var semester = $("#sem option:selected").val();
        var batch = $("#Batch").val();
        var _url = getBaseUrlCollege() + "AssignCombination/Get";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            data: { Course_ID: _Course_ID, Semester: semester, Batch: batch },
            success: function (data) {
                if (data != null && isNullOrEmpty($("#OfflinePaymentAmount").val())) {
                    $("#OfflinePaymentAmount").val(data.DefaultPaymentAmount);
                    $("#BankPaymentDate").val(data.GetDefaultPaymentDate);
                } else {
                    //$("#OfflinePaymentAmount").html('');
                    //$("#BankPaymentDate").html('');
                }
            }
        });
    }
    function getCoursePoints() {
        var _Course_ID = $(".jsCourseDDL option:selected").val();
        var _Student_ID = $("[name='model.Student_ID']").val();
        var _url = getBaseUrlCollege() + "AssignCombination/GetCoursePointsPG";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            data: { Course_ID: _Course_ID, Student_ID: _Student_ID },
            success: function (data) {
                if (data != null) {
                    $("#CourseText").html(data.CourseName);
                    $("#CourseValue").html(data.SubjectEntrancePoints);
                } else {
                    $("#CourseText").html('');
                    $("#CourseValue").html('');
                }
            }
        });
    }

    function getSubjectRegion() {
        showLoader();
        var _Course_ID = $(".jsCourseDDL option:selected").val();
        var semester = $("#sem option:selected").val();
        var formno = $("#Form_RegistrationNumber").val();
        var batch = $("#Batch").val();
        var _url = getBaseUrlCollege() + "AssignCombination/GetPGView";
        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            data: { Course_ID: _Course_ID, Semester: semester, Batch: batch, Form_RegistrationNumber: formno },
            success: function (data) {
                hideLoader();
                $("#SubjectsRegionDiv").html(data);
            }
        });
    }
});