
$(document).ready(function () {
    $("#btnSubmitView").click(function () {
        var url = "/CUSrinagarAdminPanel/" + 'DegreeCertificate/PrintDegreeCertificate' + "?";
        SearchBySubmit(url);
    });

    $(document).on('click', '.jsValidated', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificateEng/Validated";
        print_Validate(_url, student_id, $element);
    });

    $(document).on('click', '.jsPrinted', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificate/Printed";
        print_Validate(_url, student_id, $element);
    });

    $(document).on('click', '.jsDuplicate', function () {
        var $element = $(this);
        var student_id = $element.val();
        var duplicatelbl = $element.attr("data-dno");
        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificate/Duplicate";
        DuplicaDegree(_url, student_id, duplicatelbl, $element);
    });

    $(document).on('click', '.jsHandedOver', function () {
        var $element = $(this);
        var student_id = $element.val();
        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificate/HandedOver";
        print_Validate(_url, student_id, $element);
    });


    $(document).on('click', '.jsShowDegreeCertificateDialog', function () {
        $('tr').removeClass('active-deg-cert-row');
        $(this).closest('tr').addClass('active-deg-cert-row');
        $("#jsDegreeCertificateDialog").modal('show');
    });
    $(document).on('change', '#jsDegreeCertificateDialog :input.form-control', function () {
        var _CommonDispatchNumber = $("#CommonDispatchNumber").val();
        var _StartingIssueNumber = ($("#StartingIssueNumber").val());
        //var _StartingSerialNumber = ($("#StartingSerialNumber").val());
        $('#DispatchNumberView').html(String(_CommonDispatchNumber + '/' + _StartingIssueNumber + '/SERIAL_NUMBER').toUpperCase());// + _StartingSerialNumber).toUpperCase()
    });

    $(document).on('click', '#jsDegreeCertificateBtn', function () {
        var _CommonDispatchNumber = $("#CommonDispatchNumber").val();
        var _StartingIssueNumber = parseInt($("#StartingIssueNumber").val()) || 0;
        //var _StartingSerialNumber = parseInt($("#StartingSerialNumber").val()) || 0;

        if (isNullOrEmpty(_CommonDispatchNumber) || _StartingIssueNumber <= 0) {
            showErrorMessage('Please enter details correctly.');
            return;
        }
        var $tr = $('.active-deg-cert-row');
        var student_id = $tr.find('.jsEntity_ID').val();
        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificate/Create";

        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { Student_ID: student_id, CommonDispatchNumber: _CommonDispatchNumber, StartingIssueNumber: _StartingIssueNumber, PrintProgramme: $("#PrintProgramme").val() },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                    $tr.addClass('success');
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }, complete: function () {
                $("#jsDegreeCertificateDialog").modal('hide');
            }
        });
    });


    

    $(document).on('click', '#jsALLDegreeCertificateEngBtn', function () {
        showLoader();
        var _Batch = $("#Batch").val();
        var list = [];
        $('tr').find("input:checkbox:checked").each(function () {
            if ($(this).val() != "on") { list.push($(this).val()); }
        });

        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificateEng/CreateALL";

        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { Student_ID: list, Batch: _Batch, PrintProgramme: $("#PrintProgramme").val() },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                    $tr.addClass('success');
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }, complete: function () {
                hideLoader();
            }
        });
    });

});



$(document).on('click', '#jsALLPrintedBtn', function () {
    showLoader();
    var list = [];
    $('tr').find("input:checkbox:checked").each(function () {
        if ($(this).val() != "on") { list.push($(this).val()); }
    });

    if (list.length <= 0) {
        alert("No student selected");
        hideLoader();
        return;
    } else {

        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificate/MarkPrintedAll";

        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { Student_IDs: list },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                    hideLoader();
                    $("body").find(".js-search").trigger("click");
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }, complete: function () {
                hideLoader();
            }
        });
    }
});


$(document).on('click', '#jsALLHandedOverBtn', function () {
    showLoader();
    var list = [];
    $('tr').find("input:checkbox:checked").each(function () {
        if ($(this).val() != "on") { list.push($(this).val()); }
    });

    if (list.length <= 0) {
        alert("No student selected");
        hideLoader();
        return;
    } else {

        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificate/MarkHandedOverAll";

        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { Student_IDs: list },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                    hideLoader();
                    $("body").find(".js-search").trigger("click");
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }, complete: function () {
                hideLoader();
            }
        });
    }
});


$(document).on('click', '#jsALLValidatedBtn', function () {
    showLoader();
    var list = [];
    $('tr').find("input:checkbox:checked").each(function () {
        if ($(this).val() != "on") { list.push($(this).val()); }
    });

    if (list.length <= 0) {
        alert("No student selected");
        hideLoader();
        return;
    } else {

        var _url = "/CUSrinagarAdminPanel/" + "DegreeCertificate/MarkAllGood";

        $.ajax({
            url: _url,
            type: "POST",
            datatype: "Json",
            async: false,
            data: { Student_IDs: list },
            success: function (responseData) {
                if (responseData.IsSuccess) {
                    showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                    hideLoader();
                    $("body").find(".js-search").trigger("click");
                } else {
                    showErrorMessage(responseData.ErrorMessage);
                }
            },
            error: function (xhr, error, msg) {
                showErrorMessage(msg);
            }, complete: function () {
                hideLoader();
            }
        });
    }
});



function DuplicaDegree(_url, student_id, duplicatelbl, $element) {
    $.ajax({
        url: _url,
        type: "POST",
        datatype: "Json",
        async: false,
        data: { Student_ID: student_id, DuplicateLbl: duplicatelbl, PrintProgramme: $("#PrintProgramme").val() },
        success: function (responseData) {
            if (responseData.IsSuccess) {
                showSuccessAlertMessage(responseData.NumberOfRecordsEffected + " record updated successfully.", 300);
                $("body").find(".js-search").trigger("click");
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
                $("body").find(".js-search").trigger("click");
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
