
$(document).ready(function () {
    $("#btnSubmitView").click(function () {
        var url = "/CUSrinagarAdminPanel/" + 'Transcript/PrintTranscript' + "?";
        if ($("#Course").val() == "" || $("#Course").val() == null) {
            alert("Course is required");
            return;}
        SearchBySubmit(url);
    });

    $(document).on('click', '.jsValidated', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "Transcript/Validated";
        print_Validate(_url, student_id, $element);
    });

    $(document).on('click', '.jsPrinted', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "Transcript/Printed";
        print_Validate(_url, student_id, $element);
    });

    $(document).on('click', '.jsHandedOver', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "Transcript/HandedOver";
        print_Validate(_url, student_id, $element);
    });

    $(document).on('click', '.jsIsValidated', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "Transcript/Validated";
        print_Validates(_url, student_id, $element);
    });

    $(document).on('click', '.jsIsPrinted', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "Transcript/Printed";
        print_Validates(_url, student_id, $element);
    });

    $(document).on('click', '.jsIsHandedOver', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "Transcript/HandedOver";
        print_Validates(_url, student_id, $element);
    });

});

function print_Validate(_url, student_id, $element) {
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        async: false,
        data: { Student_ID: student_id, PrintProgramme: $("#PrintProgramme").val() },
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                $element.closest('tr').addClass('success');
            } else {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}
function print_Validates(_url, student_id, $element) {
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        async: false,
        data: { Student_ID: student_id, SemesterTo: $("#SemesterTo").val(), Course_ID: $element.attr("data-courseid") },
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                $element.closest('tr').addClass('success');
            } else {
                showErrorMessage(responseData.ErrorMessage);
            }
        },
        error: function (xhr, error, msg) {
            showErrorMessage(msg);
        }
    });
}